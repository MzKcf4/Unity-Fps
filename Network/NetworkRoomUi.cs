using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Michsky.UI.Shift;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class NetworkRoomUi : MonoBehaviour
{
    public static NetworkRoomUi Instance;
    [SerializeField] private Transform playerStatusPanel;
    [SerializeField] private GameObject playerStatusPrefab;
    
    [SerializeField] private TMP_Dropdown sceneDropdown;
    [SerializeField] private TextMeshProUGUI sceneText;

    [SerializeField] private TMP_Dropdown gameModeDropdown;
    [SerializeField] private TextMeshProUGUI gamemodeText;

    [SerializeField] private Button startButton;

    public UnityEvent<string> OnSceneUpdateEvent = new UnityEvent<string>();
    public UnityEvent<string> OnGamemodeUpdateEvent = new UnityEvent<string>();

    public bool isInitialized = false;
    private bool isServer = false;
    private int selectedSceneIdx = 0;

    private void Awake()
    {
        Instance = this;
        startButton.onClick.AddListener(DoChangeScene);
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
        selectedSceneIdx = 0;
    }

    private void InitializeSceneList()
    {
        List<string> sceneList = GetScenes();
        sceneList.Sort();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (string scene in sceneList)
        {
            options.Add(new TMP_Dropdown.OptionData(scene));
        }
        sceneDropdown.AddOptions(options);
        sceneDropdown.onValueChanged.AddListener(OnSceneDropdownChanged);
    }

    private void InitializeGamemodeList()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (int i in Enum.GetValues(typeof(GameModeEnum)))
        {
            options.Add(new TMP_Dropdown.OptionData(Enum.GetName(typeof(GameModeEnum), i)));
        }
        gameModeDropdown.AddOptions(options);
        gameModeDropdown.onValueChanged.AddListener(OnGamemodeDropdownChanged);
    }

    public void UpdateSceneText(string text)
    {
        sceneText.text = text;
    }

    private void OnSceneDropdownChanged(int idx)
    {
        if (isServer)
        {
            OnSceneUpdateEvent.Invoke(sceneDropdown.options[idx].text);
            this.selectedSceneIdx = idx;
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
            OnGamemodeUpdateEvent.Invoke(gameModeDropdown.options[idx].text);
        }
    }


    public void RefreshPlayerStatus(HashSet<NetworkRoomPlayer> networkRoomPlayers)
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
        if (!isServer || selectedSceneIdx <= 0) return;
        
        string sceneName = sceneDropdown.options[selectedSceneIdx].text;
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

