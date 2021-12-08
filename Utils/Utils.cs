using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Utils
{
	public static void AddAnimationEvent(AnimationClip clip, string functionName , float timeInPercent)
	{
		AnimationEvent evt = new AnimationEvent();
		evt.functionName = functionName;
		evt.time = timeInPercent;
		clip.AddEvent(evt);
	}
    
    public static Vector3 GetMouseAimByPlane()
    {
        Transform cameraTransform = Camera.main.transform;
        Plane p = new Plane(cameraTransform.up, cameraTransform.position);
        Ray r = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float d;
        if(p.Raycast(r, out d)) {
            Vector3 v = r.GetPoint(d);
            return v;
        } else {
            Debug.Log("Nothing hits");    
        }
        
        return Vector3.zero;
    }
	
	public static Vector3 GetMouseAim()
	{
		Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;
		if(Physics.Raycast(ray , out hit , 1000.0f))
		{
			return hit.point;
		}
        else
        {
            return ray.GetPoint(100.0f);
        }
		return Vector3.zero;
	}
	
	public static Vector3 GetMouseAim(LayerMask mask)
	{
		Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;
		if(Physics.Raycast(ray , out hit , 1000.0f , mask))
		{
			Debug.Log(hit);
			return hit.point;
		}
		Debug.Log("Nothing hits");
		return Vector3.zero;
	}
	
	
	public static GameObject GetMouseAimHitTarget()
	{
		Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;
		if(Physics.Raycast(ray , out hit , 1000.0f))
		{
			return hit.transform.gameObject;
		}
		Debug.Log("Nothing hits");
		return null;
	}
	
	public static GameObject GetMouseAimHitTarget(LayerMask mask)
	{
		Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;
		if(Physics.Raycast(ray , out hit , 1000.0f , mask))
		{
			return hit.transform.gameObject;
		}
		Debug.Log("Nothing hits");
		return null;
	}

    public static RayHitInfo GetMouseAimHitInfo(LayerMask mask, float spread)
    {
        return GetMouseAimHitInfo(mask.value , spread);
    }

	public static RayHitInfo GetMouseAimHitInfo(int mask, float spread)
	{        
        return CastRayAndGetHitInfo(Camera.main.transform, mask , spread);
        
	}
    
    public static RayHitInfo CastRayAndGetHitInfo(Transform origin, int mask , float spread)
    {
        return CastRayAndGetHitInfo(origin.position , origin.forward , mask , spread);
    }
    
    public static RayHitInfo CastRayAndGetHitInfo(Vector3 fromPos, Vector3 direction, int mask , float spread)
    {
        // random vector within circle
        float rad = Random.Range(0.0f , 360.0f) * Mathf.Rad2Deg;
        float spreadX = Random.Range(0.0f , spread/2.0f) * Mathf.Cos(rad);
        float spreadY = Random.Range(0.0f , spread/2.0f) * Mathf.Sin(rad);
        float spreadZ = Random.Range(0.0f , spread/2.0f) * Mathf.Cos(rad);
        Vector3 spreadVec = new Vector3(spreadX , spreadY , spreadZ);
        Vector3 directionWithSpread = direction + spreadVec;
        Vector3 originPos = fromPos;
        
        RaycastHit hit;
        if(Physics.Raycast(originPos ,directionWithSpread, out hit , 1000.0f , mask))
        {
            RayHitInfo info = new RayHitInfo(){
                hitPoint = hit.point,
                hitObject = hit.transform.gameObject,
                normal = hit.normal
            };
            return info;
        }
        return null;
    }
	
	public static T GetRandomElement<T>(List<T> list)
	{
		int idx = Random.Range(0 , list.Count);
		return list[idx];
	}
    
    public static Vector3 GetDirection(Vector3 from , Vector3 to)
    {
        return (to - from).normalized;
    }
    
    
    public static void ChangeTagRecursively(GameObject gameObject , string newTag, bool includeParent)
    {
        if(includeParent)
            gameObject.tag = newTag;
            
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))  
            child.gameObject.tag = newTag;
    }
    
    public static void ChangeLayerRecursively(GameObject gameObject, string newLayerName, bool includeParent)
    {
        if(includeParent)
            gameObject.layer = LayerMask.NameToLayer(newLayerName);
            
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = LayerMask.NameToLayer(newLayerName);
    }
    
    public static void ReplaceLayerRecursively(GameObject gameObject,string oldLayerName, string newLayerName)
    {
        // if(includeParent)
        //     gameObject.layer = LayerMask.NameToLayer(newLayerName);
            
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if(LayerMask.LayerToName(child.gameObject.layer).Equals(oldLayerName))
            {
                child.gameObject.layer = LayerMask.NameToLayer(newLayerName);    
            }
        }
            
    }
    
    
    public static void LookAtFixedY(Transform from , Transform to){
        Vector3 targetPosition = new Vector3(to.position.x , from.position.y , to.position.z);
        from.LookAt(targetPosition);
    }
    
    public static bool WithinChance(float probability)
    {
        float range = Random.RandomRange(0f , 1.0f);
        return range < probability;
    }
}
