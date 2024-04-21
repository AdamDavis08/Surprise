using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour, IEntity
{
    public CharacterController controller;
    public Animator animator;

    public GameObject jumpParticleEffect;
    public ParticleSystem runningParticleEffect;
    public Transform groundCheck;
    public Transform cam;
    public LayerMask groundMask;

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

    public static ThirdPersonMovement instance;
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

        RaycastHit hitInfo;
        if(Physics.Raycast(groundCheck.position, -Vector3.up, out hitInfo))
            transform.position = hitInfo.point;
    }

    private void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
                ResetPlayer();

            return;
        }

        // if(Input.GetMouseButtonDown(0))
        // {
        //     //TODO: SMENI GO KODOT MORASH GRD E
        //     RaycastHit hitInfo;
        //     if(Physics.Raycast(transform.GetChild(2).position, Camera.main.transform.forward, out hitInfo))
        //     {
        //         Debug.Log(hitInfo.collider.name);
        //         hitInfo.collider.TryGetComponent<EntityStats>(out EntityStats stats);
        //         if(stats != null) stats.TakeDamage(50);
        //     }
        // }

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

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(!ignoreGravity)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            Destroy(Instantiate(jumpParticleEffect, groundCheck.position, Quaternion.identity), 1f);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("isJumping", true);
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 move = (isSprinting) ? moveDir.normalized * sprintSpeed * Time.deltaTime : moveDir.normalized * speed * Time.deltaTime;
            controller.Move(move);
            animator.SetBool("isRunning", isSprinting);
            animator.SetBool("isWalking", !isSprinting);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }

        if (isSprinting && isGrounded) runningParticleEffect.emissionRate = 10f;
        else runningParticleEffect.emissionRate = 0f;
    }

    public void ResetPlayer()
    {
        controller.enabled = true;
        isDead = false;

        TryGetComponent<EntityStats>(out EntityStats stats);
        stats.health = 100f;

        animator.Rebind();
        animator.Update(0f);
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
}