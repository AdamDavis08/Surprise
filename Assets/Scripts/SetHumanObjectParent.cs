using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHumanObjectParent : MonoBehaviour
{
    private void Start()
    {
        if (transform.parent == null) return;

        RaycastHit hitInfo;
        Vector3 origin = new Vector3(transform.position.x, 500f, transform.position.z);
        if(Physics.Raycast(origin, -Vector3.up, out hitInfo))
        {
            if(hitInfo.transform.gameObject.name.StartsWith("Chunk"))
            {
                GameObject glue = null;
                foreach(Transform child in hitInfo.transform)
                {
                    if(child.gameObject.name == "Glue")
                    {
                        glue = child.gameObject;
                        break;
                    }
                }

                if(glue == null)
                    glue = new GameObject("Glue");

                glue.transform.SetParent(hitInfo.transform);
                transform.SetParent(glue.transform);
            }
        }

        Destroy(this);
    }
}
