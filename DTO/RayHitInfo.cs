using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayHitInfo
{
	public Vector3 hitPoint;
	public GameObject hitObject;
    public Vector3 normal;
    
    public HitPointInfoDto asHitPointInfoDto()
    {
        return new HitPointInfoDto()
        {
            hitPoint = this.hitPoint,
            hitPointNormal = this.normal
        };
    }
}
