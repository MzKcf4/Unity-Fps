using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MzFpsBot : FpsHumanoidCharacter
{
    private MzFpsBotBrain botBrain;

    protected override void Start()
    {
        base.Start();
        if (isServer)
        { 
            botBrain = GetComponent<MzFpsBotBrain>();
        }
    }

    public void SetSkillLevel(int level)
    {
        botBrain.SetSkillLevel(level);
    }
}
