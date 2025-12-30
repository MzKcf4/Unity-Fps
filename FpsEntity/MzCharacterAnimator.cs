using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class MzCharacterAnimator : MonoBehaviour
{
    [SerializeField] private CharacterAnimationResource animationResource;
    private FpsCharacter fpsCharacter;
    protected AnimancerComponent modelAnimancer;
    protected Animator animator;

    protected const int FULL_BODY_LAYER = 0;
    protected const int UPPER_BODY_LAYER = 1;

    private AnimationClip[] currentLayerPlayingClip = new AnimationClip[2];

    protected MovementDirection prevMoveDir = MovementDirection.None;
    protected CharacterStateEnum prevCharState = CharacterStateEnum.None;

    private bool isSetup = false;
    private bool isDisableLocomation = false;

    private AnimancerLayer upperLayer;
    private AnimancerLayer fullBodyLayer;

    private ActionCooldown animationUpdateCheck = new ActionCooldown { interval = 0.5f };

    void Start()
    {
        fpsCharacter = GetComponent<FpsCharacter>();
        fpsCharacter.onSpawnEvent.AddListener(OnCharacterSpawn);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (modelAnimancer == null || !isSetup || isDisableLocomation || !animationUpdateCheck.CanExecuteAfterDeltaTime()) 
            return;

        HandleMovementAnimation();
    }

    protected virtual void HandleMovementAnimation()
    {
        if (animationResource == null || fpsCharacter.IsDead()) return;
        
        var currentVelocity = fpsCharacter.GetCurrentVelocity();
        var horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        if (horizontalVelocity.magnitude > 3.0f)
            PlayLocomotionAnimation(animationResource.runClip);
        else if (horizontalVelocity.magnitude > 0.5f)
            PlayLocomotionAnimation(animationResource.walkClip);
        else
            PlayLocomotionAnimation(animationResource.idleClip);
    }

    public void SetAttachedModel(FpsModel fpsModel)
    {
        isSetup = false;
        modelAnimancer = fpsModel.gameObject.GetComponent<AnimancerComponent>();
        animator = fpsModel.gameObject.GetComponent<Animator>();

        if (animationResource != null)
        {
            if (animationResource.upperBodyMask != null)
            {
                upperLayer = modelAnimancer.Layers[UPPER_BODY_LAYER];

                modelAnimancer.Layers[UPPER_BODY_LAYER].SetMask(animationResource.upperBodyMask);
                modelAnimancer.Layers[UPPER_BODY_LAYER].IsAdditive = true;
            }

            if (animationResource.lowerBodyMask != null)
            {
                fullBodyLayer = modelAnimancer.Layers[FULL_BODY_LAYER];
                // modelAnimancer.Layers[LOWER_LAYER].SetMask(charRes.lowerBodyMask);

            }
              
            if (animationResource.upperBodyAimClip.Clip != null)
                this.StartCoroutine(PlayClipDelayed(animationResource.upperBodyAimClip.Clip));
            else if (animationResource.upperBodyIdleClip.Clip != null)
            {
                this.StartCoroutine(PlayClipDelayed(animationResource.upperBodyIdleClip.Clip));
            }


            if (animationResource.idleClip.Clip != null)
                PlayLocomotionAnimation(animationResource.idleClip);
        }
        isSetup = true;
    }

    IEnumerator PlayClipDelayed(AnimationClip clip)
    {
        yield return new WaitForSeconds(0.5f);
        modelAnimancer.Layers[FULL_BODY_LAYER].Play(clip, 0.1f);
    }

    public void PlayAnimationByKey(string key, bool isFullBody, bool isForceExecute) 
    {
        if (!animationResource.actionClips.ContainsKey(key))
        {
            Debug.LogWarning("Action clip key not found : " + key);
            return;
        }

        PlayActionAnimation(animationResource.actionClips[key].actionClip, isFullBody, isForceExecute);
    }


    // Do NOT play the ClipTransition itself directly if you have OnEnd events
    //      because it applies the OnEnd event to ClipTransition itself , so if the ClipTransition is shared among other objects
    //      this object's OnEnd will call OTHER object's OnEnd !
    public void PlayActionAnimation(ClipTransition clip, bool isFullBodyAnimation, bool isForceExecute)
    {
        if (!isForceExecute && currentLayerPlayingClip[FULL_BODY_LAYER] != null)
            return;

        if (isFullBodyAnimation) 
        {
            isDisableLocomation = true;

            modelAnimancer.Layers[FULL_BODY_LAYER].Stop();
            modelAnimancer.Layers[UPPER_BODY_LAYER].Stop();

            AnimancerState state = modelAnimancer.Layers[FULL_BODY_LAYER].Play(clip.Clip, 0.1f, FadeMode.FixedSpeed);
            state.Events.OnEnd = () =>
            {
                // Return to idle state
                currentLayerPlayingClip[FULL_BODY_LAYER] = null;
                currentLayerPlayingClip[UPPER_BODY_LAYER] = null;
                isDisableLocomation = false;
                modelAnimancer.Layers[FULL_BODY_LAYER].Play(animationResource.upperBodyIdleClip, 0.1f);
            };

            state.Speed = clip.Speed;
        } 
        else
        {
            currentLayerPlayingClip[UPPER_BODY_LAYER] = clip.Clip;
            AnimancerState state = upperLayer.Play(clip.Clip , 0.1f);

            // If the animation was already playing, it will continue from the current time.
            // So to force it to play from the beginning you can just reset the Time:
            state.Time = 0f;

            /*
            state.Events.OnEnd = () =>
            {
                // Return to idle state
                Debug.Log("Action animation ended , return to upper body idle");
                currentLayerPlayingClip[UPPER_LAYER] = null;
                modelAnimancer.Layers[UPPER_LAYER].Play(animationResource.upperBodyIdleClip, 0.1f);
            };
            */

            state.Speed = clip.Speed;
        }

    }

    private void PlayLocomotionAnimation(ClipTransition clip)
    {
        if (currentLayerPlayingClip[FULL_BODY_LAYER] == clip.Clip)
            return;

        currentLayerPlayingClip[FULL_BODY_LAYER] = clip.Clip;
        AnimancerState state = modelAnimancer.Layers[FULL_BODY_LAYER].Play(clip.Clip);
        state.Speed = clip.Speed;
    }

    public void PlayDeathAnimation(ClipTransition clip)
    {
        modelAnimancer.Layers[FULL_BODY_LAYER].Stop();
        modelAnimancer.Layers[UPPER_BODY_LAYER].Stop();
        
        modelAnimancer.Layers[FULL_BODY_LAYER].Play(clip, 0.1f, FadeMode.FromStart);
        modelAnimancer.Layers[UPPER_BODY_LAYER].Play(clip, 0.1f, FadeMode.FromStart);
    }

    private void OnCharacterSpawn()
    {
        if (animationResource.upperBodyAimClip.Clip != null)
            modelAnimancer.Layers[FULL_BODY_LAYER].Play(animationResource.upperBodyAimClip.Clip);
        else if (animationResource.idleClip.Clip != null)
            modelAnimancer.Layers[FULL_BODY_LAYER].Play(animationResource.idleClip, 0.1f);

        modelAnimancer.Layers[UPPER_BODY_LAYER].Play(animationResource.idleClip, 0.1f);
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
