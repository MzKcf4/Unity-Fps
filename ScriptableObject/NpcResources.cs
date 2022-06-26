using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu]
public class NpcResources : SerializedScriptableObject
{
	public GameObject modelPrefab;
	public string id;
	public AvatarMask upperBodyMask;
	public AvatarMask lowerBodyMask;
	
	public AudioClip hurtAudio;
	public AudioClip deathAudio;

	public bool canMelee;
	public bool canRange;

	public ClipTransition meleeClip;
	public ClipTransition rangedClip;
	public ClipTransition idleClip;
	public ClipTransition runClip;
	public ClipTransition walkClip;
	public List<ClipTransition> deathClips;
}
