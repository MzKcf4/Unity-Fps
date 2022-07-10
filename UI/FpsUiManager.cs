using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsUiManager : MonoBehaviour
{
	public static FpsUiManager Instance;
	
	[SerializeField]
	private TextMeshProUGUI healthText;
	[SerializeField]
	private TextMeshProUGUI progressText;
	[SerializeField]
	private TextMeshProUGUI ammoText;
    
    [SerializeField]
    private GameObject killFeedPanel;
    [SerializeField]
    private GameObject killListingPrefab;
    
    [SerializeField]
    private GameObject scopeContainer;
    [SerializeField]
    private GameObject crosshairContainer;
	[SerializeField]
	private GameObject infoPanelContainer;
	
	void Awake()
	{
		Instance = this;
	}
	
    void Start()
    {
	    LocalPlayerContext.Instance.onWeaponShootEvent.AddListener(OnWeaponShoot);
	    LocalPlayerContext.Instance.onHealthUpdateEvent.AddListener(OnHealthUpdate);
	    // ProgressionManager.Instance.onProgressUpdateEvent.AddListener(OnProgressionUpdate);
    }

    void Update()
    {
        
    }
    
	protected void OnWeaponShoot()
	{
		Crosshair.Instance.DoLerp();
	}
	
	public void OnWeaponAmmoUpdate(int currentClip)
	{
		ammoText.SetText(currentClip.ToString());
	}

	public void OnWeaponAmmoUpdate(int currentClip , int backAmmo)
	{
		ammoText.SetText(currentClip.ToString() + " / " + backAmmo);
	}

	protected void OnHealthUpdate(int newHealth ,int maxHealth)
	{
		healthText.SetText(newHealth + "/" + maxHealth);
	}
    
    public void AddNewKillListing(DamageInfo damageInfo, string victim)
    {
		if(string.IsNullOrEmpty(damageInfo.damageWeaponName))
				return;

        GameObject killListingObj = Instantiate(killListingPrefab , killFeedPanel.transform);
        killListingObj.transform.SetAsLastSibling();
        KillListing killListing = killListingObj.GetComponent<KillListing>();

		E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(damageInfo.damageWeaponName);
		bool isHeadshot = damageInfo.bodyPart == BodyPart.Head;
		killListing.SetKillInfo(damageInfo.damageSource, victim, dbWeaponInfo.f_display_name, isHeadshot);
		
	}
	
	protected void OnProgressionUpdate()
	{
		/*
		ProgressionState currState = ProgressionManager.Instance.progressionState;
		int stage = ProgressionManager.Instance.currentStage + 1;
		int targetKills = ProgressionManager.Instance.stageTargetKills;
		int currKills = ProgressionManager.Instance.stageCurrentKills;
		if(currState == ProgressionState.Started)
		{
			int timeRemain = ProgressionManager.Instance.stageTimeDisplay;
			string topText = "Stage : " + stage + " ( " + timeRemain + " )";
			string bottomText = "Kills : " + currKills + " / " + targetKills;
			progressText.text = topText + "\n" + bottomText;
		}
		else if (currState == ProgressionState.Rest)
		{
			int timeRemain = ProgressionManager.Instance.restTimeDisplay;
			progressText.text =  "Stage " + (stage+1) + " in : " + timeRemain;
		}
		else if (currState == ProgressionState.Enraged)
		{
			string topText = "Stage : " + stage + " ( Enraged )";
			string bottomText = "Kills : " + currKills + " / " + targetKills;
			progressText.text = topText + "\n" + bottomText;
		}
		else
		{
			progressText.text = "";
		}
		*/
	}
    
    public void ToggleScope(bool enable)
    {
        scopeContainer.SetActive(enable);
    }
    
    public void ToggleCrosshair(bool enable)
    {
        crosshairContainer.SetActive(enable);
    }

	public Transform GetInfoPanel() 
	{
		return infoPanelContainer.transform;
	}
}
