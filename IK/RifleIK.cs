using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class RifleIK : MonoBehaviour
{
	public FullBodyBipedIK ik;
	
	public Transform leftHandTarget , rightHandTarget;
	
	// LateUpdate is called every frame, if the Behaviour is enabled.
	protected void LateUpdate()
	{
		ik.solver.leftHandEffector.position = leftHandTarget.position;
		ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
		ik.solver.leftHandEffector.positionWeight = 1f;
		ik.solver.leftHandEffector.rotationWeight = 1f;
	}
}
