using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animancer;

// FpsNpc.Animation
public abstract partial class FpsNpc
{
    /*
    protected AnimancerComponent modelAnimancer;

    protected const int UPPER_LAYER = 0;
    protected const int LOWER_LAYER = 1;

    private ClipTransition currentPlayingClip;
    

    protected override void Start_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].SetMask(npcResources.upperBodyMask);
        modelAnimancer.Layers[LOWER_LAYER].SetMask(npcResources.lowerBodyMask);

        modelAnimancer.Layers[UPPER_LAYER].Play(npcResources.idleClip, 0.1f, FadeMode.FromStart);
    }

    
    protected override void Respawn_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(npcResources.idleClip, 0.1f, FadeMode.FromStart);
        modelAnimancer.Layers[LOWER_LAYER].Play(npcResources.idleClip, 0.1f, FadeMode.FromStart);
    }
    

    protected override void Update_Animation()
    {
        if (IsDead()) return;

        if (isServer)
        {
            UpdateMovementDir();
            if (prevMoveDir != currMoveDir || prevCharState != currState)
            {
                prevMoveDir = currMoveDir;
                prevCharState = currState;
            }
        }

        HandleMovementAnimation();
    }
    
    protected override void HandleMovementAnimation()
    {
        if (npcResources == null || IsDead()) return;

        if (currState == CharacterStateEnum.Idle)
            PlayAnimation(npcResources.idleClip, LOWER_LAYER);
        else if (currState == CharacterStateEnum.Run)
        {
            PlayAnimation(npcResources.runClip, LOWER_LAYER);
        }
    }

    protected override void UpdateMovementDir()
    {
        if (CanPlayIdleAnimation())
        {
            currState = CharacterStateEnum.Idle;
        }
        else if (CanPlayRunAnimation())
        {
            currState = CharacterStateEnum.Run;
        }
    }

    private bool CanPlayIdleAnimation()
    {
        return !IsDead() &&
        (currState != CharacterStateEnum.Idle &&
            currState != CharacterStateEnum.Dead &&
            GetMovementVelocity().magnitude <= 0.01f);
    }

    private bool CanPlayRunAnimation()
    {
        return !IsDead() &&
        (currState != CharacterStateEnum.Dead &&
            GetMovementVelocity().magnitude > 0.01f);
    }
    */
}

