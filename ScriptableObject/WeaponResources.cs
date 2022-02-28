using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class WeaponResources : SerializedScriptableObject
{
    // Should match the weapon_name column in BGDatabase
    public string weaponId;
    public GameObject weaponViewPrefab;
    public GameObject weaponWorldPrefab;
    
	public Dictionary<string , AudioClip> dictWeaponSounds = new Dictionary<string, AudioClip>();

    public ClipTransition drawClip = new ClipTransition();
	public ClipTransition idleClip = new ClipTransition();
	public ClipTransition shootClip = new ClipTransition();
	public ClipTransition reloadClip = new ClipTransition();

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