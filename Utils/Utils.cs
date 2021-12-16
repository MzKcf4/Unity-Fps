using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Utils
{    
    public static RayHitInfo CastRayAndGetHitInfo(FpsCharacter fpsCharacter, Vector3 fromPos, Vector3 direction, int mask , float spread)
    {
        Vector3 directionWithSpread = GetRandomizedSpreadDirection(fromPos , direction , spread);
        Vector3 originPos = fromPos;
        
        RaycastHit[] hits;
        hits = Physics.RaycastAll(originPos ,directionWithSpread, 100.0f , mask);
        
        // ascending distance
        List<RaycastHit> sortedHits = hits.OrderBy(hit=>Vector3.Distance(fromPos , hit.transform.position)).ToList();
        
        foreach(RaycastHit hit in sortedHits)
        {
            GameObject hitObject = hit.transform.gameObject;
            FpsHitbox hitbox = hitObject.GetComponent<FpsHitbox>();
            if(hitbox != null)
            {
                FpsEntity hitEntity = hitbox.fpsEntity;
                // Hit self for some reason , ignore and keep processing
                if(hitEntity == fpsCharacter)
                    continue;
            }
            
            RayHitInfo info = new RayHitInfo(){
                hitPoint = hit.point,
                hitObject = hit.transform.gameObject,
                normal = hit.normal
            };
            return info;
        }
        return null;
    }
    
    public static Vector3 GetRandomizedSpreadDirection(Vector3 fromPos , Vector3 direction, float spread)
    {
        // random vector within circle
        float rad = Random.Range(0.0f , 360.0f) * Mathf.Rad2Deg;
        float spreadX = Random.Range(0.0f , spread/2.0f) * Mathf.Cos(rad);
        float spreadY = Random.Range(0.0f , spread/2.0f) * Mathf.Sin(rad);
        float spreadZ = Random.Range(0.0f , spread/2.0f) * Mathf.Cos(rad);
        Vector3 spreadVec = new Vector3(spreadX , spreadY , spreadZ);
        Vector3 directionWithSpread = direction + spreadVec;
        
        return directionWithSpread;
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
