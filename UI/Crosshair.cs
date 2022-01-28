using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Done by following this video : 
// https://www.youtube.com/watch?v=-7DIdKTNjfQ

public class Crosshair : MonoBehaviour
{
	public static Crosshair Instance;
	
	private RectTransform container;
	[SerializeField] private RectTransform topRect;
	[SerializeField] private RectTransform bottomRect;
	[SerializeField] private RectTransform leftRect;
	[SerializeField] private RectTransform rightRect;
	[SerializeField] private GameObject centerDot;

	private bool isLerp = true;
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
		if(container == null || !isLerp)	return;
		
		if(currWait <= 0)
			currentSize = Mathf.Lerp(currentSize, restingSize , Time.deltaTime * decreaseSpeed);
		else
			currWait -= Time.deltaTime;
			
		container.sizeDelta = new Vector2(currentSize , currentSize);
	}

	public void SetSize(CrosshairSizeEnum newSize)
	{
		if (newSize == CrosshairSizeEnum.Standard)
		{
			restingSize = 50f;
			topRect.sizeDelta = new Vector2(2f, 15f);
			bottomRect.sizeDelta = new Vector2(2f, 15f);
			leftRect.sizeDelta = new Vector2(15f, 2f);
			rightRect.sizeDelta = new Vector2(15f, 2f);
			centerDot.SetActive(true);
		}
		else
		{
			restingSize = 30f;
			topRect.sizeDelta = new Vector2(1.5f, 10f);
			bottomRect.sizeDelta = new Vector2(1.5f, 10f);
			leftRect.sizeDelta = new Vector2(10f, 1.5f);
			rightRect.sizeDelta = new Vector2(10f, 1.5f);
			centerDot.SetActive(false);
		}
	}

	public void SetLerp(bool isLerp) 
	{
		this.isLerp = isLerp;
	}
    
	public void DoLerp()
	{
		currWait = waitUntilRest;
		float newSize = Mathf.Clamp(currentSize + sizePerLerp , restingSize , maxSize);
		currentSize = Mathf.Lerp(currentSize, newSize , Time.deltaTime * increaseSpeed);
	}
}
