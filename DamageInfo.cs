using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
	public int damage = 0;
	public BodyPart bodyPart = BodyPart.Chest;
    public string damageSource = "";
    public Vector3 hitPoint;
    public Vector3 damageSourcePosition = Vector3.zero;
    public string damageWeaponName = "";
    // For calculating damage reduction over distance
    public float weaponRangeModifier = 1f;
	
	public static DamageInfo AsDamageInfo(int dmg, BodyPart bodyPart, Vector3 hitPoint)
	{
		return new DamageInfo {
			damage = dmg
		};
	}
	
    public static DamageInfo AsDamageInfo(FpsWeapon fromWeapon, FpsHitbox hitbox , Vector3 hitPoint)
    {
        DamageInfo damageInfo = new DamageInfo(){
            bodyPart = hitbox.bodyPart,
            hitPoint = hitPoint,
        };
        
        if(fromWeapon != null)
        {
            damageInfo.damageWeaponName = fromWeapon.weaponName;
            damageInfo.damage = fromWeapon.damage;
            damageInfo.damageSource = fromWeapon.owner.characterName;
            damageInfo.damageSourcePosition = fromWeapon.owner.transform.position + Vector3.up;
            damageInfo.weaponRangeModifier = fromWeapon.rangeModifier;
        }
        
        return damageInfo;
    }
    
    public float GetDamageDistance()
    {
        return Vector3.Distance(hitPoint , damageSourcePosition);
    }
}