using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Animancer;

// A Fps_Humanoid_Character should 
// -  have a Humanoid model , with weaponRoot component defined in hand for weapon world model
// -  be able to hold FpsWeapon
// -  perform weapon action with FpsWeapon

public abstract partial class FpsHumanoidCharacter : FpsCharacter
{
    // --> protected HumanoidCommonResource;
    // protected CharacterCommonResources characterCommonResources;

    // --> HumanoidResource
    // [SerializeField] protected CharacterResources charRes;
    /*
    protected FpsModel fpsModel;
    protected GameObject modelObject;
    protected GameObject modelObjectParent;
    [SerializeField] protected Transform lookAtTransform;
    */

    /*
    [SyncVar] public string characterName;
    [SyncVar] protected Vector3 currentVelocity = Vector3.zero;
    [SyncVar] public TeamEnum team = TeamEnum.Blue;
    [SyncVar] protected MovementDirection currMoveDir = MovementDirection.None;
    [SyncVar] public CharacterStateEnum currState;
    protected MovementDirection prevMoveDir = MovementDirection.None;
    protected CharacterStateEnum prevCharState = CharacterStateEnum.None;

    protected AudioSource audioSourceWeapon;
    protected AudioSource audioSourceCharacter;
    */



    public override void OnStartClient()
    {
        base.OnStartClient();
        characterCommonResources = WeaponAssetManager.Instance.GetCharacterCommonResources();
    }

    protected override void Awake()
    {
        base.Awake();
        weaponRootTransform = GetComponentInChildren<CharacterWeaponRoot>().transform;
    }

    protected override void Start()
    {
        base.Start();
        Start_Weapon();
    }

    [Server]
    public override void Respawn()
    {
        base.Respawn();
        if (activeWeaponSlot >= 0)
            RpcSwitchWeapon(activeWeaponSlot);

        ResetAllWeapons();
    }

    [ClientRpc]
    public override void RpcRespawn()
    {
        base.RpcRespawn();
        ResetAllWeapons();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (IsDead()) return;

        Update_Weapon();
    }

}

