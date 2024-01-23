using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatAI : MonoBehaviour
{
    public float movementSpeed;
    public float steerSpeed = 10f;
    public float wanderRadius = 4f; 

    private float timeToSameWander = 0f;
    private Vector3 wanderPoint;

    private void Update()
    {
        if(wanderPoint == Vector3.zero)
        {
            ResetWanderPoint();
        }

        timeToSameWander += Time.deltaTime;
        if (timeToSameWander >= 10f)
        {
            ResetWanderPoint();
        }

        MoveTowardsTarget(wanderPoint);
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 offset = new Vector3(target.x - transform.position.x, 0f, target.z - transform.position.z);
        if (offset.magnitude > .5f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(offset);
            transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, lookRotation, steerSpeed * Time.deltaTime);

            transform.parent.Translate(movementSpeed * Vector3.forward * Time.deltaTime);
        }
        else
            ResetWanderPoint();
    }

    private void ResetWanderPoint()
    {
        timeToSameWander = 0F;
        wanderPoint = PointToWander();
    }

    private Vector3 PointToWander()
    {
        Vector3 randPoint = transform.position + GetPointOnUnitCircleCircumference() * wanderRadius;
        Collider[] hitColliders = Physics.OverlapSphere(randPoint, 1f);

        while(hitColliders.Length != 0)
        {
            randPoint = transform.position + GetPointOnUnitCircleCircumference() * wanderRadius;
            hitColliders = Physics.OverlapSphere(randPoint, 1f);
        }

        randPoint.y = transform.position.y;
        return randPoint;
    }

    private Vector3 GetPointOnUnitCircleCircumference()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle)).normalized;
    }
}
