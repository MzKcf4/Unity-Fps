using System;
using Mirror;

public class EcmNetworkMessenger : NetworkBehaviour, INetworkedClientMessenger<EcmNetworkInput, EcmNetworkState>
{
    public event Action<EcmNetworkInput> OnInputReceived;

    public EcmNetworkState LatestServerState => _latestServerState;

    EcmNetworkState _latestServerState;

    // The SendState method should call an Rpc and send the provided state to the clients:
    public void SendState(EcmNetworkState state)
    {
        RpcSendState(state);
    }

    // The SendInput method should call a Cmd and send the provided input to the server:
    public void SendInput(EcmNetworkInput input)
    {
        CmdSendInput(input);
    }

    [ClientRpc(channel = Channels.Unreliable)]
    void RpcSendState(EcmNetworkState state)
    {
        _latestServerState = state;
    }

    // Lastly, the Messenger must implement an Action of the input type,
    // and invoke it whenever an input message is received:
    [Command(channel = Channels.Unreliable)]
    void CmdSendInput(EcmNetworkInput input)
    {
        OnInputReceived?.Invoke(input);
    }
}