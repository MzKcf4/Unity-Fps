using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

// FpsCharacter.Animantion
public partial class FpsCharacter
{
    protected AnimancerComponent modelAnimancer;
    
    private const int UPPER_LAYER = 0;
    private const int LOWER_LAYER = 1;
    
    private ClipTransition currentPlayingClip;
    
    private void InitializeAnimation()
    {
        modelAnimancer.Layers[UPPER_LAYER].SetMask(charRes.upperBodyMask);
        modelAnimancer.Layers[LOWER_LAYER].SetMask(charRes.lowerBodyMask);
        
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyAimClip , 0.1f , FadeMode.FromStart);
    }
    
    protected virtual void Update_Animation()
    {
        UpdateMovementDir();
        HandleMovementAnimation();
        
    }
    
    protected virtual void UpdateMovementDir()
    {
        if(CanPlayIdleAnimation())
        {
            currState = CharacterStateEnum.Idle;
            currMoveDir = MovementDirection.None;
        } 
        else if (CanPlayRunAnimation())
        {
            currState = CharacterStateEnum.Run;
            currMoveDir = GetMovementDirection();
        }
    }
    
    protected virtual void HandleMovementAnimation()
    {
        if(charRes == null) return;
        
        if(currState == CharacterStateEnum.Idle)
            PlayAnimation(charRes.idleClip, LOWER_LAYER);
        else if(currState == CharacterStateEnum.Run)
        {
            if(currMoveDir == MovementDirection.Front)
                PlayAnimation(charRes.runClip,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Back)
                PlayAnimation(charRes.runClipBack,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Left)
                PlayAnimation(charRes.runClipLeft,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Right)
                PlayAnimation(charRes.runClipRight,LOWER_LAYER);
        }
    }
    
    private bool CanPlayIdleAnimation()
    {
        return !IsDead() && 
        (currState != CharacterStateEnum.Idle && 
            currState != CharacterStateEnum.Dead && 
            GetMovementVelocity().magnitude == 0f);
    }
    
    private bool CanPlayRunAnimation()
    {
        return !IsDead() && 
        (currState != CharacterStateEnum.Dead &&
            GetMovementVelocity().magnitude > 0f);
    }
    
    protected void PlayAnimation(ClipTransition clip, int layer)
    {
        if(currentPlayingClip == clip)
            return;
            
        currentPlayingClip = clip;
        modelAnimancer.Layers[layer].Play(clip , 0.1f , FadeMode.FromStart);
    }

    public void RpcFireWeapon_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyShootClip , 0.1f , FadeMode.FromStart);
    }
}
