using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using EasyCharacterMovement;
using Mirror;


public class MzEcmAStarCharacter : EasyCharacterMovement.Character
{
    protected IAstarAI ai;
    private bool isServer;
    [SerializeField]
    protected GameObject aiMover;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ai = GetComponentInChildren<IAstarAI>();
        isServer = GetComponent<NetworkIdentity>().isServer;
    }

    private void SyncAStarMovement()
    {
        SetMovementDirection(ai.desiredVelocity.normalized);
    }

    protected override Vector3 CalcDesiredVelocity()
    {
        return ai.desiredVelocity;
    }

    private void SyncAStarSpeed()
    { 
        ai.maxSpeed = GetMaxSpeed();
    }

    protected override void Move()
    {
        if (isServer)
        {
            SyncAStarSpeed();
            SyncAStarMovement();
        }

        base.Move();
        aiMover.transform.localPosition = Vector3.zero;
    }
}
