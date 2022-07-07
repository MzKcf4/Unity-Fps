using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 The Messenger is a component that's really a workaround. 
 Because Mirror doesn't support generic arguments in a NetworkBehaviour, 
 the messaging part of the NetworkedClient needs to be encapsulated in a separate component.
 */
public interface INetworkedClientMessenger<TClientInput, TClientState>
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
{
    event System.Action<TClientInput> OnInputReceived;

    TClientState LatestServerState { get; }

    void SendState(TClientState state);

    void SendInput(TClientInput input);
}
