using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QcSequenceEventInfo
{
    public string sequenceName;
    public float fpsMultiplier;
    public Dictionary<int, string> dictFrameToSoundName = new Dictionary<int, string>();
    public Dictionary<string, AudioClip> dictNameToAudioClip = new Dictionary<string, AudioClip>();
}

