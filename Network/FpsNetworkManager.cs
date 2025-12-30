using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using Enviro;

public class FpsNetworkManager : NetworkManager
{
    // public static FpsNetworkManager Instance;
    private GameModeEnum gameMode = GameModeEnum.Debug;
    [SerializeField] GameObject coreObjectPrefab;
    private GameObject coreObject;


    public override void Awake()
    {
        base.Awake();
        // Instance = this;
    }

    public override void Start()
    {
        base.Start();

        if(coreObject == null)
            coreObject = Instantiate(coreObjectPrefab);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitCoreObject();
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


        SharedContext.Instance.ClearList();
        CoreGameManager.Instance.SpawnGameModeManager(gameMode);

        var randomWeather = Utils.GetRandomElement(EnviroManager.instance.Weather.Settings.weatherTypes);
        Debug.Log("Setting weather to: " + randomWeather.name);
        EnviroManager.instance.Weather.ChangeWeather(randomWeather);
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
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
}
