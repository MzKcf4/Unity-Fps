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
    public ClipTransition meleeLightAttackClip;
    [FoldoutGroup("Knife")]
    public ClipTransition meleeLightAttackMissClip;
    [FoldoutGroup("Knife")]
    public ClipTransition meleeHeavyAttackClip;
    [FoldoutGroup("Knife")]
    public ClipTransition meleeHeavyAttackMissClip;

    [FoldoutGroup("Knife")]
    public AudioClip meleeLightAttackHitSound;
    [FoldoutGroup("Knife")]
    public AudioClip meleeLightAttackHitWallSound;
    [FoldoutGroup("Knife")]
    public AudioClip meleeLightAttackMissSound;
    [FoldoutGroup("Knife")]
    public AudioClip meleeHeavyAttackHitSound;
    [FoldoutGroup("Knife")]
    public AudioClip meleeHeavyAttackHitWallSound;
    [FoldoutGroup("Knife")]
    public AudioClip meleeHeavyAttackMissSound;

}