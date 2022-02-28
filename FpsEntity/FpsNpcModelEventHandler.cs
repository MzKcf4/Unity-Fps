using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FpsNpcModelEventHandler : MonoBehaviour
{
    [HideInInspector] public FpsNpc controllerEntity;

    void Awake()
    {
        controllerEntity = GetComponentInParent<FpsNpc>();
    }

    public void AniEvent_CheckMeleeHit()
    {
        controllerEntity.AniEvent_CheckMeleeHit();
    }
}

