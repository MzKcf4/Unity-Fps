using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct EcmNetworkInput : INetworkedClientInput
{
    public uint Tick => tick;
    public float DeltaTime => deltaTime;

    public uint tick;
    // public Vector2 input;
    public Vector3 velocity;
    public float deltaTime;
    

    public EcmNetworkInput(Vector3 velocity, uint tick, float deltaTime)
    {
        this.velocity = velocity;
        this.tick = tick;
        this.deltaTime = deltaTime;
    }
}