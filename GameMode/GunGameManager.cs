using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class GunGameManager : NetworkBehaviour
{
    public static GunGameManager Instance;
    [SyncVar(hook = (nameof(OnBlueTeamLevelUpdate)))]
    private int levelBlueTeam;
    [SyncVar(hook = (nameof(OnRedTeamLevelUpdate)))]
    private int levelRedTeam;
    [SyncVar(hook = (nameof(OnBlueTeamKillsUpdate)))]
    private int killsInLevelBlueTeam;
    [SyncVar(hook = (nameof(OnRedTeamKillsUpdate)))]
    private int killsInLevelRedTeam;
    [SyncVar(hook = (nameof(OnMaxLevelUpdate)))]
    private int maxLevel;
    [SyncVar]
    public int killsPerLevelBlueTeam = 4;
    [SyncVar]
    public int killsPerLevelRedTeam = 2;
    [SyncVar]
    private bool isMatchActive = false;

    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private GameObject uiPrefab;

    private readonly SyncList<string> weaponNamesPerLevelList = new SyncList<string>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
        SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(GunGameUiManager.Instance == null)
            Instantiate(uiPrefab , FpsUiManager.Instance.GetInfoPanel());
    }

    private void OnCharacterSpawn(FpsCharacter fpsCharacter)
    {
        if (!isServer || !isMatchActive || !(fpsCharacter is FpsHumanoidCharacter) ) return;

        FpsHumanoidCharacter humanoidCharacter = (FpsHumanoidCharacter)fpsCharacter;
        TeamEnum team = fpsCharacter.team;
        if (TeamEnum.Blue == team)
        {
            string weaponName = weaponNamesPerLevelList[levelBlueTeam];
            humanoidCharacter.ServerCmdGetWeapon(weaponName, 0);
        }
        else
        {
            string weaponName = weaponNamesPerLevelList[levelRedTeam];
            humanoidCharacter.ServerCmdGetWeapon(weaponName, 0);
        }
    }

    [Server]
    public void RoundStart()
    { 
        RandomizeWeapon();
        levelBlueTeam = 0;
        levelRedTeam = 0;
        killsInLevelBlueTeam = 0;
        killsInLevelRedTeam = 0;
        isMatchActive = true;
        maxLevel = weaponNamesPerLevelList.Count;
        RpcUpdateScore();

        RestorePlayerHealth();
        TeleportPlayersToSpawn();
        AssignWeapons(TeamEnum.Blue, 0);
        AssignWeapons(TeamEnum.Red, 0);
    }

    private void RandomizeWeapon()
    {
        weaponNamesPerLevelList.Clear();
        List<E_weapon_info> availableWeaponList =
            E_weapon_info.FindEntities(e => e.f_active && e.f_category != WeaponCategory.Melee);

        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(4, WeaponCategory.Rifle));
        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(4, WeaponCategory.Sniper));
        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(3, WeaponCategory.Mg));
        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(3, WeaponCategory.Smg));
        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(3, WeaponCategory.Shotgun));
        weaponNamesPerLevelList.AddRange(GetRandomWeaponsByTypes(3, WeaponCategory.Pistol));
    }

    private List<string> GetRandomWeaponsByTypes(int count, WeaponCategory category) 
    {
        List<E_weapon_info> availableWeaponList = E_weapon_info.FindEntities(e => e.f_active && e.f_category == category);
        List<string> selectedWeapons = new List<string>();
        for (int x = 0; x < count; x++)
        {
            int idx = UnityEngine.Random.Range(0, availableWeaponList.Count);
            string weaponName = availableWeaponList[idx].f_name;
            availableWeaponList.RemoveAt(idx);
            selectedWeapons.Add(weaponName);
        }
        return selectedWeapons;
    }

    [Server]
    public void OnCharacterKilled(FpsCharacter victim, DamageInfo dmgInfo)
    {
        PlayerManager.Instance.QueueRespawn(victim);

        if (!isServer || !isMatchActive || string.IsNullOrEmpty(dmgInfo.damageWeaponName))
            return;

        if (victim.team == TeamEnum.Red)
        {
            killsInLevelBlueTeam++;
            if (killsInLevelBlueTeam >= killsPerLevelBlueTeam)
            {
                killsInLevelBlueTeam = 0;
                levelBlueTeam++;
                if (IsReachedMaxLevel(levelBlueTeam))
                {
                    isMatchActive = false;
                    RpcAnnounceWin(TeamEnum.Blue);
                }
                else
                {
                    RpcLevelUp(TeamEnum.Blue);
                    AssignWeapons(TeamEnum.Blue, levelBlueTeam);
                }
            }
        }
        else
        {
            killsInLevelRedTeam++;
            if (killsInLevelRedTeam >= killsPerLevelRedTeam)
            {
                killsInLevelRedTeam = 0;
                levelRedTeam++;
                if (IsReachedMaxLevel(levelRedTeam))
                {
                    isMatchActive = false;
                    RpcAnnounceWin(TeamEnum.Red);
                }
                else
                {
                    RpcLevelUp(TeamEnum.Red);
                    AssignWeapons(TeamEnum.Red, levelRedTeam);
                }
            }
        }
    }

    private void OnBlueTeamLevelUpdate(int oldValue, int newValue)
    {
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Blue, newValue, maxLevel, killsPerLevelBlueTeam, killsInLevelBlueTeam);
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Red, levelRedTeam, maxLevel, killsPerLevelRedTeam, killsInLevelRedTeam);
    }

    private void OnBlueTeamKillsUpdate(int oldValue, int newValue)
    {
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Blue, levelBlueTeam, maxLevel, killsPerLevelBlueTeam, newValue);
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Red, levelRedTeam, maxLevel, killsPerLevelRedTeam, killsInLevelRedTeam);
    }

    private void OnRedTeamLevelUpdate(int oldValue, int newValue)
    {
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Blue, levelBlueTeam, maxLevel, killsPerLevelBlueTeam, killsInLevelBlueTeam);
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Red, newValue, maxLevel, killsPerLevelRedTeam, killsInLevelRedTeam);
    }

    private void OnRedTeamKillsUpdate(int oldValue, int newValue)
    {
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Blue, levelBlueTeam, maxLevel, killsPerLevelBlueTeam, killsInLevelBlueTeam);
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Red, levelRedTeam, maxLevel, killsPerLevelRedTeam, newValue);
    }

    private void OnMaxLevelUpdate(int oldValue, int newValue)
    {
        GunGameUiManager.Instance.SetTargetLevel(newValue);
    }

    [ClientRpc]
    private void RpcUpdateScore()
    {
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Blue, levelBlueTeam, maxLevel, killsPerLevelBlueTeam, killsInLevelBlueTeam);
        GunGameUiManager.Instance.UpdateScore(TeamEnum.Red, levelRedTeam, maxLevel, killsPerLevelRedTeam, killsInLevelRedTeam);
    }

    [ClientRpc]
    private void RpcAnnounceWin(TeamEnum team)
    {
        GunGameUiManager.Instance.SetWin(team);
        LocalPlayerContext.Instance.PlayAnnouncement(winClip);
    }

    [ClientRpc]
    private void RpcLevelUp(TeamEnum team)
    {
        if (LocalPlayerContext.Instance.player.team == team)
            LocalPlayerContext.Instance.PlayAnnouncement(levelUpClip);
    }

    private bool IsReachedMaxLevel(int level)
    {
        return level == weaponNamesPerLevelList.Count;
    }

    private void AssignWeapons(TeamEnum team, int level)
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.GetCharacters(team);
        string weaponName = weaponNamesPerLevelList[level];

        foreach (FpsCharacter fpsCharacter in fpsCharacters)
        {
            if (!(fpsCharacter is FpsHumanoidCharacter))
                continue;
            ((FpsHumanoidCharacter)fpsCharacter).ServerCmdGetWeapon(weaponName, 0);
        }
    }

    private void TeleportPlayersToSpawn()
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.characterList;
        foreach (FpsCharacter fpsCharacter in fpsCharacters) 
        {
            if (!fpsCharacter || !fpsCharacter.isServer) continue;

            fpsCharacter.onSpawnEvent.Invoke();
            PlayerManager.Instance.TeleportCharacterToSpawnPoint(fpsCharacter);
        }
    }

    private void RestorePlayerHealth()
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.characterList;
        foreach (FpsCharacter fpsCharacter in fpsCharacters)
        {
            if (!fpsCharacter || !fpsCharacter.isServer) continue;
            fpsCharacter.health = fpsCharacter.maxHealth;
        }
    }
}

