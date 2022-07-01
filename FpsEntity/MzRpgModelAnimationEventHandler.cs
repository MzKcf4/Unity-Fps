using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MzRpgModelAnimationEventHandler : MonoBehaviour
{
    private MzRpgCharacter character;

    void Awake()
    {
        character = GetComponentInParent<MzRpgCharacter>();
    }

    public void AniEvent_CheckMeleeHit()
    {
        character.CheckMeleeHit();
    }
}
