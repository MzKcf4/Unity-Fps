using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillListing : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI killerDisplay;
    [SerializeField] TextMeshProUGUI killedDisplay;
    [SerializeField] TextMeshProUGUI weaponDisplay;
    [SerializeField] Image headshotIcon;
    [SerializeField] Image wallPenIcon;

    void Start()
    {
        Destroy(gameObject , 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void SetKillInfo(string killer, string victim, string killWeapon , bool isHeadshot, bool isWallPen)
    {
        killerDisplay.text = killer;
        killedDisplay.text = victim;
        weaponDisplay.text = "[" + killWeapon + "]";
        headshotIcon.enabled = isHeadshot;
        wallPenIcon.enabled = isWallPen;
    }
}
