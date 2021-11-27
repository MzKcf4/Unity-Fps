using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfoDto
{
    public int damage = 20;
    public int clipSize = 30;
    public float shootInterval = 0.1f;
    public WeaponCategory weaponCategory = WeaponCategory.Rifle;
    public WeaponReloadType reloadType = WeaponReloadType.Clip;
    public float reloadTime = 3f;
    
    public float reloadTime_PalletStart = 0.2f;
    public float reloadTime_PalletInsert = 0.2f;
    public float reloadTime_PalletEnd = 0.2f;
    
    public float drawTime = 2f;
    public int palletPerShot = 1;
    public float spread = 0.1f;
    
    public static WeaponInfoDto AsWeaponInfoDto(string weaponName)
    {
        E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(weaponName);
        
        WeaponInfoDto dto = new WeaponInfoDto()
        {
            damage = dbWeaponInfo.f_base_damage,
            clipSize = dbWeaponInfo.f_clip_size,
            shootInterval = dbWeaponInfo.f_shoot_interval,
            reloadType = dbWeaponInfo.f_reload_type,
            weaponCategory = dbWeaponInfo.f_category,
            reloadTime = dbWeaponInfo.f_reload_time,
            reloadTime_PalletStart = dbWeaponInfo.f_reload_time_pallet_start,
            reloadTime_PalletInsert = dbWeaponInfo.f_reload_time_pallet_insert,
            reloadTime_PalletEnd = dbWeaponInfo.f_reload_time_pallet_end,
            drawTime = dbWeaponInfo.f_draw_time,
            palletPerShot = dbWeaponInfo.f_pallet_per_shot,
            spread = dbWeaponInfo.f_spread
        };
        return dto;
    }
    
}
