using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
	public static readonly string TAG_PLAYER = "Player";
	public static readonly string TAG_ENEMY = "Enemy";
    public static readonly string TAG_CHAR_WEAPON_ROOT = "CharWeaponRoot";
	
	public static readonly string LAYER_DEFAULT = "Default";
	public static readonly string LAYER_HITBOX = "Hitbox";
    public static readonly string LAYER_GROUND = "Ground";
    public static readonly string LAYER_LOCAL_PLAYER_MODEL = "LocalPlayerModel";
    public static readonly string LAYER_LOCAL_PLAYER_HITBOX = "LocalPlayerHitbox";
    public static readonly string LAYER_CHARACTER_MODEL = "CharacterModel";
    
	public static readonly string WEAPON_SOUND_FIRE = "fire";
    
	public static readonly int WEAPON_SLOT_1 = 0;
	public static readonly int WEAPON_SLOT_2 = 1;
	public static readonly int WEAPON_SLOT_3 = 2;
	public static readonly int WEAPON_SLOT_MAX = 3;
    
    public static readonly string SETTING_KEY_AUDIO_MASTER_VOLUME = "AudioMasterVolume";
    public static readonly string SETTING_KEY_MOUSE_SPEED = "MouseSpeed";
    public static readonly string SETTING_KEY_MOUSE_SPEED_ZOOMED = "MouseSpeedZoomed";
}
