using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Animancer;
using System.Text.RegularExpressions;


public class WeaponImportHelper
{
    private static HashSet<string> targetSequenceSet = new HashSet<string>()
    {
        "idle",
        "idle1",
        "shoot1",
        "draw",
        "reload",
        // L4D2
        "reload_layer",
        "deploy_layer",
        "shoot_layer"
    };

    [MenuItem("Assets/Auto populate weapon resource")]
    public static void TestReadFile()
    {
        Object selectedObject = Selection.activeObject;
        
        // ----- Initialize the paths ------------ //
        string currentFolderAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
        string qcFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(selectedObject));

        string weaponResourceTemplateAssetPath = Path.Combine(currentFolderAssetPath, "newWeaponResources.asset");
        WeaponResources weaponResources = (WeaponResources)AssetDatabase.LoadAssetAtPath(weaponResourceTemplateAssetPath, typeof(WeaponResources));

        string currentFolderPath = Path.GetDirectoryName(qcFileFullPath);

        // ------- Read the qc file and gets all events ------ //
        List<SequenceEventInfo> sequenceEventInfoList = ParseQcFile(qcFileFullPath);

        //  Find AnimationClips in /Animation Folder
        PopulateAnimationClipFromQc(sequenceEventInfoList, weaponResources, currentFolderPath, currentFolderAssetPath);

        // ----------------Audio Clips------------------
        PopulateAudioClipInResource(sequenceEventInfoList, weaponResources, currentFolderPath, currentFolderAssetPath);

        AssetDatabase.Refresh();
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
                if (!targetSequenceSet.Contains(sequenceName))
                    continue;

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

            lineParsed++;
        }

        sequenceEventInfo.dictFrameToSoundName = dictFrameToSoundName;

        return lineParsed;
    }


    private static void PopulateAnimationClipFromQc(List<SequenceEventInfo> sequenceEventInfoList, WeaponResources weaponResources, string currentFolderPath, string currentFolderAssetPath)
    {
        string animationFolder = Path.Combine(currentFolderPath, "Animations");
        DirectoryInfo dirInfo = new DirectoryInfo(animationFolder);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.anim", SearchOption.TopDirectoryOnly);
        foreach (FileInfo fileInfo in animationFileInfos)
        {
            if (!ExistsInTargetSet(fileInfo.Name))
                continue;
            string assetPath = Path.Combine(currentFolderAssetPath, "Animations", fileInfo.Name);

            SequenceEventInfo sequenceEventInfo = GetRelatedSequenceEventInfo(assetPath, sequenceEventInfoList);
            AnimationClip animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
            AnimationEvent[] createdEvents = CreateAnimationEvents(sequenceEventInfo);
            AnimationUtility.SetAnimationEvents(animationClip, createdEvents);

            // ----- Map to ClipTransitions --- //
            if (fileInfo.Name.Contains("idle"))
            {
                weaponResources.idleClip.Clip = animationClip;
            }
            else if (fileInfo.Name.Contains("reload"))
                weaponResources.reloadClip.Clip = animationClip;
            else if (fileInfo.Name.Contains("shoot"))
                weaponResources.shootClip.Clip = animationClip;
            else if (fileInfo.Name.Contains("draw"))
                weaponResources.drawClip.Clip = animationClip;
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


    private static void PopulateAudioClipInResource(List<SequenceEventInfo> sequenceEventInfoList, WeaponResources weaponResources, string currentFolderPath, string currentFolderAssetPath)
    {
        weaponResources.dictWeaponSounds = new Dictionary<string, AudioClip>();
        string soundFolder = Path.Combine(currentFolderPath, "Sounds");
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
                        string assetPath = Path.Combine(currentFolderAssetPath, "Sounds", fileInfo.Name);
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
    

    class SequenceEventInfo
    {
        public string sequenceName;
        public Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();
        public Dictionary<string, AudioClip> dictNameToAudioClip = new Dictionary<string, AudioClip>();
    }
}
