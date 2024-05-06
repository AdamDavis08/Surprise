using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bow : Weapon
{
    public GameObject arrowPrefab;
    
    public override void Equip()
    {
        Debug.Log("Equiping");
    }

    public override void Attack()
    {
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(Player.instance.transform.forward));
        arrow.GetComponent<Rigidbody>().velocity = Camera.main.transform.forward * 30f;
    }
}
