using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class MzCharacterAnimator : MonoBehaviour
{
    [SerializeField] private CharacterAnimationResource charRes;
    private FpsCharacter fpsCharacter;
    protected AnimancerComponent modelAnimancer;
    protected Animator animator;

    protected const int UPPER_LAYER = 0;
    protected const int LOWER_LAYER = 1;

    private AnimationClip[] currentLayerPlayingClip = new AnimationClip[2];

    protected MovementDirection prevMoveDir = MovementDirection.None;
    protected CharacterStateEnum prevCharState = CharacterStateEnum.None;

    void Start()
    {
        fpsCharacter = GetComponent<FpsCharacter>();
        fpsCharacter.onSpawnEvent.AddListener(OnCharacterSpawn);
    }

    void Update()
    {
        if (modelAnimancer == null) return;
        HandleMovementAnimation();
    }

    protected virtual void HandleMovementAnimation()
    {
        if (charRes == null || fpsCharacter.IsDead()) return;

        // Debug.Log(fpsCharacter.GetCurrentVelocity().magnitude);
        if (fpsCharacter.GetCurrentVelocity().magnitude > 0.001f)
            PlayLocomotionAnimation(charRes.runClip);
        else
            PlayLocomotionAnimation(charRes.idleClip);

        /*
        if (currState == CharacterStateEnum.Idle)
            PlayLocomotionAnimation(charRes.idleClip, LOWER_LAYER);
        else if (currState == CharacterStateEnum.Run)
        {
            if (currMoveDir == MovementDirection.Front)
                PlayLocomotionAnimation(charRes.runClip, LOWER_LAYER);
            else if (currMoveDir == MovementDirection.Back)
                PlayLocomotionAnimation(charRes.runClipBack, LOWER_LAYER);
            else if (currMoveDir == MovementDirection.Left)
                PlayLocomotionAnimation(charRes.runClipLeft, LOWER_LAYER);
            else if (currMoveDir == MovementDirection.Right)
                PlayLocomotionAnimation(charRes.runClipRight, LOWER_LAYER);
        }
        */
    }

    public void SetAttachedModel(FpsModel fpsModel)
    {
        modelAnimancer = fpsModel.gameObject.GetComponent<AnimancerComponent>();
        animator = fpsModel.gameObject.GetComponent<Animator>();

        modelAnimancer.Layers[UPPER_LAYER].SetMask(charRes.upperBodyMask);
        modelAnimancer.Layers[LOWER_LAYER].SetMask(charRes.lowerBodyMask);

        if (charRes.upperBodyAimClip.Clip != null)
            modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyAimClip.Clip);
            // PlayActionAnimation(charRes.upperBodyAimClip);

        if (charRes.idleClip.Clip != null)
            PlayLocomotionAnimation(charRes.idleClip);
    }

    // Do NOT play the ClipTransition itself directly if you have OnEnd events
    //      because it applies the OnEnd event to ClipTransition itself , so if the ClipTransition is shared among other objects
    //      this object's OnEnd will call OTHER object's OnEnd !
    public void PlayActionAnimation(ClipTransition clip)
    {
        AnimancerState state = modelAnimancer.Layers[UPPER_LAYER].Play(clip.Clip, 0.1f, FadeMode.FixedSpeed);
        state.Events.OnEnd = () =>
        {
            // Return to idle state
            currentLayerPlayingClip[UPPER_LAYER] = null;
            modelAnimancer.Layers[UPPER_LAYER].Play(charRes.idleClip, 0.1f);
        };
        
        state.Speed = clip.Speed;
    }

    private void PlayLocomotionAnimation(ClipTransition clip)
    {
        if (currentLayerPlayingClip[LOWER_LAYER] == clip.Clip)
            return;
        currentLayerPlayingClip[LOWER_LAYER] = clip.Clip;

        // modelAnimancer.Layers[LOWER_LAYER].Stop();

        AnimancerState state = modelAnimancer.Layers[LOWER_LAYER].Play(clip.Clip, 0.1f);
        state.Speed = clip.Speed;
    }

    public void PlayDeathAnimation(ClipTransition clip)
    {
        modelAnimancer.Layers[UPPER_LAYER].Stop();
        modelAnimancer.Layers[LOWER_LAYER].Stop();
        
        modelAnimancer.Layers[UPPER_LAYER].Play(clip, 0.1f, FadeMode.FromStart);
        modelAnimancer.Layers[LOWER_LAYER].Play(clip, 0.1f, FadeMode.FromStart);
    }

    public void RpcFireWeapon_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyShootClip);
    }

    public void RpcReloadWeapon_Animation()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyReloadClip);
    }

    private void OnCharacterSpawn()
    {
        modelAnimancer.Layers[UPPER_LAYER].Play(charRes.upperBodyAimClip, 0.1f);
        modelAnimancer.Layers[LOWER_LAYER].Play(charRes.idleClip, 0.1f);
    }

    private void OnEnable()
    {
        if (modelAnimancer == null) return;

        modelAnimancer.enabled = true;
        animator.enabled = true;
    }

    private void OnDisable()
    {
        if (modelAnimancer == null) return;

        modelAnimancer.enabled = false;
        animator.enabled = false;
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
