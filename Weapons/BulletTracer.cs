using Andtech.ProTracer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
	private float speed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetupAndPlay(Vector3 start , Vector3 endPoint)
	{ 
		this.transform.position = start;
		SpawnTracer(endPoint);
	}

	public void SpawnTracer(Vector3 endPoint)
	{
		// float speed = Speed;
		float offset = 0.0f;

		// Instantiate the tracer graphics
		GameObject bulletObj = Instantiate(StreamingAssetManager.Instance.dictEffectNameToPrefab[Constants.EFFECT_NAME_BULLET_TRACE]);
		Bullet bullet = bulletObj.GetComponent<Bullet>();

		GameObject smokeTrailObj = Instantiate(StreamingAssetManager.Instance.dictEffectNameToPrefab[Constants.EFFECT_NAME_BULLET_TRACE_SMOKE]);
		SmokeTrail smokeTrail = smokeTrailObj.GetComponent<SmokeTrail>();

		// Setup callbacks
		bullet.Completed += OnCompleted;
		smokeTrail.Completed += OnCompleted;

		static void OnCompleted(object sender, System.EventArgs e)
		{
			// Handle complete event here
			if (sender is TracerObject tracerObject)
			{
				Destroy(tracerObject.gameObject);
			}
		}

		// Since start and end point are known, use DrawLine
		bullet.DrawLine(transform.position, endPoint, speed, offset);
		smokeTrail.DrawLine(transform.position, endPoint, speed, offset);

		/*
		// Use different tracer drawing methods depending on the raycast
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, maxQueryDistance))
		{
			// Setup impact callback
			bullet.Arrived += OnImpact;

			void OnImpact(object sender, System.EventArgs e)
			{
				// Handle impact event here
				Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 0.5F);
			}
		}
		else
		{
			// Since we have no end point, use DrawRay
			bullet.DrawRay(transform.position, transform.forward, speed, maxQueryDistance, offset, useGravity);
			smokeTrail.DrawRay(transform.position, transform.forward, speed, 25.0F, offset);
		}*/
	}

}
