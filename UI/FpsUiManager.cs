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
	
	void Awake()
	{
		Instance = this;
	}
	
    void Start()
    {
	    PlayerContext.Instance.onWeaponShootEvent.AddListener(OnWeaponShoot);
	    PlayerContext.Instance.onHealthUpdateEvent.AddListener(OnHealthUpdate);
	    ProgressionManager.Instance.onProgressUpdateEvent.AddListener(OnProgressionUpdate);
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
	
	protected void OnHealthUpdate(int newHealth ,int maxHealth)
	{
		healthText.SetText(newHealth + "/" + maxHealth);
	}
    
    public void AddNewKillListing(string killer , string killed)
    {
        GameObject killListingObj = Instantiate(killListingPrefab , killFeedPanel.transform);
        killListingObj.transform.SetAsLastSibling();
        KillListing killListing = killListingObj.GetComponent<KillListing>();
        killListing.SetNames(killer , killed);
    }
	
	protected void OnProgressionUpdate()
	{
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
	}
}
