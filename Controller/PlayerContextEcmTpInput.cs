using Cinemachine;
using EasyCharacterMovement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
public class PlayerContextEcmTpInput : MonoBehaviour
{
    private CinemachineThirdPersonCharacter ecmCharacter;
    private FpsPlayer fpsPlayer;
    public Transform cmCameraTarget;

    public float cameraTurnSensitivity = 0.1f;
    public float cameraLookUpSensitivity = 0.1f;

    protected Vector2 _movementInput;
    protected bool _isMouseInput;

    // ---- Look ----
    private Vector2 _lookInput;
    private float _pitch;
    private float _yaw;
    // --------------

    protected Vector2 _zoomInput;

    private Vector3 rawInputMovement = Vector3.zero;
    public bool CanMove = true;
    public bool isJumpKeyPressed = false;

    // ---- Zoom ----
    [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;
    private Cinemachine3rdPersonFollow thirdPersonComp;
    public float lerpDuration = 0.3f;
    public Vector3 aimShoulderOffset = new Vector3(0.5f, -0.4f, 0);
    public float aimCameraDistance = 2f;

    public Vector3 freeShoulderOffset = Vector3.zero;
    public float freeCameraDistance = 5f;

    private bool isAiming;
    // ------------

    void Start()
    {
        ecmCharacter = GetComponentInParent<CinemachineThirdPersonCharacter>();
        thirdPersonComp = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        fpsPlayer = GetComponent<FpsPlayer>();

        Vector3 cameraTargetEulerAngles = cmCameraTarget.eulerAngles;
        _pitch = cameraTargetEulerAngles.x;
        _yaw = cameraTargetEulerAngles.y;

        LocalPlayerContext.Instance.mouseLookInputEvent.AddListener(OnMouseLook);
        LocalPlayerContext.Instance.weaponSecondaryActionInputEvent.AddListener(OnWeaponSecondaryActionInput);
        LocalPlayerContext.Instance.movementInputEvent.AddListener(OnMovement);
        LocalPlayerContext.Instance.jumpEvent.AddListener(OnJump);
    }

    private void OnWeaponSecondaryActionInput(KeyPressState value)
    {
        if (ecmCharacter == null) return;

        if (value == KeyPressState.Holding)
        {
            ecmCharacter.SetRotationMode(RotationMode.OrientToCameraViewDirection);
            isAiming = true;
            StopCoroutine("LerpCameraZoom");
            StartCoroutine("LerpCameraZoom");
            fpsPlayer.SetIsAiming(true);
        }
        else
        {
            ecmCharacter.SetRotationMode(RotationMode.OrientToMovement);
            isAiming = false;
            StopCoroutine("LerpCameraZoom");
            StartCoroutine("LerpCameraZoom");
            fpsPlayer.SetIsAiming(false);
        }
    }
    public virtual void OnMovement(InputAction.CallbackContext context)
    {
        // This returns Vector2.zero when context.canceled is true,
        // so no need to handle these separately

        _movementInput = context.ReadValue<Vector2>();
    }

    public virtual void OnJump(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            ecmCharacter.Jump();
        else if (context.canceled)
            ecmCharacter.StopJumping();
    }

    public virtual void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            ecmCharacter.Crouch();
        else if (context.canceled)
            ecmCharacter.StopCrouching();
    }

    public virtual void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            ecmCharacter.Sprint();
        else if (context.canceled)
            ecmCharacter.StopSprinting();
    }

    public virtual void OnMouseLook(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _isMouseInput = true;
            _lookInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _lookInput = Vector2.zero;
        }
        ecmCharacter.UpdateMouseLook(_lookInput);
    }

    public virtual void OnMouseScroll(InputAction.CallbackContext context)
    {
        // This returns Vector2.zero when context.canceled is true,
        // so no need to handle these separately

        _zoomInput = context.ReadValue<Vector2>();
    }

    public virtual void OnControllerLook(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _isMouseInput = false;
            _lookInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _lookInput = Vector2.zero;
        }
    }

    
    /// <summary>
    /// Cursor lock input action handler.
    /// </summary>

    public virtual void OnCursorLock(InputAction.CallbackContext context)
    {
        if (context.started)
            _cameraController.LockCursor();
    }

    /// <summary>
    /// Cursor unlock input action handler.
    /// </summary>

    public virtual void OnCursorUnlock(InputAction.CallbackContext context)
    {
        if (context.started)
            _cameraController.UnlockCursor();
    }
    

    /// <summary>
    /// Rotate the camera along its yaw.
    /// </summary>

    public void AddCameraYawInput(float value)
    {
        _yaw = MathLib.Clamp0360(_yaw + value);
    }

    /// <summary>
    /// Rotate the camera along its pitch.
    /// </summary>

    public void AddCameraPitchInput(float value)
    {
        _pitch = Mathf.Clamp(_pitch + value, -80.0f, 80.0f);
    }

    protected virtual void HandleCameraInput()
    {

    }

    protected virtual void HandleCharacterInput()
    {
        Vector3 movementDirection = Vector3.zero;

        movementDirection += Vector3.right * _movementInput.x;
        movementDirection += Vector3.forward * _movementInput.y;

        movementDirection = movementDirection.relativeTo(Camera.main.transform);

        ecmCharacter.SetMovementDirection(movementDirection);
    }

    private void HandlePlayerInput()
    {
        // HandleCameraInput();
        HandleCharacterInput();
    }

    private void Update()
    {
        HandlePlayerInput();
    }

    IEnumerator LerpCameraZoom()
    {
        float time = 0;
        while (time < lerpDuration)
        {
            if (isAiming)
            {
                thirdPersonComp.ShoulderOffset = Vector3.Lerp(thirdPersonComp.ShoulderOffset, aimShoulderOffset, time / lerpDuration);
                thirdPersonComp.CameraDistance = Mathf.Lerp(thirdPersonComp.CameraDistance, aimCameraDistance, time / lerpDuration);
            }
            else
            {
                thirdPersonComp.ShoulderOffset = Vector3.Lerp(thirdPersonComp.ShoulderOffset, freeShoulderOffset, time / lerpDuration);
                thirdPersonComp.CameraDistance = Mathf.Lerp(thirdPersonComp.CameraDistance, freeCameraDistance, time / lerpDuration);
            }
            time += Time.deltaTime;
            yield return null;
        }
    }
}
*/