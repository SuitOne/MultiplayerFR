using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MainR
{
    public class CommanderController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float lookSpeed = 2f;
        [SerializeField] private float maxLookAngle = 85f;
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float maxHeight = 100f;

        private float rotationX = 0;
        private float rotationY = 0;

        private CommanderManager commanderManager;

        void Start()
        {
            // Confine cursor
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            // Get manager
            if (TryGetComponent(out CommanderManager cM))
            {
                commanderManager = cM;
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
                commanderManager.SetPlayerState(
                    commanderManager.GetPlayerState == PlayerManager.PlayerState.Paused ? PlayerManager.PlayerState.Play : PlayerManager.PlayerState.Paused);
            }

            // Pause guard
            if (commanderManager.GetPlayerState != PlayerManager.PlayerState.Play) { return; }

            HandleMovement();

            CommandCheck();
        }

        private void HandleMovement()
        {
            // Camera rotation
            if (Input.GetMouseButton(2))
            {
                rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
                rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            }
            

            // Clamp the vertical rotation to avoid flipping
            rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);

            // Camera movement
            Vector3 move = new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            float boost = Input.GetKey(KeyCode.LeftShift) ? 4 : 1;
            transform.Translate(moveSpeed * boost * Time.deltaTime * move, Space.Self);

            // Clamp height restriction
            transform.position = new Vector3(
                transform.position.x,
                Mathf.Clamp(transform.position.y, minHeight, maxHeight),
                transform.position.z);
        }

        private void CommandCheck()
        {
            // Ping command
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    commanderManager.PlacementController.RpcPing(hit.point);
                }
            }

            // Place building command
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    commanderManager.SetPlayerState(PlayerManager.PlayerState.Placement);
                    commanderManager.PlacementController.PlaceGhost(hit.point);
                }
            }

            // Remove building command
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Building") && hit.collider.TryGetComponent(out Building building))
                    {
                        building.RpcDestroyBuilding();
                    }
                }
            }
        }
    }
}
