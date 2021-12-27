using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingDto
{
    public float audioMasterVolume;
    public float mouseSpeed;
    public float mouseSpeedZoomed;
    
    public string playerName;

    public float GetConvertedMouseSpeed() 
    {
        return mouseSpeed / 10000f;
    }

    public float GetConvertedMouseZoomedSpeed()
    {
        return mouseSpeedZoomed / 10000f;
    }
}
