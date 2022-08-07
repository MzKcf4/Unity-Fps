using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;

public class MzAbilitySystem : MonoBehaviour
{
    private FpsCharacter owner;
    public Dictionary<string, Ability> dictAbilities = new Dictionary<string, Ability>();

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

    public void OnOwnerPreTakeDamage(DamageInfo damageInfo) 
    {
        foreach (KeyValuePair<string, Ability> entry in dictAbilities)
        {
            Ability ability = entry.Value;
            ability.OnOwnerPreTakeDamage(damageInfo);
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

