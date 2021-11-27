using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestSpawnTrigger : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
	// OnTriggerEnter is called when the Collider other enters the trigger.
	protected void OnTriggerEnter(Collider other)
	{
		if(!isServer)	return;
		/*
		if(other.CompareTag(Constants.TAG_PLAYER))
        ProgressionManager.Instance.StartStage(0);
        */
	}
}
