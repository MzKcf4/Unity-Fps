using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Done by following this video : 
// https://www.youtube.com/watch?v=-7DIdKTNjfQ

public class Crosshair : MonoBehaviour
{
	public static Crosshair Instance;
	
	private RectTransform container;
	
	public float restingSize = 75f;
	public float maxSize = 200f;
	public float decreaseSpeed = 2f;
	public float increaseSpeed = 10f;
	private float currentSize;
	
	public float waitUntilRest = 0.2f;
	private float currWait = 0f;
	public float sizePerLerp = 10f;
	
	void Awake()
	{
		Instance = this;
		container = GetComponent<RectTransform>();
	}
	
	// Start is called before the first frame update
	void Start()
	{
	    
	}

	// Update is called once per frame
	void Update()
	{
		if(container == null)	return;
		
		if(currWait <= 0)
			currentSize = Mathf.Lerp(currentSize, restingSize , Time.deltaTime * decreaseSpeed);
		else
			currWait -= Time.deltaTime;
			
		container.sizeDelta = new Vector2(currentSize , currentSize);
	}
    
	public void DoLerp()
	{
		currWait = waitUntilRest;
		float newSize = Mathf.Clamp(currentSize + sizePerLerp , restingSize , maxSize);
		currentSize = Mathf.Lerp(currentSize, newSize , Time.deltaTime * increaseSpeed);
	}
}
