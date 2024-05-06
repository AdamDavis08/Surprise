using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 30f;
    public float despawnAfter = 60f;
    private Rigidbody rb;
    
    private void Start()
    {
        Destroy(gameObject, despawnAfter);
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            EntityStats stats = other.GetComponentInParent<EntityStats>();
            stats.TakeDamage(damage);
        }
        catch (NullReferenceException e) { }
        
        transform.parent = other.transform;
        Destroy(rb);
        Destroy(this);
    }
}
