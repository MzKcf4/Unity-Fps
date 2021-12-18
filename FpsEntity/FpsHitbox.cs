using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsHitbox : MonoBehaviour
{
	public BodyPart bodyPart = BodyPart.Chest;
	[HideInInspector] public FpsEntity fpsEntity;
	
	void Start()
	{
		fpsEntity = GetComponentInParent<FpsEntity>();
		if(fpsEntity == null)
			Debug.LogWarning("No FpsEntity associated with hitBox!");
	}
}
