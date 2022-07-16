using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;

public class EffectManager : NetworkBehaviour
{
	public static EffectManager Instance;
		
	// Start is called before the first frame update
	void Start()
	{
		Instance = this;    
	}

	// Update is called once per frame
	void Update()
	{
        
	}

	[ClientRpc]
	public void RpcSetInvisible(GameObject obj , Color color)
	{
		Renderer renderer = obj.GetComponent<SkinnedMeshRenderer>();
		if(renderer == null)
			renderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		
		if(renderer)
		{
			foreach(Material material in renderer.materials)
			{
				material.color = color;
			}
		}
	}
	
	public void SetColor(GameObject obj , Color color)
	{
		Renderer renderer = obj.GetComponent<SkinnedMeshRenderer>();
		if(renderer == null)
			renderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		
		if(renderer)
		{
			foreach(Material material in renderer.materials)
			{
				material.color = color;
			}
		}
	}
	
	[ClientRpc]
	public void RpcSetColor(GameObject obj , Color color)
	{
		SetColor(obj, color);
	}

	[ClientRpc]
	public void RpcPlayEffect(string effectName, Vector3 pos , float lifeTime)
	{
		MMFeedbacks feedback = Instantiate(StreamingAssetManager.Instance.DictEffectNameToPrefab[effectName], pos, Quaternion.identity)
			.GetComponent<MMFeedbacks>();
		feedback.PlayFeedbacks();
		//ToDo: pool the effects
		Destroy(feedback.gameObject, lifeTime);
	}
}
