using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Both fields will be supplied by the NetworkedClient when recording state.
public interface INetworkedClientInput
{
    /// <summary>
    /// The amount of time that has passed between ticks when this input state was recorded. 
    /// This is necessary because the client's application can be running at a framerate lower than the target framerate.
    /// </summary>
    float DeltaTime { get; }

    /// <summary>
    /// The tick in which this input was sent on the client
    /// This is used by the server while sending states to the clients. 
    /// The state will be stamped with the Tick number of the last processed input block. 
    /// The client will then use the stamp to determine which input ticks to predict.
    /// </summary>
    uint Tick { get; }
}

