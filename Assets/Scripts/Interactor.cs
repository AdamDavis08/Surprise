using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IInteractable
{
    public string GetInteractText();
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    public GameObject interactIndicator;
    public Transform interactorSource;
    public TextMeshProUGUI interactText;
    public static float interactRange = 3;
    public float interactCooldown = 0.1f;
    private float timer;

    public bool drawGizmos = true;
    public static bool inConversation;

    public static Interactor instance;
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if(inConversation) return;

        timer += Time.deltaTime;
        if (timer <= interactCooldown) return;

        IInteractable interactable = GetInteractable();
        if(interactable != null)
        {
            interactIndicator.SetActive(true);
            interactText.text = interactable.GetInteractText();

            if (Input.GetKeyDown(KeyCode.E))
            {
                interactable.Interact();
                timer = 0;
            }
        }
        else
            interactIndicator.SetActive(false);
    }

    public IInteractable GetInteractable()
    {
        Collider[] colliderArray = Physics.OverlapSphere(interactorSource.position, interactRange);

        foreach (Collider col in colliderArray)
        {
            if (col.TryGetComponent(out IInteractable interactable))
                return interactable;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(interactorSource.position, interactRange);
    }
}
