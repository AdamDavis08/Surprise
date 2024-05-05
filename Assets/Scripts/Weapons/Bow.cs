using System;
using System.Collections;
using System.Collections.Generic;
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
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        arrow.GetComponent<Rigidbody>().AddForce(Player.instance.transform.forward * 1000f);
    }
}
