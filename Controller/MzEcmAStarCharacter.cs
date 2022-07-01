using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using EasyCharacterMovement;


public class MzEcmAStarCharacter : EasyCharacterMovement.Character
{
    protected IAstarAI ai;
    protected Seeker seeker;

    private Vector3 aiNextPosition;
    private Quaternion aiNextRotation;

    private FpsCharacter mzCharacter;

    private bool isServer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ai = GetComponent<IAstarAI>();
        seeker = GetComponent<Seeker>();

        mzCharacter = GetComponent<FpsCharacter>();
        isServer = mzCharacter.isServer;

        ai.canMove = false;
    }

    private void SyncAStarMovement()
    {
        ai.MovementUpdate(Time.deltaTime, out aiNextPosition, out aiNextRotation);


        Vector3 planarDesiredVelocity = ai.desiredVelocity.projectedOnPlane(GetUpVector());

        if (planarDesiredVelocity.sqrMagnitude < MathLib.Square(minAnalogWalkSpeed))
            planarDesiredVelocity = planarDesiredVelocity.normalized * minAnalogWalkSpeed;

        SetMovementDirection(planarDesiredVelocity.normalized * ComputeAnalogInputModifier(planarDesiredVelocity));
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
    }
    


}
