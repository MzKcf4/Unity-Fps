using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct EcmNetworkState : IEquatable<EcmNetworkState>, INetworkedClientState
{
    public uint LastProcessedInputTick => lastProcessedInput;

    public Vector3 position;
    public Vector3 velocity;
    public float verticalVelocity;
    public uint lastProcessedInput;

    public EcmNetworkState(Vector3 position, Vector3 velocity, float verticalVelocity, uint lastProcessedInput)
    {
        this.position = position;
        this.velocity = velocity;
        this.verticalVelocity = verticalVelocity;
        this.lastProcessedInput = lastProcessedInput;
    }

    public bool Equals(EcmNetworkState other)
    {
        return position.Equals(other.position) && lastProcessedInput == other.lastProcessedInput;
    }

    public bool Equals(INetworkedClientState other)
    {
        return other is EcmNetworkState __other && Equals(__other);
    }
}