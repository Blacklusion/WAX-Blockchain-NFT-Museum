using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2.5f;
    private bool canMove = true;
    private bool isMouseLocked = false;
    private float cameraRotationY = 0f;

    private CharacterController characterController;
    private Vector3 velocity; // Current velocity

    public GameObject crosshair;

    [Inject(Id = "nftDetailParent")] private readonly CanvasGroup nftDetailParent;
    [Inject(Id = "interactionLbl")] private readonly TextMeshProUGUI interactionLbl;

    private Camera mainCamera;
    private Vector3 defaultCameraPosition;
    private float bobbingTimer = 0f;
    private float initialPlayerY;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        defaultCameraPosition = mainCamera.transform.localPosition;
        initialPlayerY = transform.position.y;
    }

    public void LockMouse()
    {
        isMouseLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockMouse()
    {
        isMouseLocked = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private bool DisplayMaterialOnHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 3.0f))
        {
            CanvasObject canvasObject = hit.collider.GetComponent<CanvasObject>();
            if (canvasObject != null && canvasObject.HasImage())
            {
                canvasObject.InitDetail();
                return true;
            }
        }
        return false;
    }

    private void ToggleInteractionText()
    {
        Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position);

        interactionLbl.text = string.Empty;
        if (Physics.Raycast(ray, out RaycastHit hit, 3.0f))
        {
            if (!hit.collider.gameObject.CompareTag("Canvas"))
            {
                return;
            }

            CanvasObject canvasObject = hit.collider.GetComponent<CanvasObject>();
            if (!canvasObject.HasImage()) { return; }
            interactionLbl.text = "Press [e] to toggle interaction";
        }
    }

    private void Update()
    {
        // If the mouse is locked, get the horizontal and vertical axis inputs.
        if (isMouseLocked)
        {
            if (canMove)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                if (horizontalInput != 0 || verticalInput != 0)
                {
                    Vector3 forwardMovement = transform.forward * verticalInput;
                    Vector3 rightMovement = transform.right * horizontalInput;
                    velocity = (forwardMovement + rightMovement).normalized * speed;

                    // Walking effect
                    bobbingTimer += Time.deltaTime * (velocity.magnitude * 2.6f);
                    float bobbingOffsetY = Mathf.Sin(bobbingTimer) * 0.035f;
                    float bobbingOffsetX = Mathf.Cos(bobbingTimer / 2) * 0.025f;
                    mainCamera.transform.localPosition = new Vector3(defaultCameraPosition.x + bobbingOffsetX, defaultCameraPosition.y + bobbingOffsetY, defaultCameraPosition.z);
                }
                else
                {
                    // Immediately stop the player
                    velocity = Vector3.zero;

                    // Reset bobbing effect smoothly
                    mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, defaultCameraPosition, Time.deltaTime * 8.0f);
                    bobbingTimer = 0; // Optionally, reset the bobbing timer
                }

                // Ensure Y-axis position doesn't change
                Vector3 move = velocity * Time.deltaTime;
                move.y = 0; // This will prevent any movement along the y-axis
                characterController.Move(move);

                // Correct the player's Y position
                Vector3 correctedPosition = transform.position;
                correctedPosition.y = initialPlayerY;
                transform.position = correctedPosition;

                // Rotate the player based on the mouse movement.
                float mouseX = Input.GetAxis("Mouse X");
                transform.Rotate(0, mouseX * mouseSensitivity, 0);

                // Rotate the camera based on the mouse movement, with inverted Y axis.
                float mouseY = -Input.GetAxis("Mouse Y");
                cameraRotationY += mouseY * mouseSensitivity;
                cameraRotationY = Mathf.Clamp(cameraRotationY, -90f, 90f);
                transform.GetChild(0).transform.localRotation = Quaternion.Euler(cameraRotationY, 0f, 0f);
            }

            ToggleInteractionText();

            // Check if the "E" key is pressed
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canMove)
                {
                    canMove = !DisplayMaterialOnHitObject();
                }
                else
                {
                    canMove = true;
                    nftDetailParent.DOFade(0.0f, 0.3f).OnComplete(() => { nftDetailParent.gameObject.SetActive(false); });
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!canMove)
                {
                    canMove = true;
                    nftDetailParent.DOFade(0.0f, 0.3f).OnComplete(() => { nftDetailParent.gameObject.SetActive(false); });
                }
            }
        }
    }
}
