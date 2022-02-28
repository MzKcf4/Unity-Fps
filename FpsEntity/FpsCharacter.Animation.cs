using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

// FpsCharacter.Animantion
public partial class FpsCharacter
{
    protected AnimancerComponent modelAnimancer;
    
    protected const int UPPER_LAYER = 0;
    protected const int LOWER_LAYER = 1;

    private ClipTransition currentPlayingClip;
    
    protected virtual void Start_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].SetMask(charRes.upperBodyMask);
        modelAnimancer.Layers[LOWER_LAYER].SetMask(charRes.lowerBodyMask);
        
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyAimClip, 0.1f);
        modelAnimancer.Layers[LOWER_LAYER].Play(charRes.idleClip, 0.1f);
    }
    
    protected virtual void Respawn_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyAimClip , 0.1f);
        modelAnimancer.Layers[LOWER_LAYER].Play(charRes.idleClip, 0.1f);
    }
    
    protected virtual void Update_Animation()
    {
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
        if(charRes == null || IsDead()) return;
        
        if(currState == CharacterStateEnum.Idle)
            PlayLocomotionAnimation(charRes.idleClip, LOWER_LAYER);
        else if(currState == CharacterStateEnum.Run)
        {
            if(currMoveDir == MovementDirection.Front)
                PlayLocomotionAnimation(charRes.runClip,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Back)
                PlayLocomotionAnimation(charRes.runClipBack,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Left)
                PlayLocomotionAnimation(charRes.runClipLeft,LOWER_LAYER);
            else if(currMoveDir == MovementDirection.Right)
                PlayLocomotionAnimation(charRes.runClipRight,LOWER_LAYER);
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
    
    protected void PlayAnimation(ClipTransition clip, int layer)
    {
        PlayAnimation(clip, layer, null);
    }

    protected void PlayActionAnimation(ClipTransition clip, int layer , ClipTransition idleClip)
    {
        var state = modelAnimancer.Layers[layer].Play(clip, 0.1f, FadeMode.FromStart);
        state.Events.OnEnd = () => { 
            state.Layer.StartFade(0, 0.1f);
            modelAnimancer.Layers[layer].Play(idleClip, 0.1f);
        };
    }

    protected AnimancerState PlayLocomotionAnimation(ClipTransition clip, int layer)
    {
        return modelAnimancer.Layers[layer].Play(clip, 0.1f);
    }

    protected void PlayAnimation(ClipTransition clip, int layer , ClipTransition idleClip)
    {
        if (currentPlayingClip == clip)
            return;
        
        currentPlayingClip = clip;
        var state = modelAnimancer.Layers[layer].Play(clip, 0.1f, FadeMode.FromStart);
        if (idleClip != null)
        {
            state.Events.OnEnd = () => PlayAnimation(idleClip, layer);
        }
        
    }

    protected void PlayFullBodyAnimation(ClipTransition clip)
    {
        modelAnimancer.Layers[UPPER_LAYER].Stop();
        modelAnimancer.Layers[LOWER_LAYER].Stop();
        currentPlayingClip = clip;
        modelAnimancer.Layers[UPPER_LAYER].Play(clip, 0.1f , FadeMode.FromStart);
        modelAnimancer.Layers[LOWER_LAYER].Play(clip, 0.1f , FadeMode.FromStart);
    }

    public void RpcFireWeapon_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyShootClip);
    }
    
    public void RpcReloadWeapon_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyReloadClip);
    }

    /*
    protected void PlayAnimation(ClipTransition clip, int layer , ClipTransition idleClip)
    {
        /*
        // From https://kybernetik.com.au/animancer/docs/examples/layers/
        // Playing new clip while another one is playing
        var state = modelAnimancer.Layers[layer].CurrentState;
        if (state != null && state.Weight > 0 && state.Clip == clip.Clip)
        {
            // Create a copy of its state to let the original fade out properly
            var time = state.Time;
            state = modelAnimancer.Layers[layer].Play(clip, 0.1f);
            //  give the new state the correct time
            state.Time = time;
            if (idleClip != null)
            {

                state.Events.OnEnd = () =>
                {
                    state.Layer.StartFade(0, 0.1f);
                    PlayAnimation(idleClip, layer);
                };
            }
            
        }
        else
        {
            modelAnimancer.Layers[layer].Play(clip, 0.1f);
        }
    */
}
