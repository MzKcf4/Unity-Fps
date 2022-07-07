using System;

/*
 The state block. 
 This block should represent a snapshot of your client at any given time (position, rotation, speed, for example).

 The only requirement is to create a uint field to store the last processed input tick. 
 This is important because the prediction will set the client at that state, 
 and then reprocess all inputs from that tick till the most recent tick.

 It's recommended to create your state as a struct to avoid heap allocations.
 */
public interface INetworkedClientState : IEquatable<INetworkedClientState>
{
    /// <summary>
    /// The tick number of the last input packet processed by the server
    /// </summary>
    uint LastProcessedInputTick { get; }
}
