using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected LayerMask ignoreAttackMask;
    [SerializeField] protected Transform shootPoint;
    protected bool isEquiped;

    public abstract void Equip();

    public virtual void Attack()
    {
        //TODO: SMENI GO KODOT MORASH GRD E
        RaycastHit hitInfo;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, 1000f, ~ignoreAttackMask))
        {
            Debug.Log(hitInfo.collider.gameObject.name);
            hitInfo.collider.TryGetComponent<EntityStats>(out EntityStats stats);
            if(stats != null) stats.TakeDamage(50);
        }
    }
}
