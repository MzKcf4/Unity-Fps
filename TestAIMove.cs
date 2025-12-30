using EasyCharacterMovement;
using Pathfinding;
using UnityEngine;

public class TestAIMove : MonoBehaviour
{
    private CharacterMovement mCharacterMovement;
    private Character character;
    [SerializeField]
    private GameObject childObject;
    private IAstarAI mAstarAI;

    void Start()
    {
        mCharacterMovement = GetComponent<CharacterMovement>();
        mAstarAI = GetComponentInChildren<IAstarAI>();
        character = GetComponent<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        // character.SetMovementDirection(mAstarAI.desiredVelocity.normalized);
        // mCharacterMovement.velocity = new Vector3(mAstarAI.desiredVelocity.x, 0, mAstarAI.desiredVelocity.z);
        // mCharacterMovement.Move();
        childObject.transform.localPosition = Vector3.zero;
    }
}
