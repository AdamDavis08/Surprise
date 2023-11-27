using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour, IInteractable
{
    [Header("Necessary Objects")]
    public ParticleSystem smokeParticleEffect;

    public Transform cameraLookAt;
    public Transform exitPosition;

    public Rigidbody rb;
    public Animator anim;

    [Space(10)]

    [Header("Movement Settings")]
    public float movementSpeed;
    public float steerSpeed = 10f;
    public Vector3 rotationVector = new Vector3(0f, 1f, 0f);
    public Vector3 forwardVector = new Vector3(0f, 0f, 1f);
    private Vector3 velocity;
    
    private bool hasPlayer;

    private void Update()
    {
        if (hasPlayer)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Exit();
            }
        }

        Move();
    }

    private void Move()
    {
        float steer = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        transform.parent.Rotate(steer * steerSpeed * rotationVector * Time.deltaTime);
        transform.parent.Translate(forward * movementSpeed * forwardVector * Time.deltaTime);
        velocity = forward * forwardVector;
    }

    private void Enter()
    {
        ThirdPersonMovement.instance.gameObject.SetActive(false);
        ZoomOptions.cmOpts.Follow = transform.parent;
        ZoomOptions.cmOpts.LookAt = cameraLookAt;
        ZoomOptions.ChangeZoom(6);

        anim.enabled = true;
        smokeParticleEffect.gameObject.SetActive(true);
        this.enabled = true;
        hasPlayer = true;

        smokeParticleEffect.Play();
        anim.SetBool("hasPlayer", hasPlayer);
        rb.isKinematic = false;
        GetComponent<MeshCollider>().convex = true;
        GetComponent<MeshCollider>().isTrigger = true;

        MapGenerator.player = transform.parent;
        Interactor.instance.interactIndicator.SetActive(false);
    }

    private void Exit()
    {
        ThirdPersonMovement.instance.gameObject.SetActive(true);
        ThirdPersonMovement.instance.transform.position = exitPosition.position;
        ZoomOptions.cmOpts.Follow = ThirdPersonMovement.instance.transform;
        ZoomOptions.cmOpts.LookAt = ThirdPersonMovement.instance.transform.GetChild(2);
        ZoomOptions.ChangeZoom(2);

        this.enabled = false;
        hasPlayer = false;

        smokeParticleEffect.Stop();
        anim.SetBool("hasPlayer", hasPlayer);
        rb.isKinematic = true;
        GetComponent<MeshCollider>().isTrigger = false;
        GetComponent<MeshCollider>().convex = false;

        MapGenerator.player = ThirdPersonMovement.instance.transform;
    }

    public void Interact()
    {
        if (!hasPlayer)
            Enter();
    }

    public string GetInteractText()
    {
        return "Enter boat";
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Water"))
            transform.parent.Translate(-velocity);
    }
}
