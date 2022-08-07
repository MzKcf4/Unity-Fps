using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

public class MonsterModeUiManager : MonoBehaviour
{
    public static MonsterModeUiManager Instance;
    [SerializeField] private GameObject weaponButtonPrefab;

    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI restText;
    [SerializeField] private TextMeshProUGUI ammoPackText;
    [SerializeField] private Transform hudPanel;
    [SerializeField] private Transform menuPanel;
    [SerializeField] private Transform weaponSelectionPanel;

    private bool isWeaponUpgradeSelected = false;

    private void Start()
    {
        Instance = this;
        LocalPlayerContext.Instance.onGamemodeMenuToggleEvent.AddListener(ToggleMenuPanel);
    }

    public void UpdateKillText(int newKill , int targetKills)
    {
        killText.text = "Kills : " + newKill + " / " + targetKills;
    }

    public void UpdateAmmoPackText(int newAmmoPack)
    { 
        ammoPackText.text = "Ammo Pack : " + newAmmoPack.ToString();
    }

    public void ToggleMenuPanel()
    {
        bool hudActive = hudPanel.gameObject.activeSelf;
        hudPanel.gameObject.SetActive(!hudActive);
        menuPanel.gameObject.SetActive(hudActive);
    }

    public void UpdateRestCountdown(int countdown)
    {
        restText.text = "Rest : " + countdown.ToString();
    }

    public void UpdateStage(int currStage , int maxStage)
    {
        stageText.text = "Stage : " + currStage + " / " + maxStage;
        SetWeaponUpgradeAvailable(currStage);
    }

    #region WeaponUpgrade

    public void SetWeaponUpgradeAvailable(int stage)
    {
        Utils.DestroyChildren(weaponSelectionPanel);
        isWeaponUpgradeSelected = false;
        PopulateWeaponUpgrade(stage);
    }

    private void PopulateWeaponUpgrade(int stage)
    {
        List<E_weapon_info> weapons = E_weapon_info.FindEntities(e => e.f_active && e.f_monster_level == stage && e.f_category != WeaponCategory.Pistol);

        if (weapons == null || weapons.Count == 0)
            return;

        for (int i = 0; i < weapons.Count; i++)
        {
            AddWeaponButton(weapons[i]);
        }
    }

    private void AddWeaponButton(E_weapon_info weaponInfo)
    {
        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponSelectionPanel);
        ButtonManagerBasic button = buttonObj.GetComponent<ButtonManagerBasic>();
        button.buttonText = weaponInfo.f_display_name;

        button.clickEvent.AddListener(() =>
        {
            OnWeaponUpgradeSelected();
            LocalPlayerContext.Instance.player.CmdGetWeapon(weaponInfo.Name, 0);
        });

        button.UpdateUI();
    }

    private void OnWeaponUpgradeSelected()
    {
        ToggleMenuPanel();
        isWeaponUpgradeSelected = true;
        Utils.DestroyChildren(weaponSelectionPanel);
    }

    #endregion WeaponUpgrade

    private void OnDestroy()
    {
        Instance = null;
    }
}

