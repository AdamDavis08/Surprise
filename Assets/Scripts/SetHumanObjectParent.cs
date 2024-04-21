using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHumanObjectParent : MonoBehaviour
{
    private void Start()
    {
        if (transform.parent == null) return;

        int i = 0;
        while(transform.childCount > 0)
        {
            Transform child = transform.GetChild(i);
            child.GetComponentInChildren<Collider>().enabled = false;    
        
            RaycastHit hitInfo;
            Vector3 origin = new Vector3(child.position.x, 500f, child.position.z);
            if(Physics.Raycast(origin, -Vector3.up, out hitInfo))
            {
                if (hitInfo.transform.gameObject.name.StartsWith("Crystal Haven"))
                {
                    child.SetParent(hitInfo.transform);
                }
                else
                    i++;
            }

            child.GetComponentInChildren<Collider>().enabled = true;
        }
        
        Destroy(this);
    }
}
