using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCharacterMovement;
using UnityEngine;

public class EcmCharacterPrediction : ClientPrediction<EcmNetworkInput, EcmNetworkState>
{
    private EasyCharacterMovement.CharacterMovement ecmCharacter;

    protected override void Awake()
    {
        base.Awake();
        ecmCharacter = GetComponent<EasyCharacterMovement.CharacterMovement>();
    }

    protected override EcmNetworkInput GetInput(float deltaTime, uint currentTick)
    {
        Vector3 velocity = ecmCharacter.velocity;
        return new EcmNetworkInput(velocity, currentTick, deltaTime);
    }
}