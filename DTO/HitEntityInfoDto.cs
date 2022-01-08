using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HitEntityInfoDto
{
    public NetworkIdentity attackerIdentity;
    public NetworkIdentity victimIdentity;
    public DamageInfo damageInfo;
}
