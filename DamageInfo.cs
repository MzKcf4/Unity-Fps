using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
	public int damage = 0;
	public BodyPart bodyPart = BodyPart.Chest;
	public Vector3 hitPoint;
    public string damageSource = "";
    public Vector3 damageSourcePosition = Vector3.zero;
	
	public static DamageInfo AsDamageInfo(int dmg, BodyPart bodyPart, Vector3 hitPoint)
	{
		return new DamageInfo {
			damage = dmg
		};
	}
	
    
    public static DamageInfo AsDamageInfo(FpsWeapon fromWeapon, FpsHitbox hitbox , Vector3 hitPoint)
    {
        DamageInfo damageInfo = new DamageInfo(){
            damage = fromWeapon.damage,
            bodyPart = hitbox.bodyPart,
            hitPoint = hitPoint,
        };
        
        if(fromWeapon != null)
        {
            damageInfo.damageSource = fromWeapon.owner.characterName;
            damageInfo.damageSourcePosition = fromWeapon.owner.transform.position + Vector3.up;
        }
        
        return damageInfo;
    }
}