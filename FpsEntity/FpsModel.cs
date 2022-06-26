using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Animations.Rigging;

/*
Indicates the ragdoll object
*/

public class FpsModel : MonoBehaviour
{
    [HideInInspector] public FpsCharacter fpsCharacter;
    private SkinnedMeshRenderer bodyRenderer;

    public bool isLookAtWeaponAim = false;

    // Controls rotation + up / down aim
    private AimController finalIkAimController;
    private AimIK finalIkAimIk;

    private Transform weaponAimAt;

    void Awake()
    {
        fpsCharacter = GetComponentInParent<FpsCharacter>();
        finalIkAimController = GetComponent<AimController>();
        finalIkAimIk = GetComponent<AimIK>();
        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // For unknown reason , cannot simply assign the rig constraint during runtime
        //  because model will throw error when re-binding the rig.
        // rigBodyAimTransform = GetComponentInChildren<RigBodyAimMarker>().transform;
        // rigBuilder = GetComponent<RigBuilder>();
        // MultiAimConstraint[] aimConstraints = rigBuilder.layers[0].rig.GetComponentsInChildren<MultiAimConstraint>();
        // Debug.Log(aimConstraints.Length);
    }

    void Start()
    {
        // Don't let it rotate with model layer , because model layer rotate with it
        // StartCoroutine(DelayDetachBodyAim());
    }

    private void Update()
    {
        /*
        if (!fpsCharacter.IsLookAtWeaponAim || weaponAimAt == null) return;

        Vector3 targetPostition = new Vector3(weaponAimAt.position.x,
                                              this.transform.position.y,
                                              weaponAimAt.position.z);
        // this.transform.LookAt(targetPostition);
        // rigBodyAimTransform.position = weaponAimAt.transform.position;
        */
    }

    public void SetRendererLayer(string layerName)
    {
        if (!bodyRenderer) return;

        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void SetLookAtTransform(Transform transform)
    {
        weaponAimAt = transform;

        if (finalIkAimController)
        {
            finalIkAimController.target = weaponAimAt;
            ((IKSolverAim)finalIkAimIk.GetIKSolver()).target = weaponAimAt;
        }
        
    }

    public void ToggleLookAt(bool enable)
    {
        
        if (finalIkAimController)
        {
            finalIkAimController.enabled = enable;
            finalIkAimIk.enabled = enable;
        }
        
    }
}
