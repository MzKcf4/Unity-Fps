using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System;

public class DeathmatchWeaponSelectionUi : MonoBehaviour
{
    public static DeathmatchWeaponSelectionUi Instance;
    
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform mainWeaponSelectionPanelTransform;
    [SerializeField] private GameObject primaryWeaponSelectionPanel;
    [SerializeField] private GameObject secondaryWeaponSelectionPanel;

    [SerializeField] private ButtonManagerBasic primaryWeaponButton;
    [SerializeField] private ButtonManagerBasic secondaryWeaponButton;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        Console.Out.WriteLine("DeathmatchWeaponSelectionUi Start");
        LocalPlayerContext.Instance.onTempDeathmatchWeaponMenuToggleEvent.AddListener(this.OnWeaponSelectionPanelToggle);

        primaryWeaponButton.clickEvent.AddListener(() =>
        {
            OnPrimaryWeaponButtonClick();
        });
        secondaryWeaponButton.clickEvent.AddListener(() =>
        {
            OnSecondaryWeaponButtonClick();
        });

        List<E_weapon_info> sortedInfoList = E_weapon_info.FindEntities(e => true,
            null, (e1, e2) => e1.f_display_name.CompareTo(e2.f_display_name));

        sortedInfoList.ForEach(weaponInfo => {
            if (weaponInfo.f_active)
            {
                AddWeaponButton(weaponInfo);
            }
        });
    }
    
    private void AddWeaponButton(E_weapon_info weaponInfo)
    {
        GameObject buttonObj = null;
        if (WeaponCategory.Pistol == weaponInfo.f_category)
            buttonObj = Instantiate(weaponButtonPrefab, secondaryWeaponSelectionPanel.transform);
        else
            buttonObj = Instantiate(weaponButtonPrefab, primaryWeaponSelectionPanel.transform);


        ButtonManagerBasic button = buttonObj.GetComponent<ButtonManagerBasic>();
            
        string formattedText = weaponInfo.f_display_name + " ( " + weaponInfo.f_dm_kill_score + " )";
        button.buttonText = formattedText;

        if (WeaponCategory.Pistol == weaponInfo.f_category)
        {
            button.clickEvent.AddListener(() =>
            {
                LocalPlayerContext.Instance.StoreAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON_SECONDARY, weaponInfo.f_name);
            });
        }
        else
        {
            button.clickEvent.AddListener(() =>
            {
                LocalPlayerContext.Instance.StoreAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON, weaponInfo.f_name);
            });
        }
            
        button.UpdateUI();
    }

    private void OnSecondaryWeaponButtonClick()
    {
        primaryWeaponSelectionPanel.SetActive(false);
        secondaryWeaponSelectionPanel.SetActive(true);
    }

    private void OnPrimaryWeaponButtonClick()
    {
        primaryWeaponSelectionPanel.SetActive(true);
        secondaryWeaponSelectionPanel.SetActive(false);
    }

    private void OnWeaponSelectionPanelToggle()
    {
        Debug.Log(mainWeaponSelectionPanelTransform);
        bool newState = !mainWeaponSelectionPanelTransform.gameObject.activeSelf;
        mainWeaponSelectionPanelTransform.gameObject.SetActive(newState);
        LocalPlayerContext.Instance.TogglePlayerControl(!newState);
    }
    
}
