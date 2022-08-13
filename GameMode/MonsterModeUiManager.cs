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
    [SerializeField] private ButtonManagerBasic weaponMenuButton;
    [SerializeField] private ButtonManagerBasic traitMenuBotton;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private GameObject traitButtonPrefab;

    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI restText;
    [SerializeField] private TextMeshProUGUI ammoPackText;
    [SerializeField] private Transform hudPanel;
    [SerializeField] private Transform menuPanel;
    [SerializeField] private Transform selectionPanel;

    [SerializeField] private ButtonManagerBasic testButton;

    private bool isWeaponUpgradeSelected = false;
    private int currentStage = 0;

    private void Start()
    {
        Instance = this;
        LocalPlayerContext.Instance.onGamemodeMenuToggleEvent.AddListener(ToggleMenuPanel);

        weaponMenuButton.clickEvent.AddListener(() => { PopulateWeaponUpgradeMenu(); });
        traitMenuBotton.clickEvent.AddListener(() => { PopulateTraitSelectionMenu(); });

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
        currentStage = stage;
        isWeaponUpgradeSelected = false;
    }

    private void PopulateWeaponUpgradeMenu()
    {
        Utils.DestroyChildren(selectionPanel);
        if (isWeaponUpgradeSelected || currentStage <= 0)  return;

        List<E_weapon_info> weapons = E_weapon_info.FindEntities(e => e.f_active && e.f_monster_level == currentStage && e.f_category != WeaponCategory.Pistol);

        if (weapons == null || weapons.Count == 0)
            return;

        for (int i = 0; i < weapons.Count; i++)
        {
            AddWeaponButton(weapons[i]);
        }
    }

    private void AddWeaponButton(E_weapon_info weaponInfo)
    {
        GameObject buttonObj = Instantiate(weaponButtonPrefab, selectionPanel);
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
        Utils.DestroyChildren(selectionPanel);
    }

    #endregion WeaponUpgrade

    #region

    private void PopulateTraitSelectionMenu() 
    {
        Utils.DestroyChildren(selectionPanel);
        MonsterModeInfoDto localMMInfoDto = (MonsterModeInfoDto) LocalPlayerContext.Instance.player.AdditionalInfoObjects[Constants.ADDITIONAL_KEY_MM_CHAR_INFO];
        foreach (AbilityEnum abilityEnum in AbilityFactory.AbilitiesForPlayer) 
        {
            if (localMMInfoDto.Abilities.Contains(abilityEnum)) continue;

            GameObject buttonObj = Instantiate(traitButtonPrefab, selectionPanel);
            ButtonManagerBasicWithIcon button = buttonObj.GetComponent<ButtonManagerBasicWithIcon>();
            button.buttonText = abilityEnum.ToString();

            button.clickEvent.AddListener(() =>
            {
                LocalPlayerContext.Instance.player.GetComponent<MzAbilitySystem>().CmdAddAbility(abilityEnum);
                localMMInfoDto.Abilities.Add(abilityEnum);
                Destroy(buttonObj);
            });

            button.UpdateUI();
        }
    }

    #endregion

    private void OnDestroy()
    {
        Instance = null;
    }
}

