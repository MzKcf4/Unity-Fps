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
		PlayEffect(effectName, pos, lifeTime, null);
	}

	public void PlayEffect(string effectName, Vector3 pos, float lifeTime, Transform objectAttachTo)
	{
		if (!StreamingAssetManager.Instance.DictEffectNameToPrefab.ContainsKey(effectName))
		{
			Debug.Log("Effect name NOT found : " + effectName);
			return;
		}

		GameObject obj = Instantiate(StreamingAssetManager.Instance.DictEffectNameToPrefab[effectName], pos, Quaternion.identity);
		if (objectAttachTo != null)
			obj.transform.SetParent(objectAttachTo.transform);

		MMFeedbacks feedback = obj.GetComponent<MMFeedbacks>();

		if(feedback != null)
			feedback.PlayFeedbacks();

		//ToDo: pool the effects
		Destroy(obj, lifeTime);
	}
}
