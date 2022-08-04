using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FpsResource/CharacterAnimationResource")]
public class CharacterAnimationResource : SerializedScriptableObject
{
    public GameObject characterModelPrefab;
    public AvatarMask upperBodyMask;
    public AvatarMask lowerBodyMask;

    public Dictionary<string, ActionAnimationInfo> actionClips = new Dictionary<string, ActionAnimationInfo>();

    // --- Deprecated action clips --- //
    public ClipTransition upperBodyAimClip;
    public ClipTransition upperBodyShootClip;
    public ClipTransition upperBodyReloadClip;
    // ------------------------------- //
    public ClipTransition upperBodyIdleClip;

    public List<ClipTransition> deathClips = new List<ClipTransition>();
    public ClipTransition idleClip;
    public ClipTransition walkClip;

    public bool hasStrafeAnimation = false;
    public ClipTransition runClip;
    public ClipTransition runClipBack;
    public ClipTransition runClipLeft;
    public ClipTransition runClipRight;
}

[Serializable]
public class ActionAnimationInfo 
{
    public bool isFullBody = false;
    public ClipTransition actionClip;
}