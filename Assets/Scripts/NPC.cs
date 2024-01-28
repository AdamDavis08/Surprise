using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public GameObject interactIndicator;
    public Transform groundCheck;
    public CharacterController controller;
    public Animator animator;
    public LayerMask groundMask;

    public float speed = 6f;
    public float sprintSpeed = 12f;
    public float turnSmoothTime = 0.1f;
    public float waitTime = 2f;
    public float gravity = -9.81f;
    public float groundDistance = 0.4f;
    public float wanderRadius = 4f; 
    public float timeToSameWander;

    public Vector3[] waypoints;
    private Vector3 velocity;
    private Vector3 wanderPoint;

    private int nextWaypointIndex = 0;
    public bool repeatPath;
    public bool run;
    public bool wander;
    public bool drawWaypoints;
    private bool isWaiting;
    private bool isGrounded;

    private void Update()
    {
        //if (MapGenerator.FindSqrDistanceBetweenTwoPoints(transform.position, MapGenerator.player.position) > MapGenerator.distanceToColliderThreshold * MapGenerator.distanceToColliderThreshold)
        //{
        //    gameObject.SetActive(false);
        //    return;
        //}

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        //animator.SetBool("isGrounded", isGrounded);

        //if (!isGrounded) animator.SetBool("isFalling", true);
        //else animator.SetBool("isFalling", false);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            //animator.SetBool("isJumping", false);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if(wander && !isWaiting)
        {
            if(wanderPoint == Vector3.zero)
            {
                timeToSameWander = 0f;
                wanderPoint = PointToWander();
            }

            timeToSameWander += Time.deltaTime;
            if (timeToSameWander >= 5f)
            {
                timeToSameWander = 0F;
                wanderPoint = PointToWander();
            }

            MoveTowardsTarget(wanderPoint);
            return;
        }

        if(waypoints.Length > 0)
            MoveTowardsTarget(waypoints[nextWaypointIndex]);
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 offset = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
        if (offset.magnitude > .1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(offset);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSmoothTime * Time.deltaTime);

            //animator.SetBool("isRunning", run); 
            //animator.SetBool("isWalking", !run);

            Vector3 move = (run) ? offset.normalized * sprintSpeed * Time.deltaTime : offset.normalized * speed * Time.deltaTime;
            controller.Move(move);
        }
        else
        {
            //animator.SetBool("isRunning", false);
            //animator.SetBool("isWalking", false);
            if (wander) wanderPoint = Vector3.zero;

            if(!isWaiting) StartCoroutine(Wait());
        }
    }

    private IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        if (nextWaypointIndex < waypoints.Length - 1) nextWaypointIndex++;
        else if (repeatPath) nextWaypointIndex = 0;

        isWaiting = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawWaypoints) return;

        for(int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[i], 1f);
        }
    }

    private Vector3 PointToWander()
    {
        Vector3 randPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        Collider[] hitColliders = Physics.OverlapSphere(randPoint, 1f);

        while(hitColliders.Length != 0)
        {
            randPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            hitColliders = Physics.OverlapSphere(randPoint, 1f);
        }

        RaycastHit hitInfo;
        randPoint.y = 500f;
        if(Physics.Raycast(randPoint, -Vector3.up, out hitInfo))
        {
            if(hitInfo.transform.gameObject.name.StartsWith("Chunk"))
            {
                randPoint = hitInfo.point;
            }
        }

        return randPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        if(wanderPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wanderPoint, 1f);
        }
    }
}
