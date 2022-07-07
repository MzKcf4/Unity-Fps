using Mirror;
using UnityEngine;
using EasyCharacterMovement;

public class EcmNetworkClient : NetworkedClient<EcmNetworkInput, EcmNetworkState>
{
    private CharacterMovement ecmCharacter;
    private NetworkIdentity identity;
    // [Header("CharacterController/References")]
    // [SerializeField] CharacterController _characterController = null;
    // [Header("CharacterController/Settings")]
    // [SerializeField] float _speed = 10f;
    float _verticalVelocity = 0f;

    protected override void Awake()
    {
        base.Awake();
        ecmCharacter = GetComponent<CharacterMovement>();
        identity = GetComponent<NetworkIdentity>();
    }

    public override void SetState(EcmNetworkState state)
    {
        // No need to sync local player with server
        if (identity.isLocalPlayer)
            return;
        ecmCharacter.enabled = false;
        ecmCharacter.transform.position = state.position;
        _verticalVelocity = state.verticalVelocity;
        ecmCharacter.enabled = true;
    }

    public override void ProcessInput(EcmNetworkInput input)
    {
        /*
        var __movement = new Vector3(input.input.x, 0f, input.input.y);
        __movement = Vector3.ClampMagnitude(__movement, 1f) * _speed;
        if (!_characterController.isGrounded)
            _verticalVelocity += Physics.gravity.y * input.deltaTime;
        else
            _verticalVelocity = Physics.gravity.y;
        __movement.y = _verticalVelocity;
        _characterController.Move(__movement * input.deltaTime);
        */

        ecmCharacter.velocity = input.velocity;
    }

    protected override EcmNetworkState RecordState(uint lastProcessedInputTick)
    {
        return new EcmNetworkState(ecmCharacter.transform.position,ecmCharacter.velocity,
                                    _verticalVelocity, lastProcessedInputTick);
    }
}