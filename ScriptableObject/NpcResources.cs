using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu]
public class NpcResources : SerializedScriptableObject
{
	public AvatarMask upperBodyMask;
	
	public AudioClip hurtAudio;
	public AudioClip deathAudio;
	
	public ClipTransition meleeClip;
	public ClipTransition rangedClip;
	public ClipTransition idleClipUpper;
	public ClipTransition runClip;
	public ClipTransition walkClip;
	public List<ClipTransition> deathClips;
}
