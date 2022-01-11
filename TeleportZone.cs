using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;

public class TeleportZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FpsCharacter fpsCharacter = other.gameObject.GetComponent<FpsCharacter>();
        if (!fpsCharacter || !fpsCharacter.isServer) return;

        PlayerManager.Instance.TeleportCharacterToSpawnPoint(fpsCharacter);
    }
}

