using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Enemy : MonoBehaviour, IEntity
{
    public CharacterController controller;
    public Animator animator;

    public Transform hitCheck;
    public Transform groundCheck;
    public LayerMask groundMask;

    [Header("Movement")]
    public float walkSpeed = 1f;
    public float sprintSpeed = 8f;
    private float speed = 1f;

    public float turnSmoothTime = 0.1f;
    public float waitTime = 2f;
    public float gravity = -9.81f;
    public float groundDistance = 0.4f;
    public float attackDistance = 2f;
    private float timeToSameWander;
    private float screamLength = 2.8f;

    private Vector3 velocity;
    private Vector3 wanderPoint;

    public bool wander;
    public bool run;
    private bool isDead = false;
    private bool seenPlayer;
    private bool isGrounded;
    private bool isAttacking = false;

    [Header("Combat")]
    public float attackCooldown = 1f;
    public float attackRadius = 1f;
    public float damage = 10f;

    [Header("Field Of View")]
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    private void Awake()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips)
        {
            switch(clip.name)
            {
                case "Scream":
                    screamLength = clip.length;
                    break;
                case "Attack":
                    attackCooldown = clip.length;
                    break;
            }
        }

        speed = (run) ? sprintSpeed : walkSpeed;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        FindVisibleTargets();
        if(seenPlayer)
        {
            run = true;

            MoveTowardsTarget(ThirdPersonMovement.playerPosition);
            if (MapGenerator.FindSqrDistanceBetweenTwoPoints(ThirdPersonMovement.playerPosition, transform.position) >= 2500f|| ThirdPersonMovement.isDead) //koren od 2500f e 50f
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", false);

                seenPlayer = false;
                run = false;
                ResetWanderPoint();
            }
            return;
        }

        if (wander && speed > 0f)
        {
            if (wanderPoint == Vector3.zero) 
                ResetWanderPoint();

            timeToSameWander += Time.deltaTime;
            if (timeToSameWander >= 20f) 
                ResetWanderPoint();

            MoveTowardsTarget(wanderPoint);
        }
    }

    private void ResetWanderPoint()
    {
        timeToSameWander = 0F;
        wanderPoint = NPC.PointToWander(transform, NPC.wanderRadius);
    }

    private void TurnTowardsTarget(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSmoothTime * Time.deltaTime);
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 offset = DirectionFromTarget(transform.position, target);
        if (offset.sqrMagnitude > attackDistance * attackDistance)
        {
            TurnTowardsTarget(offset);

            animator.SetBool("isRunning", run); 
            animator.SetBool("isWalking", !run);
            animator.SetBool("isAttacking", false);

            Vector3 move = offset.normalized * speed * Time.deltaTime;
            controller.Move(move);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);

            if (seenPlayer)
            {
                if(!isAttacking)
                {
                    StartCoroutine(Attack());
                }
            }
            else 
            { 
                if (wander) wanderPoint = Vector3.zero;

                if (speed != 0f) StartCoroutine(Wait());
            }
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);
        speed = 0f;

        float startTime = Time.time;
        bool hitSomething = false;
        while (Time.time < startTime + attackCooldown)
        {
            Collider[] hitTargets = Physics.OverlapSphere(hitCheck.position, attackRadius, targetMask);
            foreach (Collider target in hitTargets)
            {
                if(target.TryGetComponent<EntityStats>(out EntityStats stats) && !hitSomething)
                {
                    stats.TakeDamage(damage);
                    hitSomething = true;
                    break;
                }
            }
            yield return null;
        }
        speed = (run) ? sprintSpeed : walkSpeed;
        isAttacking = false;
    }

    private IEnumerator Wait()
    {
        speed = 0f;
        yield return new WaitForSeconds(waitTime);
        speed = (run) ? sprintSpeed : walkSpeed;
    }

    private IEnumerator Scream()
    {
        animator.SetTrigger("scream");
        speed = 0f;
        yield return new WaitForSeconds(screamLength);
        speed = (run) ? sprintSpeed : walkSpeed;
    }

    public void Die()
    {
        if (isDead) return;

        if (!isGrounded)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo))
                transform.position = hitInfo.point;
        }

        controller.enabled = false;
        isDead = true;
        animator.SetTrigger("die");
    }

    public static Vector3 DirectionFromTarget(Vector3 center, Vector3 target)
    {
        return new Vector3(target.x - center.x, 0f, target.z - center.z);
    }

    public static Vector3 DirectionFromAngle(float angleInDegrees)
    {
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    } 

    private void FindVisibleTargets()
    {
        if (seenPlayer) return;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, target.position, dstToTarget, obstacleMask))
                {
                    seenPlayer = true;
                    StartCoroutine(Scream());
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, NPC.wanderRadius);

        if (wanderPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wanderPoint, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, viewRadius);

        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2f + transform.eulerAngles.y);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2f + transform.eulerAngles.y);
        Handles.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Handles.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(hitCheck.position, attackRadius);
    }
}
