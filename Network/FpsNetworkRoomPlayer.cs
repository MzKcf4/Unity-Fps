using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class FpsNetworkRoomPlayer : NetworkRoomPlayer
{
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        //Debug.Log($"OnStartClient {gameObject}");
    }

    public override void OnClientEnterRoom()
    {
        
    }

    public override void OnClientExitRoom()
    {
        //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
    }

    public override void IndexChanged(int oldIndex, int newIndex)
    {
        //Debug.Log($"IndexChanged {newIndex}");
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        //Debug.Log($"ReadyStateChanged {newReadyState}");
    }

}

