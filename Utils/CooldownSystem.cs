using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    private readonly Dictionary<string, CooldownData> dictCooldown = new Dictionary<string, CooldownData>();
    private readonly List<string> removeCooldownList = new List<string>();

    // Update is called once per frame
    void Update()
    {
        ProcessCooldown();
    }

    private void ProcessCooldown()
    {
        float deltaTime = Time.deltaTime;

        foreach (var item in dictCooldown)
        {
            CooldownData cooldownData = item.Value;
            if (cooldownData.DecreaseCooldown(deltaTime))
                removeCooldownList.Add(item.Key);
        }

        if (removeCooldownList.Count > 0)
        {
            removeCooldownList.ForEach(key => dictCooldown.Remove(key));
            removeCooldownList.Clear();
        }
    }

    public float GetOrPutCooldown(string id, float time)
    {
        if (!dictCooldown.ContainsKey(id))
        {
            dictCooldown.Add(id, new CooldownData(id, time));
            return time;
        }

        return dictCooldown[id].remainingTime;
    }

    public float GetCooldown(string id)
    {
        if (!dictCooldown.ContainsKey(id))
            return 0f;

        return dictCooldown[id].remainingTime;
    }

    public bool IsOnCooldown(string id)
    {
        return dictCooldown.ContainsKey(id);
    }

    public void PutOnCooldown(string id, float time)
    {
        dictCooldown.Add(id, new CooldownData(id, time));
    }

    [System.Serializable]
    class CooldownData
    {
        public CooldownData(string id, float time)
        {
            this.id = id;
            this.remainingTime = time;
        }

        public string id;

        public float remainingTime;

        public bool DecreaseCooldown(float deltaTime)
        {
            remainingTime = Mathf.Max(remainingTime - deltaTime, 0);
            return remainingTime <= 0;
        }
    }
}
