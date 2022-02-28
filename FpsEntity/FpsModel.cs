using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

/*
Indicates the ragdoll object
*/

public class FpsModel : MonoBehaviour
{
    [HideInInspector] public FpsEntity controllerEntity;
    private SkinnedMeshRenderer bodyRenderer;
    private AimController finalIkAimController;
    private AimIK finalIkAimIk;
    
    private Transform lookAtTransform;
    
    void Awake()
    {
        controllerEntity = GetComponentInParent<FpsEntity>();
        finalIkAimController = GetComponent<AimController>();
        finalIkAimIk = GetComponent<AimIK>();
        bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    void Start()
    {
        
    }

    public void SetRendererLayer(string layerName)
    {
        if (!bodyRenderer) return;

        gameObject.layer = LayerMask.NameToLayer(layerName);
    }
    
    public void SetLookAtTransform(Transform transform)
    {
        lookAtTransform = transform;

        if (finalIkAimController)
        {
            finalIkAimController.target = transform;
            ((IKSolverAim)finalIkAimIk.GetIKSolver()).target = transform;
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
