using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avatar : MonoBehaviour
{
    [Header("Movement Settings: ")]
    public float MovementSpeed = 7f;        // This is how fast the player will move when they are in a full sprint. Walking and Jogging will be factors of this value
    public float TurningSpeed = 15f;

    public float JoggingSpeedFactor = 0.75f;    // How fast the character will be when jogging. This is a % of 'MovementSpeed'
    public float WalkingSpeedFactor = 0.4f;     // How fast the character will be when walking. This is a % of 'MovementSpeed'
    private float speedFactor;

    public LayerMask traversableLayers;
    public AvatarMovementState movementState = AvatarMovementState.Normal;

    [Header("Jump and Crouch Settings: ")]
    public float JumpForce = 6f;
    public Vector3 JumpDirection = new Vector3(0f, 1f, 0.1f);
    private bool canJump = true;

    public float CrouchingSpeedFactor = 0.15f;
    public float CrouchTransitionSpeed = 15f;
    private float targetCrouchHeight = 0f;          // The Crouch Height we want to be at
    private float crouchHeight = 0f;                // The Crouch Height we are currently at.
    private bool isCrouching = false;

    public GameObject JumpSFX;

    [Header("Slope Movement Settings: ")]
    public float MaxTraversableSlope = 40f;
    public float SlideVelocity = 5f;
    public float SlideFriction = 0.3f;
    public float SlideTurnSpeed = 5f;

    private float currentSlideVelocity = 5f;
    private float TimeToReachSlideVelocity = 5f;
    private float slideTime = 0f;

    [Header("Avatar Health Settings: ")]
    public AvatarHealth m_AvatarHealth;

    #region Ragdoll
    [Header("Ragdoll Settings: ")]
    public float ragdollRest = 5f;
    public float RagdollEnterForce;
    private List<Rigidbody> r_Rigidbodies;
    private List<Collider> r_Colliders;
    private float ragdollRestTimer = 0f;
    #endregion

    [Header("Avatar Fall Settings:")]
    public float minimumFallHeight = 10f;
    public float damagePerFallDistance = 10f;
    public float fallingMovementFactor = 0.1f;
    private float lastHeight = 0f, currentFallDistance = 0f;

    [Header("Misc. Settings: ")]
    [Range(-4f,4f)]
    public float GravityMultiplier = 1f;
    public float MaximumHeadroom = 1.75f;
    public float MinimumHeadroom = 0.75f;
    public LayerMask heightAffectingLayers;
    private float availableHeadroom;
    public Transform ragdollRoot;

    [Header("Components: ")]
    public Rigidbody m_Rigidbody;
    public CapsuleCollider m_Collider;
    public Animator m_Animator;
    public Transform m_Head;

    private Vector3 playerInput = Vector3.zero;
    private Vector3 surfaceNormal = Vector3.up;
    private RaycastHit groundHit;
    RaycastHit headroomHit;
    private float groundCheckDistance = 0.3f;
    private bool isGrounded = true;

    public bool InWater = false;

    private bool standingUp = false;

    // Use this for initialization
    void Start ()
    {
        SetupRagdoll();

        movementState = AvatarMovementState.Normal;
        speedFactor = JoggingSpeedFactor;

        availableHeadroom = MaximumHeadroom;
	}

    private void SetupRagdoll()
    {
        r_Rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        r_Colliders = GetComponentsInChildren<Collider>().ToList();

        r_Rigidbodies.Remove(m_Rigidbody);
        r_Colliders.Remove(m_Collider);

        SetRagdollCollidersAndRigidbodies(false);
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        m_Animator.ResetTrigger("Jump");

        playerInput = Vector3.zero;

        CheckIfGrounded();
        CheckRagdollStatic();

        if (!isGrounded)
        {
            UpdateSpeedFactor(fallingMovementFactor);

            ApplyAdditionalGravity();

            currentFallDistance = lastHeight - transform.position.y;
            currentFallDistance = Mathf.Clamp(currentFallDistance, 0f, Mathf.Infinity);
        }
        else
        {
            CheckAvailableHeadroom();

            LerpAvatarCrouch();
        }

        if (InWater) canJump = true;

        m_Animator.SetBool("IsGrounded", isGrounded);
        m_Animator.SetBool("InWater", InWater);
    }

    public void Move(Vector3 inputVector, float movementFactor)
    {
        playerInput = inputVector;

        if (playerInput != Vector3.zero)
        {
            float speed = (MovementSpeed * inputVector.magnitude * speedFactor);

            if (movementState == AvatarMovementState.Normal)
            {
                if (inputVector.magnitude > 1f) inputVector.Normalize();

                inputVector.y = 0f;

                m_Rigidbody.transform.forward = Vector3.RotateTowards(m_Rigidbody.transform.forward, inputVector.normalized, TurningSpeed * Time.deltaTime, 0.0f);

                Vector3 moveForward = Vector3.Cross(transform.right, surfaceNormal);

                Vector3 newVelocity = moveForward.normalized * speed;

                newVelocity.y = m_Rigidbody.velocity.y;

                m_Rigidbody.velocity = newVelocity;
            }
        }
        else
        {
            m_Rigidbody.velocity = Vector3.Lerp(m_Rigidbody.velocity, Vector3.zero, Time.deltaTime);
        }

        m_Animator.SetFloat("MovementSpeed", inputVector.magnitude * speedFactor);
    }

    public void UpdateSpeedFactor(float factor)
    {
        speedFactor = Mathf.Lerp(speedFactor, factor, Time.deltaTime * 5f);
    }

    public void Jump()
    {
        if (canJump == true && movementState == AvatarMovementState.Normal)
        {
            m_Animator.SetTrigger("Jump");

            Vector3 myVelo = m_Rigidbody.velocity;
            myVelo.y = 0f;
            m_Rigidbody.velocity = myVelo;

            m_Rigidbody.AddForce(JumpDirection * JumpForce, ForceMode.Impulse);

            Instantiate(JumpSFX, transform.position, Quaternion.identity);

            isGrounded = false;
            canJump = false;

            // When we jump, we want to toggle crouching if we were crouching pre-jump
            if (isCrouching) Crouch();
        }
    }

    public void Crouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            // Now that we are crouching, we want to update the Animator, Collider, and character's speed.
            targetCrouchHeight = 1f;

            UpdateCharacterCollider(MinimumHeadroom);
            UpdateSpeedFactor(CrouchingSpeedFactor);
        }
        else
        {
            targetCrouchHeight = crouchHeight;

            UpdateCharacterCollider(availableHeadroom);
            UpdateSpeedFactor(JoggingSpeedFactor);
        }
    }

    public void EnterRagdoll()
    {
        // Seeing if we have any objects held and dropping it if we do
        AvatarInventory inventory = GetComponent<AvatarInventory>();
        inventory.DropObject();

        // Keeping a reference of the last velocity of the character.
        Vector3 previousVelocity = m_Rigidbody.velocity;

        // Immobilising the character.
        movementState = AvatarMovementState.Ragdoll;

        // Stopping thew Character's rigidbody from moving. We only want the ragdoll's components to
        // move the Avatar.
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;

        // Setting the velocity to 0.
        m_Rigidbody.velocity = Vector3.zero;

        // Disabling the animator and collider to stop them overriding the ragdoll.
        m_Collider.enabled = false;
        m_Animator.enabled = false;

        SetRagdollCollidersAndRigidbodies(true);

        r_Rigidbodies[0].velocity = previousVelocity * 5f;
    }

    // Used to exit the ragdoll state
    public void Recover()
    {
        // Checking to see if the Avatar is Dead.
        if (m_AvatarHealth.currentState == AvatarHealth.HealthState.Dead) return;

        // We only want to stand up if the ragdoll is not moving.
        if (movementState != AvatarMovementState.RagdollStatic) return;

        movementState = AvatarMovementState.RagdollStandingUp;

        // Calculating if the charater is face up or face down on the ground.
        float viewDir = Vector3.Dot(ragdollRoot.forward, Vector3.up);
        transform.position = ragdollRoot.position;                          // Setting the Avatars position to the ragdoll's root position.

        // Disabling the RB's and Colliders on the ragdoll and enabling the RB, Collider and Animator on the Avatar.
        SetRagdollCollidersAndRigidbodies(false);
        ToggleAnimatorAndCollider(true);
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;

        float animLength = 1f;

        if (viewDir < 0f)
        {
            m_Animator.Play("Stand Up Back");
            animLength = 3f;
        }
        else if (viewDir > 0f)
        {
            m_Animator.Play("Stand Up Front");
            animLength = 2f;
        }

        Debug.LogFormat("AnimationClip length: {0} seconds", animLength);

        Invoke("ResetMovementState", animLength);
    }

    private void SetRagdollCollidersAndRigidbodies(bool state)
    {
        foreach (Rigidbody rb in r_Rigidbodies)
        {
            rb.isKinematic = !state;
            rb.useGravity = state;
        }

        foreach (Collider col in r_Colliders)
        {
            col.enabled = state;
        }

    }

    private void CheckIfGrounded()
    {
        //RaycastHit groundHit;
        float sphereRadius = m_Collider.radius;

        Ray ray = new Ray(transform.position + (Vector3.up * (sphereRadius  + 0.1f)), Vector3.down);
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.green);

        //if (Physics.Raycast(ray, out groundHit, groundCheckDistance, traversableLayers, QueryTriggerInteraction.Ignore))
        if (Physics.SphereCast(ray, sphereRadius * 0.75f, out groundHit, groundCheckDistance + sphereRadius, traversableLayers, QueryTriggerInteraction.Ignore))
        {
            surfaceNormal = groundHit.normal;

            float angle = Vector3.Angle(surfaceNormal, transform.up);

            if (angle <= MaxTraversableSlope)
            {
                // Checking to see if we were last grounded.
                if (isGrounded == false)
                {
                    // As we were not grounded last frame, we want to check the distance we have fallen since we were last grounded
                    if (currentFallDistance >= minimumFallHeight)
                    {
                        float damageRatio = currentFallDistance / minimumFallHeight;

                        if (damageRatio > 1f)
                        {
                            m_AvatarHealth.TakeDamage((damageRatio * damageRatio) * damagePerFallDistance);
                        }
                    }
                }

                currentSlideVelocity = 5f;
                slideTime = 0f;
                isGrounded = true;
                canJump = true;

                // Tracking the info for fall damage.
                lastHeight = transform.position.y;
                currentFallDistance = 0f;

                return;
            }
            else
            {
                SlideDownSurface();
                isGrounded = false;
                return;
            }
        }

        currentSlideVelocity = 5f;
        slideTime = 0f;
        surfaceNormal = Vector3.up;
        isGrounded = false;

        canJump = false;
    }

    private void CheckRagdollStatic()
    {
        if (movementState != AvatarMovementState.Ragdoll) return;

        // We want to see if the root ragdoll is moving at all.
        Rigidbody rb = ragdollRoot.gameObject.GetComponent<Rigidbody>();

        if (rb.velocity.magnitude <= 0.01f)
        {
            ragdollRestTimer += Time.deltaTime;
        }
        else
        {
            ragdollRestTimer = 0f;
        }

        if (ragdollRestTimer >= ragdollRest)
        {
            movementState = AvatarMovementState.RagdollStatic;
        }
    }

    /// <summary>
    /// Fires a Spherecast up to check the headroom of the Avatar. Given the room, we will adjust the Avatar's crouching stance based on how much room we have.
    /// </summary>
    private void CheckAvailableHeadroom()
    {
        // Creating the RaycastHit and Ray for the SphereCast.

        // We do NOT do any sort of prediction of where the player is going to be, and the headroom there as it produced artifacts when walking to up walls.
        Ray ray = new Ray(transform.position, Vector3.up);

        // Firing the Spherecast up, if we hit something, check to see if the distance is greater than the minimum headroom, and update the character's 
        // Animator and speed.
        if (Physics.SphereCast(ray, (m_Collider.radius * 1.5f), out headroomHit, MaximumHeadroom, heightAffectingLayers, QueryTriggerInteraction.Ignore))
        {
            float distance = Vector3.Distance(transform.position, headroomHit.point);

            if (distance >= MinimumHeadroom)
            {
                // So we know that the Avatar can fit into the available space.
                availableHeadroom = distance - 0.05f;
            }
        }
        // The Spherecast has not hit anything above the Avatar, so we can assume the Avatar can stand up fully.
        else
        {
            availableHeadroom = MaximumHeadroom;
        }

        // Now that we know how much room the Avatar has available, we want to update the character's stance and speed. We will do this if the Avatar has specifically not been set
        // to crouch.
        if (!isCrouching)
        {
            targetCrouchHeight = Mathf.InverseLerp(MaximumHeadroom, MinimumHeadroom, availableHeadroom);

            UpdateCharacterCollider(availableHeadroom);
        }

        Debug.DrawRay(ray.origin, ray.direction * headroomHit.distance);
    }

    private void ApplyAdditionalGravity()
    {
        m_Rigidbody.AddForce(Physics.gravity * (GravityMultiplier - 1f));
    }

    private void UpdateCharacterCollider(float height)
    {
        // When we update the Collider's size, we want to set it to the maximum room they are in.
        m_Collider.height = height;
        m_Collider.center = new Vector3(m_Collider.center.x, m_Collider.height / 2, m_Collider.center.z);
    }

    private void LerpAvatarCrouch()
    {
        float currentCrouchAmount = m_Animator.GetFloat("CrouchAmount");

        crouchHeight = Mathf.Lerp(currentCrouchAmount, targetCrouchHeight, CrouchTransitionSpeed * Time.deltaTime);
        m_Animator.SetFloat("CrouchAmount", crouchHeight);
    }

    /// <summary>
    /// Handles sliding down surfaces which are angled greater than MaxTraversableSlope.
    /// </summary>
    private void SlideDownSurface()
    {
        slideTime += Time.deltaTime;
        slideTime = Mathf.Clamp(slideTime, 0f, TimeToReachSlideVelocity);

        // We want to slide down the surface of whatever we're on
        Vector3 slideDirection = new Vector3(0f, m_Rigidbody.velocity.y, 0f);

        currentSlideVelocity = Mathf.Lerp(currentSlideVelocity, SlideVelocity, slideTime / TimeToReachSlideVelocity) - SlideFriction;

        slideDirection.x += ((1f - surfaceNormal.y) * surfaceNormal.x * currentSlideVelocity);
        slideDirection.z += (1f - surfaceNormal.y) * surfaceNormal.z * currentSlideVelocity;

        m_Rigidbody.velocity = slideDirection;

        // Rotate the character to rotate towards the sliding direction.
        Vector3 lookDirection = slideDirection;
        lookDirection.y = 0f;

        transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, SlideTurnSpeed * Time.deltaTime, 0.0f);
    }

    /// <summary>
    /// Handles logic for when an object collides with the Avatar. This collision could hit anywhere
    /// on the Avatar.
    /// </summary>
    /// <param name="impactVecolity"></param>
    public void ImpactCollision(Vector3 impactVelocity)
    {
        if (impactVelocity.magnitude >= RagdollEnterForce)
        {
            EnterRagdoll();

            // Here we want to apply some damage to the Avatar based on the force applied to them.
            float damage = impactVelocity.magnitude / RagdollEnterForce * 50f;

            m_AvatarHealth.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Applies a force to the ragdoll, at a specified rigidbody.
    /// </summary>
    /// <param name="bodyPart"> Part of the body which the force shall be applied to. </param>
    /// <param name="force"> Force to be applied to the specified rigidbody. </param>
    public void ApplyForceToBodyPart(Rigidbody bodyPart, Vector3 force)
    {
        // Checking to see if we are trying to apply a valid force to a valid rigidbody.
        if (force == Vector3.zero || bodyPart == null)
        {
            // Returning out of the method if the rigidbody is invalid, or we are not applying a force.
            return;
        }

        if (force.magnitude >= RagdollEnterForce)
        {
            EnterRagdoll();

            bodyPart.AddForce(force, ForceMode.Impulse);
        }
    }

    private void ToggleAnimatorAndCollider(bool state)
    {
        m_Animator.enabled = state;
        m_Collider.enabled = state;
    }

    private void ResetMovementState()
    {
        movementState = AvatarMovementState.Normal;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundHit.point, m_Collider.radius );
    }

    public float GetSpeedFactor()
    {
        return speedFactor;
    }
}

public enum AvatarMovementState
{
    Normal, Hindered, Ragdoll, RagdollStatic, RagdollStandingUp
}
