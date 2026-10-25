using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>()?.transform;

        Camera playerCamera = cameraTransform != null
            ? cameraTransform.GetComponent<Camera>()
            : null;

        if (playerCamera != null)
            playerCamera.enabled = IsOwner;

        AudioListener listener = cameraTransform != null
            ? cameraTransform.GetComponent<AudioListener>()
            : null;

        if (listener != null)
            listener.enabled = IsOwner;

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
{
    if (!IsSpawned || !IsOwner)
        return;

    // Stop movement when the player is dead.
    PlayerHealth health = GetComponent<PlayerHealth>();

    if (health != null && health.IsDead.Value)
        return;

    // Only allow movement while the match is active.
    MatchManager match = FindFirstObjectByType<MatchManager>();

    if (match == null || !match.IsSpawned || !match.IsMatchActive.Value)
        return;

    HandleMouseLook();
    HandleMovement();

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    if (Input.GetMouseButtonDown(0) &&
        Cursor.lockState != CursorLockMode.Locked)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
