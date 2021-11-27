using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillListing : MonoBehaviour
{
    [SerializeField] Text killerDisplay;
    [SerializeField] Text killedDisplay;
    
    void Start()
    {
        Destroy(gameObject , 8f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetNames(string killer , string killed)
    {
        killerDisplay.text = killer;
        killedDisplay.text = killed;
    }
}
