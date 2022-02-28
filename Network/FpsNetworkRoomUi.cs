using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Michsky.UI.ModernUIPack;

public class FpsNetworkRoomUi : MonoBehaviour
{
    public static FpsNetworkRoomUi Instance;
    [SerializeField] private Transform playerStatusPanel;
    [SerializeField] private GameObject playerStatusPrefab;

    [SerializeField] private UIManagerDropdown sceneDropdown;
    [SerializeField] private ButtonManagerBasic startButton;

    private bool isServer = false;

    private void Awake()
    {
        Instance = this;
        startButton.clickEvent.AddListener(ChangeScene);
    }

    public void Initialize(bool isServer)
    {
        this.isServer = isServer;
    }

    public void RefreshPlayerStatus(List<NetworkRoomPlayer> networkRoomPlayers)
    {
        int childs = playerStatusPanel.transform.childCount;
        for (int i = childs - 1; i > 0; i--)
        {
            GameObject.Destroy(playerStatusPanel.transform.GetChild(i).gameObject);
        }

        foreach (NetworkRoomPlayer networkRoomPlayer in networkRoomPlayers)
        {
            GameObject statusObj = Instantiate(playerStatusPrefab , playerStatusPanel);
        }
    }

    public void UpdatePlayerStatus()
    { 
        
    }

    private void ChangeScene()
    {
        if(isServer)
            FpsNetworkRoomManager.Instance.ServerChangeScene("fy_sample");
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}

