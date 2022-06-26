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
    [SerializeField] private Transform hudPanel;
    [SerializeField] private Transform menuPanel;
    [SerializeField] private Transform weaponSelectionPanel;

    private bool isWeaponUpgradeSelected = false;

    private void Start()
    {
        Instance = this;
        LocalPlayerContext.Instance.onGamemodeMenuToggleEvent.AddListener(ToggleMenuPanel);
    }

    public void UpdateKillText(int newKill)
    {
        killText.text = newKill.ToString();
    }

    public void ToggleMenuPanel()
    {
        bool hudActive = hudPanel.gameObject.activeSelf;
        hudPanel.gameObject.SetActive(!hudActive);
        menuPanel.gameObject.SetActive(hudActive);
    }

    public void UpdateRestCountdown(int countdown)
    {
        restText.text = countdown.ToString();
    }

    public void UpdateStage(int stage)
    {
        SetWeaponUpgradeAvailable(stage);
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
        List<E_weapon_info> weaponInfoList = E_weapon_info.FindEntities(e => e.f_active && e.f_horde_level == stage);
        RandomUtils.Shuffle(weaponInfoList);

        for (int i = 0; i < 4 && i < weaponInfoList.Count; i++)
        {
            AddWeaponButton(weaponInfoList[i]);
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

