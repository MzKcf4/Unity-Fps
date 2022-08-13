using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class MzAbilitySystem : NetworkBehaviour
{
    public UnityEvent<string> AbilityReadyEvent { get { return abilityReadyEvent; } }

    private FpsCharacter owner;
    private Dictionary<string, Ability> dictAbilities = new Dictionary<string, Ability>();
    private UnityEvent<string> abilityReadyEvent = new UnityEvent<string>();

    private void Start()
    {
        owner = GetComponent<FpsCharacter>();
    }

    private void Update()
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.Update(Time.deltaTime);
        }
    }

    public void OnOwnerPreDealDamage(DamageInfo damageInfo)
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.OnOwnerPreDealDamage(damageInfo);
        }
    }

    public void OnOwnerPreTakeDamage(DamageInfo damageInfo) 
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.OnOwnerPreTakeDamage(damageInfo);
        }
    }

    public void OnOwnerPostDealDamage(DamageInfo damageInfo)
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.OnOwnerPostDealDamage(damageInfo);
        }
    }

    public void OnOwnerPostTakeDamage(DamageInfo damageInfo) 
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.OnOwnerPostTakeDamage(damageInfo);
        }
    }
    public void AbilityReady(Ability ability)
    {
        // dictAbilities[abilityId]
    }

    [Client]
    public void ClientAddAbility(AbilityEnum abilityEnum)
    {
        Ability ability = AbilityFactory.GetAbility(abilityEnum, owner);
        if (dictAbilities.ContainsKey(ability.GetID())) {
            return;
        }
        AddAbility(abilityEnum);
        CmdAddAbility(abilityEnum);
    }

    [Command]
    public void CmdAddAbility(AbilityEnum abilityEnum)
    {
        AddAbility(abilityEnum);
    }

    public void AddAbility(AbilityEnum abilityEnum) 
    {
        Ability ability = AbilityFactory.GetAbility(abilityEnum, owner);
        AddAbility(ability);
    }

    public void AddAbility(Ability ability) 
    {
        dictAbilities.Add(ability.GetID(), ability);
    }

    public Ability GetAbility(string key)
    {
        if (dictAbilities.ContainsKey(key))
            return dictAbilities[key];

        return null;
    }

    // Temp method, as zombies only have 1 ability by now
    public Ability GetFirstAbility()
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            return entry.Value;
        }
        return null;
    }

    public void ActiviateAbility(string key)
    {
        if (dictAbilities.ContainsKey(key))
            dictAbilities[key].ActiviateAbility();
        else
            Debug.LogWarning("No ability " + key + " is found ");
    }
}

