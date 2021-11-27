using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class TestAnimance : MonoBehaviour
{
	private AnimancerComponent animancer;
	
	[SerializeField]
	private ClipTransition runClip;
	
    // Start is called before the first frame update
    void Start()
    {
	    animancer = GetComponent<AnimancerComponent>();
	    animancer.Play(runClip);
    }
	
	void OnEnable()
	{
		animancer = GetComponent<AnimancerComponent>();
		animancer.Play(runClip);
	}
	
    // Update is called once per frame
    void Update()
    {
        
    }
}
