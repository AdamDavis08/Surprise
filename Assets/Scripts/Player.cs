using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour, IEntity
{
    public CharacterController controller;
    public Animator animator;

    public GameObject jumpParticleEffect;
    public ParticleSystem runningParticleEffect;
    public Transform groundCheck;
    public Transform cam;
    public LayerMask groundMask;
    public LayerMask ignoreAttackMask;

    public float speed = 6f;
    public float sprintSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float groundDistance = 0.4f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    public static bool isDead = false;
    public bool ignoreGravity;
    private bool isSprinting;
    private bool isGrounded;
    private Vector3 velocity;

    private CinemachineFreeLook cinemachineFreeLook;
    private CinemachineCameraOffset camOffset;
    private PlayerInputActions playerActions;
    
    public static Player instance;
    public static Vector3 playerPosition
    {
        get
        {
            return instance.transform.position;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ZoomOptions.cmOpts = FindObjectOfType<CinemachineFreeLook>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        cinemachineFreeLook = FindObjectOfType<CinemachineFreeLook>();
        camOffset = cinemachineFreeLook.GetComponent<CinemachineCameraOffset>();
        
        playerActions = new PlayerInputActions();
        playerActions.Player.Enable();
        playerActions.Player.Attack.performed += Attack;
        playerActions.Player.Jump.performed += Jump;

        RaycastHit hitInfo;
        if(Physics.Raycast(groundCheck.position, -Vector3.up, out hitInfo))
            transform.position = hitInfo.point;
    }
    
    private void Jump(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;
        
        Destroy(Instantiate(jumpParticleEffect, groundCheck.position, Quaternion.identity), 1f);
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetBool("isJumping", true);
    }

    private void Move(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        Vector3 move = (isSprinting) ? moveDir.normalized * sprintSpeed * Time.deltaTime : moveDir.normalized * speed * Time.deltaTime;
        controller.Move(move);
        animator.SetBool("isRunning", isSprinting);
        animator.SetBool("isWalking", !isSprinting);
            
        if (isSprinting && isGrounded) runningParticleEffect.emissionRate = 10f;
        else runningParticleEffect.emissionRate = 0f;
    }

    private void Attack(InputAction.CallbackContext context)
    {
        //TODO: SMENI GO KODOT MORASH GRD E
        RaycastHit hitInfo;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 1000f, ~ignoreAttackMask))
        {
            Debug.Log(hitInfo.collider.gameObject.name);
            hitInfo.collider.TryGetComponent<EntityStats>(out EntityStats stats);
            if(stats != null) stats.TakeDamage(50);
        }
    }
    
    private void Aim()
    {
        Vector3 result = new Vector3(cam.forward.x, 0f, cam.forward.z);
        transform.forward = Vector3.Lerp(transform.forward, result, (turnSmoothTime * 100f) * Time.deltaTime);
        camOffset.m_Offset = Vector3.Lerp(camOffset.m_Offset, Vector3.right, turnSmoothTime);
        cinemachineFreeLook.m_Lens.FieldOfView = Mathf.Lerp(cinemachineFreeLook.m_Lens.FieldOfView, 20f, turnSmoothTime);
        UIManager.SetCrosshairVisibility(true);
    }
    
    public void Die()
    {
        if (isDead) return;

        if(!isGrounded)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo))
                transform.position = hitInfo.point;
        }

        controller.enabled = false;
        isDead = true;
        animator.SetTrigger("die");
    }
    
    private void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
                ResetPlayer();

            return;
        }
        
        if (Input.GetMouseButton(1))
        {
            Aim();
        }
        else
        {
            if (camOffset.m_Offset != Vector3.zero || cinemachineFreeLook.m_Lens.FieldOfView != 40)
            {
                camOffset.m_Offset = Vector3.Lerp(camOffset.m_Offset, Vector3.zero, turnSmoothTime);
                cinemachineFreeLook.m_Lens.FieldOfView = Mathf.Lerp(cinemachineFreeLook.m_Lens.FieldOfView, 40f, turnSmoothTime);
                UIManager.SetCrosshairVisibility(false);   
            }
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !ignoreGravity;
        animator.SetBool("isGrounded", isGrounded);

        if (!isGrounded) animator.SetBool("isFalling", true);
        else animator.SetBool("isFalling", false);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("isJumping", false);
        }
        
        Vector2 direction = playerActions.Player.Movement.ReadValue<Vector2>().normalized;

        if(!ignoreGravity)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        if (direction.sqrMagnitude >= 0.1f) 
            Move(direction);
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            
            runningParticleEffect.emissionRate = 0f;
        }
    }

    private void ResetPlayer()
    {
        controller.enabled = true;
        isDead = false;

        TryGetComponent<EntityStats>(out EntityStats stats);
        stats.health = 100f;

        animator.Rebind();
        animator.Update(0f);
    }
}