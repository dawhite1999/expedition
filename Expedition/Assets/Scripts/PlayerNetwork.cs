using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    //Movement variables
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float mouseSensitivity = 100;
    [SerializeField] float upDownRange;
    [SerializeField] Camera povCam;
    CharacterController characterController;
    float verticalLookRotation = 0f;
    bool pauseMenuOpen = false;
    Vector2 moveInput;
    Vector2 lookInput;
    Vector3 velocity;
    NetworkVariable<PlayerData> rnum = new NetworkVariable<PlayerData>( new PlayerData
    {
        _int = 32,
        _bool = true
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private bool HasControl()//determines if the player has control of this player character
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            return true; // offline mode
        return IsOwner;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        ToggleLockCursor(true);
    }

    public struct PlayerData : INetworkSerializable 
    {
        public int _int;
        public FixedString128Bytes _string;
        public bool _bool;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _string);
            serializer.SerializeValue(ref _bool);
        }
    }
    public override void OnNetworkSpawn()
    {
        rnum.OnValueChanged += (PlayerData previousValue, PlayerData newValue) =>
        {
            Debug.Log(OwnerClientId + " Random Number Changed: " + previousValue + " to " + newValue._int + " " + newValue._bool + newValue._string);
        };
    }
    private void Update()
    {
        if (HasControl() == false)
            return;

        Movement();
        Looking();
    }

    void ToggleLockCursor(bool isLock)
    {
        if(isLock) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pauseMenuOpen = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseMenuOpen = true;
        }
    }

    void Looking()
    {
        //horizontal
        float mouseXrot = lookInput.x * mouseSensitivity;
        transform.Rotate(0, mouseXrot, 0);
        //vertical
        verticalLookRotation -= lookInput.y * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -upDownRange, upDownRange);
        povCam.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
    }
    void Movement()
    {
        //movement
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        //jumping and gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep the player grounded
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    void Action1()
    {
        rnum.Value = new PlayerData
        {
            _int = Random.Range(0, 100),
            _string = "I want to squeeze some titties so bad ",
            _bool = false
        };
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////Input Actions///////////////////////////////////////////////////////////////////////////////////////////////
    public void OnMove(InputAction.CallbackContext context)
    {
        if (HasControl() == false)
            return;
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (HasControl() == false)
            return;
        if (context.performed && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
    public void OnMouse1(InputAction.CallbackContext context)
    {
        if (HasControl() == false)
            return;
        if (context.performed)
            Action1();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (HasControl() == false)
            return;
        if(pauseMenuOpen == true)
            return;
        lookInput = context.ReadValue<Vector2>();
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        if (HasControl() == false)
            return;
        if (context.performed)
            ToggleLockCursor(false);
    }
}
