using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillListing : MonoBehaviour
{
    [SerializeField] Text killerDisplay;
    [SerializeField] Text killedDisplay;
    [SerializeField] Text weaponDisplay;
    
    void Start()
    {
        Destroy(gameObject , 8f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void SetKillInfo(string killer, string victim, string killWeapon)
    {
        killerDisplay.text = killer;
        killedDisplay.text = victim;
        weaponDisplay.text = "[" + killWeapon + "]";
    }
}
