using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmBoneToWeaponBone : MonoBehaviour
{
	// The partial bone names that should exist in the weapon's bone name
	static string[] partialBoneNames = new string[]
	{
		"Bip01_L_UpperArm",
		// "Bip01_L_ForeTwist",
		"Bip01_L_Forearm",
		"Bip01_L_Hand",
		

		"Bip01_L_Finger0",
		"Bip01_L_Finger01",
		"Bip01_L_Finger02",
		"Bip01_L_Finger1",
		"Bip01_L_Finger11",
		"Bip01_L_Finger12",
		"Bip01_L_Finger2",
		"Bip01_L_Finger21",
		"Bip01_L_Finger22",
		"Bip01_L_Finger3",
		"Bip01_L_Finger31",
		"Bip01_L_Finger32",
		"Bip01_L_Finger4",
		"Bip01_L_Finger41",
		"Bip01_L_Finger42",
		
		"Bip01_R_UpperArm",
		// "Bip01_R_ForeTwist",
		"Bip01_R_Forearm",
		"Bip01_R_Hand",
		
		"Bip01_R_Finger0",
		"Bip01_R_Finger01",
		"Bip01_R_Finger02",
		"Bip01_R_Finger1",
		"Bip01_R_Finger11",
		"Bip01_R_Finger12",
		"Bip01_R_Finger2",
		"Bip01_R_Finger21",
		"Bip01_R_Finger22",
		"Bip01_R_Finger3",
		"Bip01_R_Finger31",
		"Bip01_R_Finger32",
		"Bip01_R_Finger4",
		"Bip01_R_Finger41",
		"Bip01_R_Finger42"
		
	};
	
	static string[] partialBoneNamesTFA = new string[]
	{
		"L UpperArm",
		// "L ForeTwist",
		"L Forearm",
		"L Hand",

		"L Finger0",
		"L Finger01",
		"L Finger02",
		"L Finger1",
		"L Finger11",
		"L Finger12",
		"L Finger2",
		"L Finger21",
		"L Finger22",
		"L Finger3",
		"L Finger31",
		"L Finger32",
		"L Finger4",
		"L Finger41",
		"L Finger42",
		
		"R UpperArm",
		// "R ForeTwist",
		"R Forearm",
		"R Hand",

		"R Finger0",
		"R Finger01",
		"R Finger02",
		"R Finger1",
		"R Finger11",
		"R Finger12",
		"R Finger2",
		"R Finger21",
		"R Finger22",
		"R Finger3",
		"R Finger31",
		"R Finger32",
		"R Finger4",
		"R Finger41",
		"R Finger42"
	};
	
	// The bone names that should appear in the hand object
	static string[] boneNames = new string[]
	{
		"ValveBiped.Bip01_L_UpperArm",
		// "ValveBiped.Bip01_L_ForeTwist",
		"ValveBiped.Bip01_L_Forearm",
		"ValveBiped.Bip01_L_Hand",
		
		"ValveBiped.Bip01_L_Finger0",
		"ValveBiped.Bip01_L_Finger01",
		"ValveBiped.Bip01_L_Finger02",
		"ValveBiped.Bip01_L_Finger1",
		"ValveBiped.Bip01_L_Finger11",
		"ValveBiped.Bip01_L_Finger12",
		"ValveBiped.Bip01_L_Finger2",
		"ValveBiped.Bip01_L_Finger21",
		"ValveBiped.Bip01_L_Finger22",
		"ValveBiped.Bip01_L_Finger3",
		"ValveBiped.Bip01_L_Finger31",
		"ValveBiped.Bip01_L_Finger32",
		"ValveBiped.Bip01_L_Finger4",
		"ValveBiped.Bip01_L_Finger41",
		"ValveBiped.Bip01_L_Finger42",
		
		"ValveBiped.Bip01_R_UpperArm",
		// "ValveBiped.Bip01_R_ForeTwist",
		"ValveBiped.Bip01_R_Forearm",
		"ValveBiped.Bip01_R_Hand",
		
		"ValveBiped.Bip01_R_Finger0",
		"ValveBiped.Bip01_R_Finger01",
		"ValveBiped.Bip01_R_Finger02",
		"ValveBiped.Bip01_R_Finger1",
		"ValveBiped.Bip01_R_Finger11",
		"ValveBiped.Bip01_R_Finger12",
		"ValveBiped.Bip01_R_Finger2",
		"ValveBiped.Bip01_R_Finger21",
		"ValveBiped.Bip01_R_Finger22",
		"ValveBiped.Bip01_R_Finger3",
		"ValveBiped.Bip01_R_Finger31",
		"ValveBiped.Bip01_R_Finger32",
		"ValveBiped.Bip01_R_Finger4",
		"ValveBiped.Bip01_R_Finger41",
		"ValveBiped.Bip01_R_Finger42"
		
	};

	private Transform[] armBones;
	private Transform[] targetBones;
	public GameObject debugTarget;

	private Vector3[] armBoneOriginalPos;
	private Quaternion[] armBoneOriginalRot;
	
	void Awake()
	{
		armBones = FindBones(gameObject);
		saveOriginalBone();
		if (debugTarget != null)
		{
			targetBones = FindBones(debugTarget);
		}
	}

	private void saveOriginalBone()
	{
		armBoneOriginalPos = new Vector3[armBones.Length];
		armBoneOriginalRot = new Quaternion[armBones.Length];
		for (int i = 0; i < armBones.Length; i++)
		{
			if (armBones[i] == null)
				continue;
			armBoneOriginalPos[i] = armBones[i].localPosition;
			armBoneOriginalRot[i] = armBones[i].rotation;
		}
	}
	
    // Update is called once per frame
    void Update()
    {
	    if(targetBones == null)
	    	return;
    	
    	for(int i = 0 ; i < armBones.Length ; i++)
    	{
    		if(targetBones[i] == null || armBones[i] == null)
    		{
				if (armBones != null)
				{
					// armBones[i].localPosition = Vector3.zero;
					// armBones[i].localRotation = Quaternion.identity;
					armBones[i].localPosition = armBoneOriginalPos[i];
					armBones[i].rotation = armBoneOriginalRot[i];
				}
			}
    		else
    		{
				armBones[i].position = targetBones[i].position;
				// armBones[i].rotation = Quaternion.Euler(targetBones[i].eulerAngles.x + 180f, targetBones[i].eulerAngles.y, targetBones[i].eulerAngles.z);
				armBones[i].rotation = targetBones[i].rotation;
				// Debug.Log(armBones[i].rotation + " -- " + targetBones[i].rotation);
			}
    	}
    }
    
	private Transform[] FindBones(GameObject targetObject)
	{
		Transform[] bones = new Transform[boneNames.Length];
		for(int i = 0 ; i < boneNames.Length ; i++)
		{
			string boneName = boneNames[i];
			// Link the hand's bone to the gun's bone by matching the boneName
			Transform bone = targetObject.transform.FirstOrDefault(x => {
				// Default source weapon bones have "ValveBiped" prefix , remove it.
				string[] nameParts = x.name.Split('.');
				if(nameParts.Length <= 1)
					// TFA bones doesn't have the prefix
					return x.name.Equals(partialBoneNamesTFA[i]);
				else
				{
					return nameParts[1].Equals(partialBoneNames[i] , System.StringComparison.OrdinalIgnoreCase);
				}
			});

			if(bone != null)
				bones[i] = bone;

			/*
			if (bone == null)
				Debug.LogWarning("Hand bone not found : " + boneName);
			else
			{
				bones[i] = bone;
			}
			*/
			
		}
		return bones;
	}
	
	public void AttachToWeapon(GameObject newWeaponObj)
	{
		targetBones = FindBones(newWeaponObj);
	}
}
