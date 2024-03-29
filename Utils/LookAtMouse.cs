﻿using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
 
public class LookAtMouse : MonoBehaviour
{
	public GameObject pointObj;
	// speed is the rate at which the object will rotate
	public float speed;
 
	void FixedUpdate () 
	{
		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Camera.main.transform.forward, Camera.main.transform.position + Camera.main.transform.forward * 10f);
		// Plane playerPlane = new Plane(Vector3.right, transform.position);
 
		// Generate a ray from the cursor position
		Ray ray = Camera.main.ScreenPointToRay (Mouse.current.position.ReadValue());
 
		// Determine the point where the cursor ray intersects the plane.
		// This will be the point that the object must look towards to be looking at the mouse.
		// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
		//   then find the point along that ray that meets that distance.  This will be the point
		//   to look at.
		float hitdist = 0.0f;
		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast (ray, out hitdist)) 
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 targetPoint = ray.GetPoint(hitdist);
			
			if(pointObj != null)
				pointObj.transform.position = targetPoint;
 
			// Determine the target rotation.  This is the rotation if the transform looks at the target point.
			// Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
 
			// Smoothly rotate towards the target point.
			// transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
		}
	}
}