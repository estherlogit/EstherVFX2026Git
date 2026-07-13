using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class MovementHandler : MonoBehaviour
{
    // Rigidbody of the movement target
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    
    [Header("Movement Parameters")]
    // Speed of our movable
    [SerializeField]
    internal float moveSpeed = 6;
    // How high our movable can jump
    [SerializeField]
    internal float maxJumpHeight = 3.5f;
    // How low our movable can jump
    [SerializeField]
    internal float minJumpHeight = 1;
    // How long will take to the movable to reach the highest point of the jump
    [SerializeField]
    internal float timeToJumpApex = 0.4f;
    // Acceleration while changing directions being airborne
    internal float accelerationTimeAirborne = 0.2f;
    // Acceleration while changing directions being grounded
    internal float accelerationTimeGrounded = 0.1f;
    
    // Movable's velocity
    [SerializeField]
    internal Vector2 velocity;
    // Vertical jump maximum velocity 
    private float maxJumpVelocity;
    // Vertical jump minimum velocity
    private float minJumpVelocity;
    
    // Gravity 
    private float gravity;
    // Ground check
    internal bool isGrounded;
    // Attack check
    internal bool isAttacking;
    // Locomotion lock check
    internal bool isLocked = true;

    // Horizontal movement smoothing
    internal float velocityXSmoothing;
    
    // Direction
    [SerializeField]
    internal Vector2 directionalInput;
    // Right = 1 , Left = -1
    internal float currentHorizontalDirection;

    internal float turnSmoothVelocity;
    [SerializeField] 
    internal Transform jeanetteTransform;
    [SerializeField] 
    internal float turnSmoothTime;

    internal int _attackType = 1;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        
        // Sets gravity depending on our movable properties
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        // Sets maximum jumpVelocity depending on the gravity and the time needed to reach the apex of our jump
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        // Sets minimum jumpVelocity depending on the gravity and the minimum jump height
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        // Sets horizontal direction
        currentHorizontalDirection = 1;
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        CalculateRotation();

        if (!isAttacking && !isLocked)
        {
            Move(velocity * Time.deltaTime);
        }
    }

    public void SetHorizontalInput(float inputX)
    {
        directionalInput.x = inputX;
        
        if (directionalInput.x != 0)
        {
            currentHorizontalDirection = directionalInput.x;
        }
    }

    void CalculateVelocity()
    {
        // Sets horizontal speed
        if (!isLocked && !isAttacking)
        {
            // Calculates horizontal speed
            float targetVelocityX = directionalInput.x * moveSpeed;
            
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        else
        {
            velocity.x = 0;
        }
        
        // Applies vertical gravity if jumping
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0;
        }
    }

    void CalculateRotation()
    {
        if (directionalInput.x != 0 && isGrounded && !isLocked)
        {
            float targetRotation = Mathf.Atan2(directionalInput.x, 0f) * Mathf.Rad2Deg;
            jeanetteTransform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(jeanetteTransform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
    }
    
    public void Move(Vector2 deltaMovement)
    {
        // Applies movement
        transform.Translate(deltaMovement);
        Physics.SyncTransforms();
    }

    public void OnJumpInputDown()
    {
        directionalInput.y = 1;
        velocity.y = maxJumpVelocity;
        isGrounded = false;
    }

    public void OnJumpInputUp()
    {
        directionalInput.y = 0;
    }
    
    public void OnAttackInputDown()
    {
        velocity.x = 0;
        isAttacking = true;
    }

    public void OnAttackInputUp()
    {
        isAttacking = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isLocked = false;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            velocity.x = 0;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
        
        if (other.gameObject.CompareTag("Wall"))
        {
            
        }
    }
}
