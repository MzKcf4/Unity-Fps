using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu]
public class WeaponResources : SerializedScriptableObject
{
    public GameObject weaponViewPrefab;
    public GameObject weaponWorldPrefab;
    
	public Dictionary<string , AudioClip> dictWeaponSounds;
	
	public ClipTransition drawClip;
	public ClipTransition idleClip;
	public ClipTransition shootClip;
	public ClipTransition reloadClip;

    [FoldoutGroup("Shotgun")]
    public ClipTransition palletReload_StartClip;
    [FoldoutGroup("Shotgun")]
    public ClipTransition palletReload_InsertClip;
    [FoldoutGroup("Shotgun")]
    public ClipTransition palletReload_EndClip;


    [FoldoutGroup("Knife")]
    public ClipTransition lightAttackClip;
    [FoldoutGroup("Knife")]
    public ClipTransition lightAttackMissClip;
    [FoldoutGroup("Knife")]
    public ClipTransition heavyAttackClip;
    [FoldoutGroup("Knife")]
    public ClipTransition heavyAttackMissClip;
}