using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInWaterChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ThirdPersonMovement.instance.ignoreGravity = true;
            ThirdPersonMovement.instance.animator.SetTrigger("inWater");
            ThirdPersonMovement.instance.animator.SetBool("outOfWater", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            ThirdPersonMovement.instance.ignoreGravity = false;
            ThirdPersonMovement.instance.animator.SetBool("outOfWater", true);
        }
    }
}
