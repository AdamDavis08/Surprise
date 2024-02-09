using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    public void Die();
}

[System.Serializable]
public class Town
{
    public GameObject townGameObject;
    public float sizeOfSquare = 1f;

    [HideInInspector]
    public Transform entityParent;

    public Vector3 position
    {
        get
        {
            return townGameObject.transform.position;
        }
    }

    public void CreateEntityParent()
    {
        entityParent = new GameObject("Entities").transform;
        entityParent.parent = townGameObject.transform;
        entityParent.localPosition = Vector3.zero;
    }
}

[System.Serializable]
public class Entity //: ScriptableObject
{
    public GameObject entityPrefab;
    public float spawnRate = 0.5f;

    public void SpawnEntity(Vector3 position, Transform parent = null)
    {
        GameObject entity = GameObject.Instantiate(entityPrefab, position, Quaternion.identity);
        entity.transform.parent = parent;
    }
}

public class EntityManager : MonoBehaviour
{
    public Town[] towns;
    public Entity entity;
    public float townRenderDistance = 100f;
    //public int maxNumberPerEntity = 10;

    public static EntityManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this);
            return;
        }

        foreach(Town town in towns)
        {
            town.CreateEntityParent();
            for (float x = town.position.x - town.sizeOfSquare / 2f; x <= town.position.x + town.sizeOfSquare / 2f; x++)
            {
                for (float z = town.position.z - town.sizeOfSquare / 2f; z <= town.position.z + town.sizeOfSquare / 2f; z++)
                {
                    if(Random.value > 1f - entity.spawnRate)
                    {
                        Vector3 position = new Vector3(x, 100f, z);
                        RaycastHit hitInfo;
                        if(Physics.Raycast(position, -Vector3.up, out hitInfo))
                        {
                            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                            {
                                position = hitInfo.point;
                                entity.SpawnEntity(position, town.entityParent);
                            }
                        }
                    }
                }
            }
        }

    }

    private void Update()
    {
        foreach(Town town in towns)
        {
            town.entityParent.gameObject.SetActive(MapGenerator.FindSqrDistanceBetweenTwoPoints(town.position, ThirdPersonMovement.playerPosition) <= townRenderDistance * townRenderDistance);
        } 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        foreach(Town town in towns) Gizmos.DrawWireCube(town.position, new Vector3(town.sizeOfSquare, 1f, town.sizeOfSquare));
    }
}
