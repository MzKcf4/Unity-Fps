using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu(menuName = "FpsResource/CharacterResources")]
public class CharacterResources : SerializedScriptableObject
{
    public AvatarMask upperBodyMask;
    public AvatarMask lowerBodyMask;
    
    public AudioClip hurtAudio;
    public AudioClip deathAudio;
    
    public ClipTransition upperBodyAimClip;
    public ClipTransition upperBodyShootClip;
    
    public ClipTransition idleClip;
    public ClipTransition runClip;
    public ClipTransition walkClip;
    public List<ClipTransition> deathClips;
    
    public ClipTransition runClipBack;
    public ClipTransition runClipLeft;
    public ClipTransition runClipRight;
}

