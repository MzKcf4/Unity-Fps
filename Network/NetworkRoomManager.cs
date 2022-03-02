using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class NetworkRoomManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSelectedMapChanged))]
    public string selectedMap;
    [SyncVar(hook = nameof(OnSelectedGamemodeChanged))]
    public string selectedGamemode;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkRoomUi.Instance.OnSceneUpdateEvent.AddListener(str => selectedMap = str);
        NetworkRoomUi.Instance.OnGamemodeUpdateEvent.AddListener(str => {
            selectedGamemode = str;
            if (Enum.TryParse(str, true, out GameModeEnum gameModeEnum)) {
                FpsNetworkRoomManager.Instance.SetGamemode(gameModeEnum);
            };
        });
        NetworkRoomUi.Instance.Initialize(isServer);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkRoomUi.Instance.Initialize(isServer);
    }

    private void OnSelectedMapChanged(string oldMap, string newMap)
    {
        if(!NetworkRoomUi.Instance.isInitialized)
            NetworkRoomUi.Instance.Initialize(isServer);

        NetworkRoomUi.Instance.UpdateSceneText(newMap);
    }

    private void OnSelectedGamemodeChanged(string oldVal, string newVal)
    {
        if (!NetworkRoomUi.Instance.isInitialized)
            NetworkRoomUi.Instance.Initialize(isServer);

        NetworkRoomUi.Instance.UpdateGamemodeText(newVal);
    }
}

