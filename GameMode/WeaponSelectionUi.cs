using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class WeaponSelectionUi : MonoBehaviour
{
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform mainWeaponSelectionPanelTransform;
    [SerializeField] private GameObject primaryWeaponSelectionPanel;
    [SerializeField] private GameObject secondaryWeaponSelectionPanel;

    [SerializeField] private ButtonManagerBasic primaryWeaponButton;
    [SerializeField] private ButtonManagerBasic secondaryWeaponButton;

    void Awake()
    {
        
    }

    void Start()
    {
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
                LocalPlayerContext.Instance.player.LocalCmdGetWeapon(weaponInfo.f_name, 1);
            });
        }
        else
        {
            button.clickEvent.AddListener(() =>
            {
                LocalPlayerContext.Instance.player.LocalCmdGetWeapon(weaponInfo.f_name, 0);
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
        bool newState = !mainWeaponSelectionPanelTransform.gameObject.activeSelf;
        mainWeaponSelectionPanelTransform.gameObject.SetActive(newState);
        LocalPlayerContext.Instance.TogglePlayerControl(!newState);
    }

    private void OnDestroy()
    {
        LocalPlayerContext.Instance.onTempDeathmatchWeaponMenuToggleEvent.RemoveListener(this.OnWeaponSelectionPanelToggle);
    }
}

