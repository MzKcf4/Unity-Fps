using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiHitMarker : MonoBehaviour
{
    public static UiHitMarker Instance;
    
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ShowHitMarker()
    {
        gameObject.SetActive(true);
        Invoke("Hide" , 0.1f);
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
