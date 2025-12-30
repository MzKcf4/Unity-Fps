using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsProjectile : MonoBehaviour
{
    public LayerMask collideWithLayer;
	public GameObject hitEffect;
    
	private Vector3 shootDir;
	public float speed = 10f;

	public void Setup(Vector3 shootDir)
	{
		this.shootDir = shootDir;
		transform.LookAt(shootDir);	
	}
	
	void Start()
	{

    }

	// Update is called once per frame
	void Update()
	{
		transform.position += shootDir * speed * Time.deltaTime;
	}

    private void OnTriggerEnter(Collider other)
    {
		if(!other.CompareTag(Constants.TAG_PLAYER)) return;

		Debug.Log(other.gameObject);
		if(hitEffect != null) 
		{
			hitEffect.SetActive(true);
			hitEffect.transform.SetParent(null, true);
			Destroy(hitEffect, 1f);
		}
        Destroy(gameObject);
    }
}
