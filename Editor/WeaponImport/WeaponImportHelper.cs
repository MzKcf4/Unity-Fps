using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Animancer;
using System.Text.RegularExpressions;

public class WeaponImportHelper
{
    enum AnimType 
    {
        ANIM_IDLE,
        ANIM_FIRE,
        ANIM_RELOAD,
        ANIM_DRAW,
        ANIM_RELOAD_PALLET_START,
        ANIM_RELOAD_PALLET_INSERT,
        ANIM_RELOAD_PALLET_END,
    }
    private static readonly string WEAPON_PREFAB_ASSET_PATH = "Assets/FpsResources/Prefabs/Weapons";

    private static List<string> fireSoundNameList = new List<string>()
    {
        "fire",
        "shoot",
        "shoot1",
        "fp"
    };

    private static Dictionary<AnimType, List<string>> dictAnimTypeToNameList = new Dictionary<AnimType, List<string>>()
    {
        { AnimType.ANIM_IDLE , new List<string>(){ "idle" , "idle1"  } },
        { AnimType.ANIM_FIRE , new List<string>(){ "fire" , "shoot" , "shoot1", "shoot_1" , "shoot_layer" } },
        { AnimType.ANIM_RELOAD , new List<string>(){ "reload" , "reload_layer"  } },
        { AnimType.ANIM_DRAW , new List<string>(){ "draw" , "deploy_layer" , "draw_first"  } },
        { AnimType.ANIM_RELOAD_PALLET_START , new List<string>(){ "reload_start"} },
        { AnimType.ANIM_RELOAD_PALLET_INSERT , new List<string>(){ "reload_insert"} },
        { AnimType.ANIM_RELOAD_PALLET_END , new List<string>(){ "reload_end"} },

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

        // ----- Create the weapon resource ----- //

        // ------- Read the qc file and gets all events ------ //
        List<QcSequenceEventInfo> sequenceEventInfoList = ParseQcFile(context.qcFileSystemPath);

        //  Find AnimationClips in /Animation Folder
        PopulateAnimationClipFromQc(sequenceEventInfoList, context);

        // ----------------Audio Clips------------------ //
        PopulateAudioClipInResource(sequenceEventInfoList, context);

        // ----------------Create prefab---------------- //
        CreateWeaponPrefab(context);
        
        EditorUtility.SetDirty(context.weaponResources);
        AssetDatabase.Refresh();
        Debug.Log("Weapon import for " + context.weaponName + " completed !");
    }

    private static void PopulatePath(Object selectedObject , WeaponImportContext context) 
    {
        context.weaponName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
        context.qcFileSystemPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderFileSystemPath = Path.GetDirectoryName(context.qcFileSystemPath);

        // string weaponResourceAssetPath = Path.Combine(context.modelFolderAssetPath, context.weaponName + ".asset");
        context.weaponResources = ScriptableObject.CreateInstance<WeaponResources>();
        string weaponResourceAssetPath = Path.Combine(context.modelFolderAssetPath, context.weaponName + ".asset");
        string name = AssetDatabase.GenerateUniqueAssetPath(weaponResourceAssetPath);
        AssetDatabase.CreateAsset(context.weaponResources, name);
        // (WeaponResources)AssetDatabase.LoadAssetAtPath(weaponResourceAssetPath, typeof(WeaponResources));

        string vModelFileName = context.weaponName + "_v" + ".fbx";
        context.vModelAssetPath = Path.Combine(context.modelFolderAssetPath, vModelFileName);

        string wModelFileName = context.weaponName + "_w" + ".fbx";
        context.wModelAssetPath = Path.Combine(context.modelFolderAssetPath, wModelFileName);
    }

    private static List<QcSequenceEventInfo> ParseQcFile(string qcFilePath)
    {
        List<QcSequenceEventInfo> sequenceEventInfoList = new List<QcSequenceEventInfo>();

        string[] fileLines = File.ReadAllLines(qcFilePath);
        for (int i = 0; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            if (line.Contains("$sequence"))
            {
                // Find the name of the sequence , inside the "";
                string sequenceName = Regex.Match(line, "\"([^\"]*)\"").ToString().Replace("\"", "");
                Debug.Log("Parsing " + sequenceName + " in QC file");

                QcSequenceEventInfo sequenceEventInfo = new QcSequenceEventInfo()
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

    private static int ParseQcSequence(string[] fileLines, int startLine, QcSequenceEventInfo sequenceEventInfo)
    {
        Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();

        // First , look for "event" line
        int lineParsed = 0;
        for (int i = startLine; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            // fps should indicates the end of sequence.
            if (line.Contains("fps"))
            {
                //  0    1
                // fps 42.5
                string[] fpsParts = line.Split(' ');
                sequenceEventInfo.fpsMultiplier = float.Parse(fpsParts[1]) / 24f;
                break;
            }
             
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

    private static void PopulateAnimationClipFromQc(List<QcSequenceEventInfo> sequenceEventInfoList, WeaponImportContext context)
    {
        string animationFolder = Path.Combine(context.modelFolderFileSystemPath, "Animations");
        DirectoryInfo dirInfo = new DirectoryInfo(animationFolder);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.anim", SearchOption.TopDirectoryOnly);
        foreach (FileInfo fileInfo in animationFileInfos)
        {
            // Populate EVERY animation files when it got events defined.
            string assetPath = Path.Combine(context.modelFolderAssetPath, "Animations", fileInfo.Name);
            Debug.Log("Handling animation events for " + fileInfo.Name);

            QcSequenceEventInfo sequenceEventInfo = GetRelatedSequenceEventInfo(assetPath, sequenceEventInfoList);
            if (sequenceEventInfo == null)
            {
                Debug.Log("Skipped " + fileInfo.Name);
                continue;
            }
                
            AnimationClip animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
            AnimationEvent[] createdEvents = CreateAnimationEvents(sequenceEventInfo);
            AnimationUtility.SetAnimationEvents(animationClip, createdEvents);

            // ----- Map to ClipTransitions --- //
            MapAnimClipToWeaponResource(Path.GetFileNameWithoutExtension(fileInfo.Name), animationClip, context.weaponResources, sequenceEventInfo);
        }
    }

    private static void MapAnimClipToWeaponResource(string fileName, AnimationClip clip, WeaponResources weaponResources, QcSequenceEventInfo sequenceEventInfo)
    {
        foreach (KeyValuePair<AnimType, List<string>> entry in dictAnimTypeToNameList)
        {
            AnimType animType = entry.Key;
            List<string> possibleNameList = entry.Value;

            if (!string.IsNullOrEmpty(possibleNameList.Find(str => fileName.EndsWith(str))))
            {
                if (animType == AnimType.ANIM_IDLE)
                    clip.wrapMode = WrapMode.Loop;

                ClipTransition clipTransitionToMap = null;
                if (AnimType.ANIM_DRAW == animType)
                    clipTransitionToMap = weaponResources.drawClip;
                else if (AnimType.ANIM_IDLE == animType)
                    clipTransitionToMap = weaponResources.idleClip;
                else if (AnimType.ANIM_FIRE == animType)
                    clipTransitionToMap = weaponResources.shootClip;
                else if (AnimType.ANIM_RELOAD == animType)
                    clipTransitionToMap = weaponResources.reloadClip;
                else if (AnimType.ANIM_RELOAD_PALLET_START == animType)
                    clipTransitionToMap = weaponResources.palletReload_StartClip;
                else if (AnimType.ANIM_RELOAD_PALLET_INSERT == animType)
                    clipTransitionToMap = weaponResources.palletReload_InsertClip;
                else if (AnimType.ANIM_RELOAD_PALLET_END == animType)
                    clipTransitionToMap = weaponResources.palletReload_EndClip;
                else
                    continue;

                clipTransitionToMap.Clip = clip;
                clipTransitionToMap.Speed = sequenceEventInfo.fpsMultiplier;
                
                /*
                if (AnimType.ANIM_DRAW == animType)
                    weaponResources.drawClip.Clip = clip;
                else if (AnimType.ANIM_IDLE == animType)
                    weaponResources.idleClip.Clip = clip;
                else if (AnimType.ANIM_FIRE == animType)
                    weaponResources.shootClip.Clip = clip;
                else if (AnimType.ANIM_RELOAD == animType)
                    weaponResources.reloadClip.Clip = clip;
                */
                Debug.Log("Mapped " + fileName + " to " + animType);
            } 
        }
    }

    private static QcSequenceEventInfo GetRelatedSequenceEventInfo(string animationClipName, List<QcSequenceEventInfo> sequenceEventInfoList)
    {
        foreach (QcSequenceEventInfo sequenceEventInfo in sequenceEventInfoList)
        {
            if (Path.GetFileNameWithoutExtension(animationClipName).EndsWith(sequenceEventInfo.sequenceName))
                return sequenceEventInfo;
        }
        return null;
    }

    private static AnimationEvent[] CreateAnimationEvents(QcSequenceEventInfo sequenceEventInfo) 
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

            Debug.Log("Added animation event " + soundName + " at frame " + frame);
            animationEventList.Add(evt);
        }

        return animationEventList.ToArray();
    }

    private static void PopulateAudioClipInResource(List<QcSequenceEventInfo> sequenceEventInfoList, WeaponImportContext context)
    {
        WeaponResources weaponResources = context.weaponResources;

        weaponResources.dictWeaponSounds = new Dictionary<string, AudioClip>();
        string soundFolder = Path.Combine(context.modelFolderFileSystemPath, "Sounds");
        DirectoryInfo soundDirInfo = new DirectoryInfo(soundFolder);
        FileInfo[] soundFileInfos = soundDirInfo.GetFiles("*.wav", SearchOption.TopDirectoryOnly);
        foreach (QcSequenceEventInfo sequenceEventInfo in sequenceEventInfoList)
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
        {
            bool found = false;
            foreach (FileInfo fileInfo in soundFileInfos)
            {
                if (!string.IsNullOrEmpty(fireSoundNameList.Find(str => fileInfo.Name.Contains(str))))
                {
                    found = true;
                    string assetPath = Path.Combine(context.modelFolderAssetPath, "Sounds", fileInfo.Name);
                    AudioClip clip = (AudioClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip));
                    weaponResources.dictWeaponSounds.Add("fire", clip);
                    break;
                }
            }
            if(!found)
                weaponResources.dictWeaponSounds.Add("fire", null);
        }
        

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
        Object.DestroyImmediate(vModelInstance);

        // ----- w model -------- //
        GameObject wModelFbx = (GameObject)AssetDatabase.LoadMainAssetAtPath(context.wModelAssetPath);
        GameObject wModelInstance = (GameObject)PrefabUtility.InstantiatePrefab(wModelFbx);
        wModelInstance.AddComponent(typeof(FpsWeaponWorldModel));
        string wModelSavePath = Path.Combine(WEAPON_PREFAB_ASSET_PATH, context.weaponName + "_w_variant.prefab");
        var wModelVariant = PrefabUtility.SaveAsPrefabAsset(wModelInstance, wModelSavePath);
        Object.DestroyImmediate(wModelInstance);

        // set prefab to weaponResource
        WeaponResources weaponResources = context.weaponResources;
        weaponResources.weaponViewPrefab = vModelVariant;
        weaponResources.weaponWorldPrefab = wModelVariant;
        weaponResources.weaponId = context.weaponName;
    }

    class QcSequenceEventInfo
    {
        public string sequenceName;
        public float fpsMultiplier;
        public Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();
        public Dictionary<string, AudioClip> dictNameToAudioClip = new Dictionary<string, AudioClip>();
    }
}
