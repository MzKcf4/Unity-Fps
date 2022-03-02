using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Michsky.UI.ModernUIPack;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

public class NetworkRoomUi : MonoBehaviour
{
    public static NetworkRoomUi Instance;
    [SerializeField] private Transform playerStatusPanel;
    [SerializeField] private GameObject playerStatusPrefab;
    
    [SerializeField] private CustomDropdown sceneDropdown;
    [SerializeField] private TextMeshProUGUI sceneText;

    [SerializeField] private CustomDropdown gameModeDropdown;
    [SerializeField] private TextMeshProUGUI gamemodeText;

    [SerializeField] private ButtonManagerBasic startButton;

    public UnityEvent<string> OnSceneUpdateEvent = new UnityEvent<string>();
    public UnityEvent<string> OnGamemodeUpdateEvent = new UnityEvent<string>();

    public bool isInitialized = false;
    private bool isServer = false;

    private void Awake()
    {
        Instance = this;
        startButton.clickEvent.AddListener(DoChangeScene);
    }

    public void Initialize(bool isServer)
    {
        if (isInitialized) return;

        isInitialized = true;
        this.isServer = isServer;

        if (isServer)
        {
            InitializeSceneList();
            InitializeGamemodeList();
        }

        startButton.gameObject.SetActive(isServer);
        sceneDropdown.gameObject.SetActive(isServer);
        gameModeDropdown.gameObject.SetActive(isServer);

        sceneText.gameObject.SetActive(!isServer);
        gamemodeText.gameObject.SetActive(!isServer);
    }

    private void InitializeSceneList()
    {
        List<string> sceneList = GetScenes();
        foreach (string scene in sceneList)
        {
            sceneDropdown.CreateNewItemFast(scene, null);
        }
        sceneDropdown.SetupDropdown();
        sceneDropdown.dropdownEvent.AddListener(OnSceneDropdownChanged);
    }

    private void InitializeGamemodeList()
    {
        foreach (int i in Enum.GetValues(typeof(GameModeEnum)))
        {
            gameModeDropdown.CreateNewItemFast(Enum.GetName(typeof(GameModeEnum), i) , null);
        }
        gameModeDropdown.SetupDropdown();
        gameModeDropdown.dropdownEvent.AddListener(OnGamemodeDropdownChanged);
    }

    public void UpdateSceneText(string text)
    {
        sceneText.text = text;
    }

    private void OnSceneDropdownChanged(int idx)
    {
        if (isServer)
        {
            OnSceneUpdateEvent.Invoke(sceneDropdown.dropdownItems[sceneDropdown.selectedItemIndex].itemName);
        }
    }

    public void UpdateGamemodeText(string text)
    {
        gamemodeText.text = text;
    }

    private void OnGamemodeDropdownChanged(int idx)
    {
        if (isServer)
        {
            OnGamemodeUpdateEvent.Invoke(gameModeDropdown.dropdownItems[gameModeDropdown.selectedItemIndex].itemName);
        }
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

    private void DoChangeScene()
    {
        if (!isServer) return;
        string sceneName = sceneDropdown.dropdownItems[sceneDropdown.selectedItemIndex].itemName;
        FpsNetworkRoomManager.Instance.ServerChangeScene(sceneName);
    }

    private List<string> GetScenes()
    {
        var sceneNumber = SceneManager.sceneCountInBuildSettings;
        List<string> sceneList = new List<string>();
        for (int i = 0; i < sceneNumber; i++)
        {
            sceneList.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }
        return sceneList;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}

