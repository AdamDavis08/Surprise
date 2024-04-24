using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameOfCharaterTalking;
    public TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed;

    public static DialogueManager instance;
    public static int activeSentenceIndex = -1;
    private bool inConversation;

    private void Awake()
    {
        if(instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        sentences = new Queue<string>();
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            DisplayNextSentence();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        sentences.Clear();
        Interactor.inConversation = true;

        if(Player.instance != null)
        {
            Player.instance.enabled = false;
            ZoomOptions.cmOpts.enabled = false;
        }

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0) 
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        activeSentenceIndex++;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Interactor.inConversation = false;

        if(Player.instance == null) return;

        Player.instance.enabled = true;
        ZoomOptions.cmOpts.enabled = true;
    }
}
