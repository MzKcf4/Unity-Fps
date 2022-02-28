using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PathologicalGames;
using MoreMountains.Feedbacks;

public class LocalSpawnManager : MonoBehaviour
{
    public static LocalSpawnManager Instance;

    [SerializeField] private GameObject bloodFxPrefab;
    [SerializeField] private GameObject bulletEffectPrefab;
    [SerializeField] private GameObject bulletDecalPrefab;
    [SerializeField] private GameObject damageTextPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnBloodFx(Vector3 position)
    {
        if (position == null) return;

        Transform objTransform = PoolManager.Pools["FX"].Spawn(bloodFxPrefab, position, Quaternion.identity);
        PoolManager.Pools["FX"].Despawn(objTransform, 2f);
    }

    public void SpawnBulletDecalFx(Vector3 position, Vector3 normalVector)
    {
        // ------- Feedback --------- //
        Transform effectTransform = PoolManager.Pools["FX"].Spawn(bulletEffectPrefab, position, Quaternion.identity);
        effectTransform.rotation = Quaternion.FromToRotation(effectTransform.up, normalVector) * effectTransform.rotation;

        MMFeedbacks feedbacks = effectTransform.gameObject.GetComponent<MMFeedbacks>();
        if (feedbacks)
            feedbacks.PlayFeedbacks();

        // ------- Decal ------- // 
        Transform decalTransform = PoolManager.Pools["FX"].Spawn(bulletDecalPrefab, position, Quaternion.identity);
        decalTransform.rotation = Quaternion.FromToRotation(decalTransform.forward, normalVector) * decalTransform.rotation;


        PoolManager.Pools["FX"].Despawn(decalTransform, 5f);
        PoolManager.Pools["FX"].Despawn(effectTransform, 5f);
    }

    public void SpawnDamageText(int damage, Vector3 position, bool isHeadshot)
    {
        Transform objTransform = PoolManager.Pools["FX"].Spawn(damageTextPrefab, position, Quaternion.identity);
        objTransform.SetParent(LocalPlayerContext.Instance.inGameDynamicCanvas.transform);

        DamageText damageText = objTransform.GetComponent<DamageText>();
        damageText.Initialize(damage, position, isHeadshot);

        PoolManager.Pools["FX"].Despawn(objTransform, 2f);
    }
}
