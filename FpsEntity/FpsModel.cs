using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

/*
Indicates the ragdoll object
*/

public class FpsModel : MonoBehaviour
{
    public FpsEntity controllerEntity;
    public SkinnedMeshRenderer bodyRenderer;
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
    
    public void SetLookAtTransform(Transform transform)
    {
        lookAtTransform = transform;
        
        finalIkAimController.target = transform;
        ((IKSolverAim)finalIkAimIk.GetIKSolver()).target = transform;
    }
    
    public void ToggleLookAt(bool enable)
    {
        finalIkAimController.enabled = enable;
        finalIkAimIk.enabled = enable;
    }
}
