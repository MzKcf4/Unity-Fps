using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using Mirror;

public class WeaponSelectionUi : MonoBehaviour
{
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform mainWeaponSelectionPanelTransform;

    [SerializeField] private GameObject pistolSelectionPanel;
    [SerializeField] private GameObject shotgunSelectionPanel;
    [SerializeField] private GameObject smgSelectionPanel;
    [SerializeField] private GameObject rifleSelectionPanel;
    [SerializeField] private GameObject sniperSelectionPanel;
    [SerializeField] private GameObject mgSelectionPanel;
    private List<GameObject> selectionPanels;

    [SerializeField] private ButtonManagerBasic pistolButton;
    [SerializeField] private ButtonManagerBasic shotgunButton;
    [SerializeField] private ButtonManagerBasic smgButton;
    [SerializeField] private ButtonManagerBasic rifleButton;
    [SerializeField] private ButtonManagerBasic sniperButton;
    [SerializeField] private ButtonManagerBasic mgButton;



    void Awake()
    {
        
    }

    void Start()
    {
        LocalPlayerContext.Instance.onTempDeathmatchWeaponMenuToggleEvent.AddListener(this.OnWeaponSelectionPanelToggle);
        selectionPanels = new List<GameObject>
        {
            pistolSelectionPanel,
            shotgunSelectionPanel,
            smgSelectionPanel,
            rifleSelectionPanel,
            sniperSelectionPanel,
            mgSelectionPanel
        };

        pistolButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(pistolSelectionPanel);
        });

        shotgunButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(shotgunSelectionPanel);
        });
        smgButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(smgSelectionPanel);
        });
        rifleButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(rifleSelectionPanel);
        });
        sniperButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(sniperSelectionPanel);
        });
        mgButton.clickEvent.AddListener(() =>
        {
            ShowOnlySelectionPanel(mgSelectionPanel);
        });

        List<E_weapon_info> sortedInfoList = E_weapon_info.FindEntities(e => true,
            null, (e1, e2) => e1.f_display_name.CompareTo(e2.f_display_name));

        sortedInfoList.ForEach(weaponInfo => {
            if (weaponInfo.f_category == WeaponCategory.Melee)
                return;

            if (weaponInfo.f_active)
            {
                AddWeaponButton(weaponInfo);
            }
        });
    }

    private void AddWeaponButton(E_weapon_info weaponInfo)
    {
        GameObject buttonObj = null;
        switch (weaponInfo.f_category) {
            case WeaponCategory.Pistol:
                buttonObj = Instantiate(weaponButtonPrefab, pistolSelectionPanel.transform);
                break;
            case WeaponCategory.Shotgun:
                buttonObj = Instantiate(weaponButtonPrefab, shotgunSelectionPanel.transform);
                break;
            case WeaponCategory.Smg:
                buttonObj = Instantiate(weaponButtonPrefab, smgSelectionPanel.transform);
                break;
            case WeaponCategory.Rifle:
                buttonObj = Instantiate(weaponButtonPrefab, rifleSelectionPanel.transform);
                break;
            case WeaponCategory.Sniper:
                buttonObj = Instantiate(weaponButtonPrefab, sniperSelectionPanel.transform);
                break;
            case WeaponCategory.Mg:
                buttonObj = Instantiate(weaponButtonPrefab, mgSelectionPanel.transform);
                break;
            default:
                Debug.LogError("Unknown weapon category: " + weaponInfo.f_category);
                return;
        }

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

    private void ShowOnlySelectionPanel(GameObject targetPanel) {
        foreach (var panel in selectionPanels)
        {
            if(panel == targetPanel)
                panel.SetActive(true);
            else
                panel.SetActive(false);
        }
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

