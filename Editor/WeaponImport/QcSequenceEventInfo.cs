using System.Collections.Generic;
using UnityEngine;

public class QcSequenceEventInfo
{
    public WeaponAnimType WeaponAnimType = WeaponAnimType.OTHER;
    public string sequenceName;
    public float fpsMultiplier;
    public Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();
    public Dictionary<string, AudioClip> dictNameToAudioClip = new Dictionary<string, AudioClip>();

    public void CopyFrom(QcSequenceEventInfo other) 
    {
        if (other.fpsMultiplier > 0)
            this.fpsMultiplier = other.fpsMultiplier;

        foreach (KeyValuePair<int, string> kvp in other.dictFrameToSoundName)
            this.dictFrameToSoundName.Add(kvp.Key, kvp.Value);

        foreach(KeyValuePair<string, AudioClip> kvp in other.dictNameToAudioClip)
            this.dictNameToAudioClip.Add(kvp.Key, kvp.Value);
    }
}

