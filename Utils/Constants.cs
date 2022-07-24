using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
	public static readonly string TAG_PLAYER = "Player";
	public static readonly string TAG_ENEMY = "Enemy";
    public static readonly string TAG_CHAR_WEAPON_ROOT = "CharWeaponRoot";
    public static readonly string TAG_BOT_WAYPOINT = "BotWaypoint";
    public static readonly string TAG_TEAM_A_SPAWN = "TeamA_Spawn";
    public static readonly string TAG_TEAM_B_SPAWN = "TeamB_Spawn";
    public static readonly string TAG_MONSTER_SPAWN = "Monster_Spawn";
    public static readonly string TAG_MUZZLE_VIEW = "Muzzle";

    public static readonly string LAYER_DEFAULT = "Default";
    public static readonly string LAYER_IGNORE_RAYCAST = "Ignore Raycast";
    public static readonly string LAYER_HITBOX = "Hitbox";
    public static readonly string LAYER_GROUND = "Ground";
    public static readonly string LAYER_LOCAL_PLAYER_MODEL = "LocalPlayerModel";
    public static readonly string LAYER_LOCAL_PLAYER_HITBOX = "LocalPlayerHitbox";
    public static readonly string LAYER_CHARACTER_MODEL = "CharacterModel";
    public static readonly string LAYER_CHARACTER = "Character";
    public static readonly string LAYER_FIRST_PERSON_VIEW = "FirstPersonView";
    public static readonly string[] LAYER_CHARACTER_RAYCAST_LAYER = { LAYER_LOCAL_PLAYER_MODEL , LAYER_CHARACTER };

    public static readonly string LABEL_WEAPON_RESOURCE = "WeaponResource";
    public static readonly string ADDRESS_LABEL_MONSTER_PREFAB = "MonsterPrefab";
    public static readonly string ADDRESS_LABEL_EFFECT_PREFAB = "EffectPrefab";

    public static readonly string EFFECT_NAME_GREEN_SPHERE_BLAST = "GreenSphereBlast".ToLower();

    public static readonly string WEAPON_SOUND_FIRE = "fire";
    
	public static readonly int WEAPON_SLOT_1 = 0;
	public static readonly int WEAPON_SLOT_2 = 1;
	public static readonly int WEAPON_SLOT_3 = 2;
	public static readonly int WEAPON_SLOT_MAX = 3;

    public static readonly string SETTING_KEY_LOCAL_PLAYER_SETTINGS = "LocalPlayerSetting";

    public static readonly string ADDITIONAL_KEY_DM_SELECTED_WEAPON = "dm_selected_weapon";
    public static readonly string ADDITIONAL_KEY_DM_SELECTED_WEAPON_SECONDARY = "dm_selected_weapon_secondary";

    public static readonly string ADDITIONAL_INFO_MONSTER_ID = "monster_id";
    public static readonly string ADDITIONAL_INFO_AMMOPACK = "ammopack";

    public static readonly string ACTION_KEY_MELEE = "melee";
}
