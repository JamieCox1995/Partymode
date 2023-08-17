using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarUserControl : MonoBehaviour
{
    private Avatar _Avatar;
    private AvatarInventory _Inventory;
    private bool isCrouching = false;

    [Header("World Interaction Settings:")]
    public Transform eyeLocation;
    public LayerMask interactableObjectLayer;
    public float reach = 2f;
    private Interactable currentlyHighlightedObject;

    #region Camera Settings;
    [Header("Camera Settings: ")]
    public float cameraSensitivity = 4f;
    public float cameraOffset = 6f;

    public float cameraAngleClamp = 80f;
    public Transform cameraTarget;

    // Private variables to do with the camera.
    private Camera mainCamera;
    private Transform cameraParent;
    private Vector3 localRotation = new Vector3();
    #endregion

    // Use this for initialization
    void Start ()
    {
        _Avatar = GetComponent<Avatar>();
        _Inventory = GetComponent<AvatarInventory>();

        SetupCamera();
	}

    private void SetupCamera()
    {
        mainCamera = Camera.main;
        cameraParent = mainCamera.transform.parent;

        mainCamera.transform.localPosition = new Vector3(0f, 0f, -cameraOffset);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (_Avatar.m_AvatarHealth.currentState == AvatarHealth.HealthState.Dead) return;

        if (_Avatar.movementState != AvatarMovementState.Normal && Input.anyKeyDown)
        {
            _Avatar.Recover();
        }

        // Here we are going to detect for actions such as jumping and crouching.
        HandleControlInput();

        if (_Inventory.HoldingItem && Input.GetKeyDown(KeyCode.E))
        {
            _Inventory.DropObject();
        }

        WorldInteraction();
	}

    private void FixedUpdate()
    {
        // Here we are going to do all of the logic for MOVING the player with WASD. We won't do stuff like Jumping/Crouching in here.
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if (mainCamera == null)
        {
            if (Camera.main == null)
            {
                throw new System.Exception("There is no MainCamera in the scene, please create one to allow the movement to work.");
            }

            SetupCamera();
        }

        Vector3 playerInput = new Vector3
        {
            x = Input.GetAxis("Horizontal"),
            z = Input.GetAxis("Vertical")
        };

        // Working out which way the camera is looking.
        Vector3 forward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;

        _Avatar.Move(playerInput.x * right + playerInput.z * forward, 1f);
    }

    // This is the methods which shall be used to detect whether crouching/jumping/running/walking has been done.
    private void HandleControlInput()
    {
        #region Movement Speed Handling
        // Detecting to see if the player is sprinting.
        if (Input.GetButton("Sprint"))
        {
            // We are assuming that sprinting is a speed factor of 1f.
            _Avatar.UpdateSpeedFactor(1f);
        }

        if (Input.GetButton("Walk"))
        {
            _Avatar.UpdateSpeedFactor(_Avatar.WalkingSpeedFactor);
        } 

        if (isCrouching)
        {
            _Avatar.UpdateSpeedFactor(_Avatar.CrouchingSpeedFactor);
        }

        if (!Input.GetButton("Sprint") && !Input.GetButton("Walk") && !isCrouching)
        {
            _Avatar.UpdateSpeedFactor(_Avatar.JoggingSpeedFactor);
        }
        #endregion

        // Special Movement Handling
        if (Input.GetButtonDown("Jump"))
        {
            _Avatar.Jump();
            isCrouching = false;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            _Avatar.Crouch();
            isCrouching = !isCrouching;
        }
    }

    /// <summary>
    /// We are going to use LateUpdate to update all of our camera behaviours.
    /// </summary>
    private void LateUpdate()
    {
        // Here we are going to do the updates for the camera orbitting.
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        cameraParent.position = cameraTarget.transform.position;

        localRotation.y += Input.GetAxis("Mouse X") * cameraSensitivity;
        localRotation.x -= Input.GetAxis("Mouse Y") * cameraSensitivity;

        localRotation.x = Mathf.Clamp(localRotation.x, -cameraAngleClamp, cameraAngleClamp);

        Quaternion newRot = Quaternion.Euler(localRotation);

        cameraParent.rotation = newRot;
    }

    // Checks to see if we are looking at an interactable object.
    private void WorldInteraction()
    {
        // Firing a Raycast
        Ray ray = new Ray(eyeLocation.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray.origin, 0.5f, ray.direction, out hit, reach, interactableObjectLayer))
        {
            Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();

            if (interactable == null) return;

            if (interactable != currentlyHighlightedObject && currentlyHighlightedObject != null)
            {
                currentlyHighlightedObject.UnHighlightObject();
            }

            currentlyHighlightedObject = interactable;
            interactable.ObjectHighlighted(_Avatar);

            if (Input.GetKeyDown(KeyCode.E))
            {
                interactable.InvokePrimaryAction();
            }

            if (Input.GetMouseButtonDown(1))
            {
                interactable.InvokeSecondaryAction();
            }
        }
        else
        {
            if (currentlyHighlightedObject != null)
            {
                currentlyHighlightedObject.UnHighlightObject();
                currentlyHighlightedObject = null;
            }
        }
    }
}
