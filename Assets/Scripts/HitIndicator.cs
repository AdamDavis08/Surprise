using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitIndicator : MonoBehaviour
{
    public LayerMask targetMask;

    private void Awake()
    {
        targetMask = GetComponentInParent<Enemy>().targetMask;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetMask) != 0)
        {
            Debug.Log("HIT");
        }
    }
}
