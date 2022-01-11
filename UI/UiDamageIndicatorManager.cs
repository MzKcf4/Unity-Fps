using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDamageIndicatorManager : MonoBehaviour
{
    public static UiDamageIndicatorManager Instance;

    [SerializeField] private GameObject container;
    [SerializeField] private GameObject damageIndicatorPrefab;

    private Dictionary<Transform, UiDamageIndicator> dictIndicators = new Dictionary<Transform, UiDamageIndicator>();

    private void Awake()
    {
        Instance = this;
    }

    public void CreateIndicator(Transform player, Transform target)
    {
        if (dictIndicators.ContainsKey(target))
        {
            dictIndicators[target].Restart();
            return;
        }

        GameObject obj = Instantiate(damageIndicatorPrefab, container.transform);
        UiDamageIndicator damageIndicator = obj.GetComponent<UiDamageIndicator>();
        damageIndicator.Register(player , target);

        dictIndicators.Add(target, damageIndicator);
    }
}
