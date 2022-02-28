using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class DebugModeManager : NetworkBehaviour
{
    public static DebugModeManager Instance;

    [SerializeField] private GameObject weaponSelectionUiPrefab;
    private WeaponSelectionUi weaponSelectionUi;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (weaponSelectionUi == null)
        {
            GameObject obj = Instantiate(weaponSelectionUiPrefab, FpsUiManager.Instance.GetInfoPanel());
            weaponSelectionUi = obj.GetComponent<WeaponSelectionUi>();
        }
    }


}

