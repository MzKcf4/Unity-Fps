using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class FpsNetworkRoomManager : Mirror.NetworkRoomManager
{
    public GameModeEnum GameMode { get { return gameMode; } }
    public static FpsNetworkRoomManager Instance;
    private GameModeEnum gameMode = GameModeEnum.Debug;
    [SerializeField] GameObject coreObjectPrefab;

    private GameObject coreObject;
    bool isServer = false;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void Start()
    {
        base.Start();

        if (coreObject == null)
            coreObject = Instantiate(coreObjectPrefab);
    }

    public override void OnStartServer()
    {
        if (string.IsNullOrWhiteSpace(RoomScene))
        {
            Debug.LogError("NetworkRoomManager RoomScene is empty. Set the RoomScene in the inspector for the NetworkRoomManager");
            return;
        }

        isServer = true;
        OnRoomStartServer();
        InitCoreObject();
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == RoomScene)
        {
            
        }
        else
        { 
            
        }
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();

        if (IsSceneActive(RoomScene))
        {
            NetworkRoomUi.Instance.RefreshPlayerStatus(roomSlots);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InitCoreObject();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        RemoveCoreObject();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        RemoveCoreObject();
    }

    private void InitCoreObject()
    {
        if (coreObject == null)
            coreObject = Instantiate(coreObjectPrefab);
    }

    private void RemoveCoreObject()
    {
        if (coreObject != null)
            Destroy(coreObject);

        coreObject = null;
    }

    public void SetGamemode(GameModeEnum gameModeEnum)
    {
        gameMode = gameModeEnum;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if(SharedContext.Instance)
            SharedContext.Instance.ClearList();

        if(CoreGameManager.Instance)
            CoreGameManager.Instance.SpawnGameModeManager(gameMode);
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        if(SharedContext.Instance)
            SharedContext.Instance.ClearList();

        if (FpsUiManager.Instance != null)
        {
            Transform infoPanel = FpsUiManager.Instance.GetInfoPanel();
            for (int i = infoPanel.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(infoPanel.GetChild(i).gameObject);
            }
        }
    }

    public override void OnGUI()
    {
        if (NetworkServer.active && !IsSceneActive(RoomScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to Room"))
                ServerChangeScene(RoomScene);
            GUILayout.EndArea();
        }
    }
}

