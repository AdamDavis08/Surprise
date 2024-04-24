using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInWaterChecker : MonoBehaviour
{
    public static bool isUnderwater = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player player = Player.instance;
            player.ignoreGravity = true;
            player.animator.SetTrigger("inWater");
            player.animator.SetBool("outOfWater", false);
            isUnderwater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            Player.instance.ignoreGravity = false;
            Player.instance.animator.SetBool("outOfWater", true);
            isUnderwater = false;
        }
    }
}
