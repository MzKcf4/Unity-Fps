using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu(menuName = "FpsResource/CsoEnemyResources")]
public class CsoEnemyResources : SerializedScriptableObject
{
	public AvatarMask upperBodyMask;
	
	public AudioClip hurtAudio;
	public AudioClip deathAudio;
	
	public ClipTransition meleeClip;
	public ClipTransition idleClipUpper;
	public ClipTransition runClip;
	public List<ClipTransition> deathClips;
	
}
