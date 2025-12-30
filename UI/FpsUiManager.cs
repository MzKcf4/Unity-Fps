using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

	[SerializeField]
	private Image weaponTypeImage;
	[SerializeField]
	private Sprite weaponTypeSpriteRifle;
	[SerializeField]
	private Sprite weaponTypeSpriteSniper;
	[SerializeField]
	private Sprite weaponTypeSpriteMg;
	[SerializeField]
	private Sprite weaponTypeSpritePistol;
	[SerializeField]
	private Sprite weaponTypeSpriteShotgun;
	[SerializeField]
	private Sprite weaponTypeSpriteSmg;

	private readonly Dictionary<WeaponCategory , Sprite> weaponTypeSprite = new Dictionary<WeaponCategory , Sprite>();

	void Awake()
	{
		Instance = this;
	}
	
    void Start()
    {
	    LocalPlayerContext.Instance.onWeaponShootEvent.AddListener(OnWeaponShoot);
	    LocalPlayerContext.Instance.onHealthUpdateEvent.AddListener(OnHealthUpdate);
		LocalPlayerContext.Instance.OnWeaponDeployEvent.AddListener(OnWeaponDeploy);

		weaponTypeSprite[WeaponCategory.Shotgun] = weaponTypeSpriteShotgun;
		weaponTypeSprite[WeaponCategory.Sniper] = weaponTypeSpriteSniper;
		weaponTypeSprite[WeaponCategory.Smg] = weaponTypeSpriteSmg;
		weaponTypeSprite[WeaponCategory.Rifle] = weaponTypeSpriteRifle;
		weaponTypeSprite[WeaponCategory.Mg] = weaponTypeSpriteMg;
		weaponTypeSprite[WeaponCategory.Pistol] = weaponTypeSpritePistol;
		weaponTypeSprite[WeaponCategory.Melee] = null;
	}

    void Update()
    {
        
    }
    
	protected void OnWeaponShoot()
	{
		Crosshair.Instance.DoLerp();
	}

	private void OnWeaponDeploy(FpsWeapon fpsWeapon)
	{
		if (fpsWeapon == null) return;
		weaponTypeImage.sprite = weaponTypeSprite[fpsWeapon.weaponCategory];

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
		bool isWallPen = damageInfo.wallsPenetrated > 0;
        killListing.SetKillInfo(damageInfo.damageSource, victim, dbWeaponInfo.f_display_name, isHeadshot, isWallPen);
		
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
