using UnityEngine;
using Mirror;

namespace BingoGame.Network
{
    // Handles mouse-look camera rotation for local player
    public class CameraMovement : NetworkBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float smoothTime = 0.1f;

        [Header("Rotation Limits")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        private float rotationX = 0f;
        private float rotationY = 0f;
        private float currentRotationX = 0f;
        private float currentRotationY = 0f;
        private float velocityX = 0f;
        private float velocityY = 0f;

        private bool isCursorLocked = true;

        // Network sync
        [System.NonSerialized]
        private float lastSyncTime = 0f;
        private const float syncInterval = 0.05f;

        private void Start()
        {
            // Only enable camera for local player
            if (!isLocalPlayer)
            {
                if (playerCamera != null)
                {
                    playerCamera.enabled = false;
                    AudioListener listener = playerCamera.GetComponent<AudioListener>();
                    if (listener != null)
                    {
                        listener.enabled = false;
                    }
                }
                enabled = false;
                return;
            }

            // Lock cursor for local player
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Initialize rotation from current transform
            Vector3 currentRotation = transform.eulerAngles;
            rotationY = currentRotation.y;
            rotationX = currentRotation.x;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            // Toggle cursor lock with Space key
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isCursorLocked = !isCursorLocked;
                Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isCursorLocked;
            }

            if (isCursorLocked)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                rotationY += mouseX;
                rotationX -= mouseY;

                rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

                currentRotationX = Mathf.SmoothDamp(currentRotationX, rotationX, ref velocityX, smoothTime);
                currentRotationY = Mathf.SmoothDamp(currentRotationY, rotationY, ref velocityY, smoothTime);

                // Apply rotation to camera transform
                transform.rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);

                if (Time.time - lastSyncTime >= syncInterval)
                {
                    CmdSyncRotation(currentRotationX, currentRotationY);
                    lastSyncTime = Time.time;
                }
            }
        }

        [Command]
        private void CmdSyncRotation(float rotX, float rotY)
        {
            RpcSyncRotation(rotX, rotY);
        }

        [ClientRpc]
        private void RpcSyncRotation(float rotX, float rotY)
        {
            if (!isLocalPlayer)
            {
                transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
