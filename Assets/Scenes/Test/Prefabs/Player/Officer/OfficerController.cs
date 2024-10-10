using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    [RequireComponent(typeof(Rigidbody))]
    public class OfficerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float lookSpeed = 2f;
        public GameObject head;

        private Rigidbody rb;
        private float horizontalInput, verticalInput;
        private float mouseX, mouseY;
        private OfficerManager playerManager;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            // Lock the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;

            if (TryGetComponent(out OfficerManager pM))
            {
                playerManager = pM;
            }
            else
            {
                Debug.LogError("Could not get player manager!");
            }
        }

        void Update()
        {
            // Get pause command
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playerManager.SetPlayerState(
                    playerManager.GetPlayerState == PlayerManager.PlayerState.Paused ? PlayerManager.PlayerState.Play : PlayerManager.PlayerState.Paused);
            }

            // Pause guard
            if (playerManager.GetPlayerState != PlayerManager.PlayerState.Play) { return; }

            // Get input for movement and looking around
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            // Rotate the character around the Y axis
            transform.Rotate(0, mouseX * lookSpeed, 0);

            // Rotate the head around the local X axis
            float currentXRotation = head.transform.localEulerAngles.x;
            currentXRotation = currentXRotation > 180 ? currentXRotation - 360 : currentXRotation; // Adjusting the angle range from 0-360 to -180 to 180
            float newXRotation = Mathf.Clamp(currentXRotation - mouseY * lookSpeed, -80, 80);
            head.transform.localEulerAngles = new Vector3(newXRotation, 0, 0);
        }

        void FixedUpdate()
        {
            if (playerManager.GetPlayerState != PlayerManager.PlayerState.Play) { return; }

            // Move the character based on input
            Vector3 movement = transform.forward * verticalInput + transform.right * horizontalInput;
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement);
        }
    }

}
