
using Animancer;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class WeaponAnimationEventHelper
{
    private static readonly string WEAPON_PREFAB_ASSET_PATH = "Assets/FpsResources/Prefabs/Weapons";
    private static readonly string QC_SKELETON_PATTERN = "qc_skeleton_";

    private static List<string> fireSoundNameList = new List<string>()
    {
        "fire",
        "shoot",
        "shoot1",
        "fp"
    };

    [MenuItem("Assets/QC/1. Populate Animations found (Click on QC file)")]
    public static void PopulateWeaponResourceByQc()
    {
        Object selectedObject = Selection.activeObject;
        string ext = Path.GetFileName(AssetDatabase.GetAssetPath(selectedObject));
        if (!ext.Contains(".qc"))
        {
            Debug.LogError("Only .qc file is supported");
            return;
        }

        WeaponAnimationImportContext context = new WeaponAnimationImportContext();

        // ----- Initialize the paths ------------ //
        PopulatePath(selectedObject, context);

        // ------- Read the qc file and gets all events ------ //
        ParseQcFile(context);

        //  Find AnimationClips in /Animation Folder
        PopulateAnimationClipFromQc(context);

        // ----------------Audio Clips------------------ //
        // PopulateAudioClipInResource(context);

        // EditorUtility.SetDirty(context.weaponResources);
        AssetDatabase.Refresh();
        Debug.Log("Weapon animations for " + context.weaponName + " completed !");
    }

    private static void PopulatePath(Object selectedObject, WeaponAnimationImportContext context)
    {
        context.weaponName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
        context.qcFileSystemPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(selectedObject));
        context.modelFolderFileSystemPath = Path.GetDirectoryName(context.qcFileSystemPath);

        // context.weaponResources = ScriptableObject.CreateInstance<WeaponResources>();
        string weaponResourceAssetPath = Path.Combine(context.modelFolderAssetPath, context.weaponName + ".asset");
        string name = AssetDatabase.GenerateUniqueAssetPath(weaponResourceAssetPath);

        // AssetDatabase.CreateAsset(context.weaponResources, name);

        string vModelFileName = context.weaponName + "_v" + ".fbx";
        context.vModelAssetPath = Path.Combine(context.modelFolderAssetPath, vModelFileName);

        string wModelFileName = context.weaponName + "_w" + ".fbx";
        context.wModelAssetPath = Path.Combine(context.modelFolderAssetPath, wModelFileName);
    }

    private static void ParseQcFile(WeaponAnimationImportContext context)
    {
        string qcFilePath = context.qcFileSystemPath;

        string[] fileLines = File.ReadAllLines(qcFilePath);
        for (int i = 0; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            if (line.Contains("$attachment") && line.Contains("muzzle_flash"))
            {
                line = line.Replace("\"", "");
                WeaponModelAttachment attachment = new WeaponModelAttachment();
                //      0            1         2    3 4  5   6     7  8 9
                // $attachment "muzzle_flash" "gun" 0 1 29 rotate -90 0 0
                string[] parts = line.Split(' ');

                attachment.name = parts[1];
                attachment.attachTo = parts[2];
                attachment.offset = new Vector3(
                        float.Parse(parts[3]) / -100f,      // need to revert x
                        float.Parse(parts[4]) / 100f,
                        float.Parse(parts[5]) / 100f
                    );
                attachment.rotation = Quaternion.Euler(
                    float.Parse(parts[8]),
                    float.Parse(parts[7]) * -1,         // x in qc = -y in Unity
                    float.Parse(parts[9])
                    );
                context.weaponAttachmentList.Add(attachment);
                Debug.Log("Added attachment " + parts[1] + " to " + parts[2] + " with offset " + attachment.offset + " ; rotation " + attachment.rotation);
            }

            if (line.Contains("$sequence"))
            {
                // Find the name of the sequence , inside the "";
                string sequenceName = Regex.Match(line, "\"([^\"]*)\"").ToString().Replace("\"", "");
                Debug.Log("Parsing " + sequenceName + " in QC file");

                QcSequenceEventInfo sequenceEventInfo = new QcSequenceEventInfo()
                {
                    sequenceName = sequenceName
                };

                int lineParsed = ParseQcAnimationSequence(fileLines, i, sequenceEventInfo);
                i += lineParsed;

                string seqName = sequenceEventInfo.sequenceName.ToLower();
                // If the name is "_layer" , it's supplementory info 
                seqName = seqName.Replace("_layer", "");
                if (context.dictAnimEvent.ContainsKey(seqName))
                    context.dictAnimEvent[seqName].CopyFrom(sequenceEventInfo);
                else
                    context.dictAnimEvent.Add(seqName, sequenceEventInfo);
            }
        }
    }

    private static int ParseQcAnimationSequence(string[] fileLines, int startLine, QcSequenceEventInfo sequenceEventInfo)
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
            }
            // 'event 5004' means play sound
            else if (line.Contains("event 5004"))
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
            else if (line.Contains("}"))
            {
                if (line.Contains("{"))
                {
                    // Other types of events "{event XXX}", currently have no use}
                }
                else
                {
                    // End of whole sequence
                    lineParsed++;
                    break;
                }
            }

            lineParsed++;
        }

        sequenceEventInfo.dictFrameToSoundName = dictFrameToSoundName;

        return lineParsed;
    }

    private static void PopulateAnimationClipFromQc(WeaponAnimationImportContext context)
    {
        string animationFolder = Path.Combine(context.modelFolderFileSystemPath, "Animations");
        DirectoryInfo dirInfo = new DirectoryInfo(animationFolder);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.anim", SearchOption.TopDirectoryOnly);
        foreach (FileInfo fileInfo in animationFileInfos)
        {
            // Populate EVERY animation files when it got events defined.
            string assetPath = Path.Combine(context.modelFolderAssetPath, "Animations", fileInfo.Name);
            Debug.Log("Handling animation events for " + fileInfo.Name);

            QcSequenceEventInfo sequenceEventInfo = GetRelatedSequenceEventInfo(assetPath, context);
            if (sequenceEventInfo == null)
            {
                Debug.Log("Skipped " + fileInfo.Name);
                continue;
            }

            AnimationClip animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
            AnimationEvent[] createdEvents = CreateAnimationEvents(sequenceEventInfo);
            AnimationUtility.SetAnimationEvents(animationClip, createdEvents);
        }
    }

    private static QcSequenceEventInfo GetRelatedSequenceEventInfo(string animationClipName, WeaponAnimationImportContext context)
    {
        string clipFileNameNoExt = Path.GetFileNameWithoutExtension(animationClipName);
        // Animation file name is v_huntingrifle.qc_skeleton_sniper_draw
        int subStrIdx = clipFileNameNoExt.IndexOf(QC_SKELETON_PATTERN) + QC_SKELETON_PATTERN.Length;
        clipFileNameNoExt = clipFileNameNoExt.Substring(subStrIdx);
        clipFileNameNoExt = clipFileNameNoExt.ToLower();
        if (context.dictAnimEvent.ContainsKey(clipFileNameNoExt))
            return context.dictAnimEvent[clipFileNameNoExt];
        else
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

    private static void PopulateAudioClipInResource(WeaponAnimationImportContext context)
    {
        // List<QcSequenceEventInfo> sequenceEventInfoList = context.sequenceEventInfoList;
        List<QcSequenceEventInfo> sequenceEventInfoList = new List<QcSequenceEventInfo>(context.dictAnimEvent.Values);
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
                    // clip_in_1 -->  clipin1
                    string correctedName = fileInfo.Name.Replace("_", "");
                    bool contains = correctedName.IndexOf(soundName, System.StringComparison.OrdinalIgnoreCase) >= 0;
                    if (contains)
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
            if (!found)
                weaponResources.dictWeaponSounds.Add("fire", null);
        }
    }


}