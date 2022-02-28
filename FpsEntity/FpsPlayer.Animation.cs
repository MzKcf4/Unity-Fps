using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// FpsPlayer.Animation
public partial class FpsPlayer
{
    protected override void Update_Animation()
    {
        if (isLocalPlayer)
        {
            UpdateMovementDir();
            if (prevMoveDir != currMoveDir || prevCharState != currState)
            {
                CmdUpdateMovementVar(currMoveDir, currState);
                prevMoveDir = currMoveDir;
                prevCharState = currState;
            }
        }
        HandleMovementAnimation();
    }

    [Command]
    private void CmdUpdateMovementVar(MovementDirection newMoveDir , CharacterStateEnum newCharState) 
    {
        currMoveDir = newMoveDir;
        currState = newCharState;
    }
    
}
