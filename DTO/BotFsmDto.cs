using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/*
 A dto class for variables used across different BotStateProcessors
 */
[Serializable]
public class BotFsmDto
{
    public Vector3 targetLookAtPosition;
    public Vector3 targetWaypoint;
    public Vector3 shootTargetLastSeenPosition;
    public FpsModel shootTargetModel;
    public DamageInfo lastUnexpectedDamage;

    public void Clear()
    {
        targetLookAtPosition = Vector3.zero;
        targetWaypoint = Vector3.zero;
        shootTargetLastSeenPosition = Vector3.zero;
        shootTargetModel = null;
        lastUnexpectedDamage = null;
    }
}
