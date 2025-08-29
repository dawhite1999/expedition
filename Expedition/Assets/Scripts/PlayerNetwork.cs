using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    //Movement variables
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float gravity = -9.8f;
    CharacterController characterController;
    Vector3 moveInput;
    Vector3 velocity;
    NetworkVariable<PlayerData> rnum = new NetworkVariable<PlayerData>( new PlayerData
    {
        randomNumber = 32,
        name = "Player",
        isActive = true
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public struct PlayerData
    {
        public int randomNumber;
        public string name;
        public bool isActive;
    }
    public override void OnNetworkSpawn()
    {
        rnum.OnValueChanged += (PlayerData previousValue, PlayerData newValue) =>
        {
            Debug.Log(OwnerClientId + " Random Number Changed: " + previousValue + " to " + newValue.randomNumber + " " + newValue.isActive);
        };
    }
    private void Update()
    {
        if (!IsOwner)
            return;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep the player grounded
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnMouse1(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;
        if (context.performed)
        {
            rnum.Value = new PlayerData
            {
                randomNumber = Random.Range(0, 100),
                name = "Player" + OwnerClientId,
                isActive = true
            };
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }


}
