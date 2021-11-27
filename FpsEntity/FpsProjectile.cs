using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FpsProjectile : NetworkBehaviour
{
    public LayerMask collideWithLayer;
    
    public Vector3 forwardRotation = Vector3.zero;
	private Vector3 shootDir;
	public float speed = 10f;
    
    private Vector3 positionLastFrame;
		
	public void Setup(Vector3 shootDir)
	{
		this.shootDir = shootDir;
		
	}
	
	// Start is called before the first frame update
	void Start()
	{
        transform.eulerAngles = forwardRotation;
        positionLastFrame = transform.position;
        ServerContext.Instance.DestroyAfterSeconds(2 , gameObject);
	}

	// Update is called once per frame
	void Update()
	{
		transform.position += shootDir * speed * Time.deltaTime;
	}
    
    void FixedUpdate()
    {

    }
}
