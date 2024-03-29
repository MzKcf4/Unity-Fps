﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingDto
{
    public float audioMasterVolume = -25f;
    public float mouseSpeed = 10f;
    public float mouseSpeedZoomed = 3f;
    public CrosshairSizeEnum crosshairSize = CrosshairSizeEnum.Small;
    public bool isLerpCrosshair = false;

    public string playerName;

    public float GetConvertedMouseSpeed() 
    {
        return mouseSpeed / 100f;
    }

    public float GetConvertedMouseZoomedSpeed()
    {
        return mouseSpeedZoomed / 100f;
    }

}
