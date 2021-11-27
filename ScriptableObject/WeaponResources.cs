using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu]
public class WeaponResources : SerializedScriptableObject
{
	public Dictionary<string , AudioClip> dictWeaponSounds;
	
	public ClipTransition drawClip;
	public ClipTransition idleClip;
	public ClipTransition shootClip;
	public ClipTransition reloadClip;
    
    public ClipTransition palletReload_StartClip;
    public ClipTransition palletReload_InsertClip;
    public ClipTransition palletReload_EndClip;
}