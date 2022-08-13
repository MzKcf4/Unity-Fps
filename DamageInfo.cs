using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DamageInfo
{
    // Note that this value will be updated to calculated final damage after FpsEntity's TakeDamage
	public int damage = 0;
	public BodyPart bodyPart = BodyPart.Chest;
    public string damageSource = "";
    public Vector3 hitPoint;
    // ToDo : Consider replace position with attacker.
    public NetworkIdentity attackerNetIdentity;
    public Vector3 damageSourcePosition = Vector3.zero;
    public string damageWeaponName = "";
    // For calculating damage reduction over distance
    public float weaponRangeModifier = 1f;

    public bool isFromWeapon = true;

    // This cannot be sync over the network , becareful when access this in client side
    public FpsCharacter attacker;
    
	
	public static DamageInfo AsDamageInfo(int dmg, FpsCharacter attacker)
	{
        return new DamageInfo {
            bodyPart = BodyPart.Chest,
            damage = dmg,
            damageSource = attacker.characterName,
            attackerNetIdentity = attacker.netIdentity,
            attacker = attacker,
            isFromWeapon = false
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
            damageInfo.attackerNetIdentity = fromWeapon.owner.netIdentity;
            damageInfo.isFromWeapon = true;
            damageInfo.attacker = fromWeapon.owner;
        }
        
        return damageInfo;
    }
    
    public float GetDamageDistance()
    {
        if (isFromWeapon)
            return Vector3.Distance(hitPoint, damageSourcePosition);
        else
            return 1f;
    }
}