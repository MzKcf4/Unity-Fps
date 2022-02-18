using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Animancer;
using System.Text.RegularExpressions;

public class WeaponImportHelper
{
    private static readonly string WEAPON_PREFAB_ASSET_PATH = "Assets/FpsResources/Prefabs/Weapons";
    private static HashSet<string> targetSequenceSet = new HashSet<string>()
    {
        "idle",
        "idle1",
        "shoot1",
        "fire",
        "draw",
        "reload",
        // L4D2
        "reload_layer",
        "deploy_layer",
        "shoot_layer"
    };

    [MenuItem("Assets/Auto populate weapon resource")]
    public static void PopulateWeaponResourceByQc()
    {
        Object selectedObject = Selection.activeObject;
        string ext = Path.GetFileName(AssetDatabase.GetAssetPath(selectedObject));
        if (!ext.Contains(".qc")) {
            Debug.LogError("Only .qc file is supported");
            return;
        }

        WeaponImportContext context = new WeaponImportContext();

        // ----- Initialize the paths ------------ //
        PopulatePath(selectedObject, context);

        // ------- Read the qc file and gets all events ------ //
        List<SequenceEventInfo> sequenceEventInfoList = ParseQcFile(context.qcFileSystemPath);

        //  Find AnimationClips in /Animation Folder
        PopulateAnimationClipFromQc(sequenceEventInfoList, context);

        // ----------------Audio Clips------------------ //
        PopulateAudioClipInResource(sequenceEventInfoList, context);

        // ----------------Create prefab---------------- //
        CreateWeaponPrefab(context);

        AssetDatabase.Refresh();
        Debug.Log("Weapon import for " + context.weaponName + " completed !");
    }

    private static void PopulatePath(Object selectedObject , WeaponImportContext context) 
    {
        context.weaponName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
        context.qcFileSystemPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderFileSystemPath = Path.GetDirectoryName(context.qcFileSystemPath);

        string weaponResourceAssetPath = Path.Combine(context.modelFolderAssetPath, context.weaponName + ".asset");
        context.weaponResources = (WeaponResources)AssetDatabase.LoadAssetAtPath(weaponResourceAssetPath, typeof(WeaponResources));

        string vModelFileName = context.weaponName + "_v" + ".fbx";
        context.vModelAssetPath = Path.Combine(context.modelFolderAssetPath, vModelFileName);

        string wModelFileName = context.weaponName + "_w" + ".fbx";
        context.wModelAssetPath = Path.Combine(context.modelFolderAssetPath, wModelFileName);
    }

    private static bool ExistsInTargetSet(string name)
    {
        foreach (string targetName in targetSequenceSet)
        {
            if (name.Contains(targetName))
                return true;
        }
        return false;
    }

    private static List<SequenceEventInfo> ParseQcFile(string qcFilePath)
    {
        List<SequenceEventInfo> sequenceEventInfoList = new List<SequenceEventInfo>();

        string[] fileLines = File.ReadAllLines(qcFilePath);
        for (int i = 0; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            if (line.Contains("$sequence"))
            {
                // Find the name of the sequence , inside the "";
                string sequenceName = Regex.Match(line, "\"([^\"]*)\"").ToString().Replace("\"", "");
                Debug.Log("Parsing " + sequenceName + " in QC file");

                // if (!IsTargetSequenceSet(sequenceName))
                // continue;

                SequenceEventInfo sequenceEventInfo = new SequenceEventInfo()
                {
                    sequenceName = sequenceName
                };

                int lineParsed = ParseQcSequence(fileLines, i, sequenceEventInfo);
                i += lineParsed;

                sequenceEventInfoList.Add(sequenceEventInfo);
            }
        }

        return sequenceEventInfoList;
    }

    private static bool IsTargetSequenceSet(string sequenceName)
    {
        foreach (string targetSequence in targetSequenceSet)
        {
            if (sequenceName.Contains(targetSequence))
                return true;
        }
        return false;
    }

    private static int ParseQcSequence(string[] fileLines, int startLine, SequenceEventInfo sequenceEventInfo)
    {
        Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();

        // First , look for "event" line
        int lineParsed = 0;
        for (int i = startLine; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            // fps should indicates the end of sequence.
            if (line.Contains("fps"))
                break;
             
            // 'event 5004' means play sound
            if (line.Contains("event 5004"))
            {
                // 0   1    2   3       4                5
                // { event 5004 15 "TFA_CSGO_AUG.Clipout }"
                string[] eventParts = line.Split(' ');

                int frame = int.Parse(eventParts[3]);
                string soundName = eventParts[4].Replace("\"", "").Split('.')[1].ToLower();
                dictFrameToSoundName.Add(frame, soundName);
                Debug.Log("Added " + frame + " : " + soundName);
            }
            else if (line.Contains("AE_CL_PLAYSOUND"))
            {
                // 0   1    2              3    4   5
                // { event AE_CL_PLAYSOUND 6 "bolt" }
                string[] eventParts = line.Split(' ');

                int frame = int.Parse(eventParts[3]);
                string soundName = eventParts[4].Replace("\"", "");
                dictFrameToSoundName.Add(frame, soundName);
                Debug.Log("Added " + frame + " : " + soundName);
            }

            lineParsed++;
        }

        sequenceEventInfo.dictFrameToSoundName = dictFrameToSoundName;

        return lineParsed;
    }

    private static void PopulateAnimationClipFromQc(List<SequenceEventInfo> sequenceEventInfoList, WeaponImportContext context)
    {
        string animationFolder = Path.Combine(context.modelFolderFileSystemPath, "Animations");
        DirectoryInfo dirInfo = new DirectoryInfo(animationFolder);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.anim", SearchOption.TopDirectoryOnly);
        foreach (FileInfo fileInfo in animationFileInfos)
        {
            // Populate EVERY animation files when it got events defined.
            string assetPath = Path.Combine(context.modelFolderAssetPath, "Animations", fileInfo.Name);
            Debug.Log("Handling animation events for " + fileInfo.Name);

            SequenceEventInfo sequenceEventInfo = GetRelatedSequenceEventInfo(assetPath, sequenceEventInfoList);
            if (sequenceEventInfo == null)
            {
                Debug.Log("Skipped " + fileInfo.Name);
                continue;
            }
                

            AnimationClip animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
            AnimationEvent[] createdEvents = CreateAnimationEvents(sequenceEventInfo);
            AnimationUtility.SetAnimationEvents(animationClip, createdEvents);

            // ----- Map to ClipTransitions --- //
            if (fileInfo.Name.Contains("idle"))
            {
                context.weaponResources.idleClip.Clip = animationClip;
            }
            else if (fileInfo.Name.Contains("reload"))
                context.weaponResources.reloadClip.Clip = animationClip;
            else if (fileInfo.Name.Contains("shoot"))
                context.weaponResources.shootClip.Clip = animationClip;
            else if (fileInfo.Name.Contains("draw"))
                context.weaponResources.drawClip.Clip = animationClip;
        }
    }

    private static SequenceEventInfo GetRelatedSequenceEventInfo(string animationClipName, List<SequenceEventInfo> sequenceEventInfoList)
    {
        foreach (SequenceEventInfo sequenceEventInfo in sequenceEventInfoList)
        {
            if (animationClipName.Contains(sequenceEventInfo.sequenceName))
                return sequenceEventInfo;
        }
        return null;
    }

    private static AnimationEvent[] CreateAnimationEvents(SequenceEventInfo sequenceEventInfo) 
    {
        List<AnimationEvent> animationEventList = new List<AnimationEvent>();
        foreach (KeyValuePair<int, string> entry in sequenceEventInfo.dictFrameToSoundName)
        {
            int frame = entry.Key;
            string soundName = entry.Value;

            AnimationEvent evt = new AnimationEvent();
            evt.time = frame / 24f;
            evt.functionName = "AniEvent_PlayWeaponSound";
            evt.stringParameter = soundName;

            animationEventList.Add(evt);
        }

        return animationEventList.ToArray();
    }

    private static void PopulateAudioClipInResource(List<SequenceEventInfo> sequenceEventInfoList, WeaponImportContext context)
    {
        WeaponResources weaponResources = context.weaponResources;

        weaponResources.dictWeaponSounds = new Dictionary<string, AudioClip>();
        string soundFolder = Path.Combine(context.modelFolderFileSystemPath, "Sounds");
        DirectoryInfo soundDirInfo = new DirectoryInfo(soundFolder);
        FileInfo[] soundFileInfos = soundDirInfo.GetFiles("*.wav", SearchOption.TopDirectoryOnly);
        foreach (SequenceEventInfo sequenceEventInfo in sequenceEventInfoList)
        {
            HashSet<string> soundNameSet = new HashSet<string>(sequenceEventInfo.dictFrameToSoundName.Values);
            foreach (string soundName in soundNameSet)
            {
                if (weaponResources.dictWeaponSounds.ContainsKey(soundName))
                    continue;

                bool found = false;
                foreach (FileInfo fileInfo in soundFileInfos)
                {
                    if (fileInfo.Name.Contains(soundName))
                    {
                        found = true;
                        string assetPath = Path.Combine(context.modelFolderAssetPath, "Sounds", fileInfo.Name);
                        AudioClip clip = (AudioClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip));
                        if (!weaponResources.dictWeaponSounds.ContainsKey(soundName))
                            weaponResources.dictWeaponSounds.Add(soundName, clip);
                    }
                }

                if (!found)
                {
                    Debug.LogWarning(soundName + " not found in sound files !");
                }
            }
        }

        // Manually adds a "fire" key to resource
        if (!weaponResources.dictWeaponSounds.ContainsKey("fire"))
            weaponResources.dictWeaponSounds.Add("fire", null);

    }

    private static void CreateWeaponPrefab(WeaponImportContext context)
    {
        // ------ v model ------- //
        GameObject vModelFbx = (GameObject)AssetDatabase.LoadMainAssetAtPath(context.vModelAssetPath);
        GameObject vModelInstance = (GameObject)PrefabUtility.InstantiatePrefab(vModelFbx);
        vModelInstance.AddComponent(typeof(FpsWeaponViewModel));
        Utils.ChangeLayerRecursively(vModelInstance, Constants.LAYER_FIRST_PERSON_VIEW, true);
        string vModelSavePath = Path.Combine(WEAPON_PREFAB_ASSET_PATH, context.weaponName + "_v_variant.prefab");
        var vModelVariant = PrefabUtility.SaveAsPrefabAsset(vModelInstance, vModelSavePath);

        // ----- w model -------- //
        GameObject wModelFbx = (GameObject)AssetDatabase.LoadMainAssetAtPath(context.wModelAssetPath);
        GameObject wModelInstance = (GameObject)PrefabUtility.InstantiatePrefab(wModelFbx);
        wModelInstance.AddComponent(typeof(FpsWeaponWorldModel));
        string wModelSavePath = Path.Combine(WEAPON_PREFAB_ASSET_PATH, context.weaponName + "_w_variant.prefab");
        var wModelVariant = PrefabUtility.SaveAsPrefabAsset(wModelInstance, wModelSavePath);

        // set prefab to weaponResource
        WeaponResources weaponResources = context.weaponResources;
        weaponResources.weaponViewPrefab = vModelVariant;
        weaponResources.weaponWorldPrefab = wModelVariant;
        weaponResources.weaponId = context.weaponName;
    }

    class SequenceEventInfo
    {
        public string sequenceName;
        public Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();
        public Dictionary<string, AudioClip> dictNameToAudioClip = new Dictionary<string, AudioClip>();
    }
}
