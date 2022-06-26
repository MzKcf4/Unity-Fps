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

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        
        ai = GetComponent<IAstarAI>();
        seeker = GetComponent<Seeker>();

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

    protected override void Move()
    {
        SyncAStarMovement();
        base.Move();
    }

}
