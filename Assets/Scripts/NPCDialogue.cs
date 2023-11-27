using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    public Dialogue dialogue;

    public void Interact()
    {
        DialogueManager dialogueManager = DialogueManager.instance;

        dialogueManager.dialoguePanel.SetActive(true);
        dialogueManager.nameOfCharaterTalking.text = dialogue.name;

        TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        DialogueManager dialogueManager = DialogueManager.instance;
        dialogueManager.StartDialogue(dialogue);
    }

    public string GetInteractText()
    {
        return interactText;
    }
}

[System.Serializable]
public class Dialogue 
{
    public string name;

    [TextArea(3, 10)]
    public string[] sentences;
}
