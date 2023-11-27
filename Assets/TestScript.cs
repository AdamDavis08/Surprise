using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public float speed = 0.5f;
    public Dialogue dialogue;
    private Transform mainCam;

    private bool move = false;
    private bool shaken = false;

    private void Start()
    {
        StartCoroutine(AfterStart());
    }

    private IEnumerator AfterStart()
    {
        yield return null;
        mainCam = Camera.main.transform;

        DialogueManager dialogueManager = DialogueManager.instance;
        
        dialogueManager.dialoguePanel.SetActive(true);
        dialogueManager.nameOfCharaterTalking.text = dialogue.name;

        dialogueManager.StartDialogue(dialogue);
    }

    private void Update()
    {
        //if(Input.GetMouseButtonDown(0))
        //    StartCoroutine(CameraShake.Shake(Camera.main.transform, 1f, 5f));

        switch(DialogueManager.activeSentenceIndex)
        {
            case 8:
                move = true;
                break;
            case 11:
                if(!shaken)
                {
                    StartCoroutine(CameraShake.Shake(Camera.main.transform, 2f, 5f));
                    shaken = true;
                }
                break;
        }

        if(move)
        {
            mainCam.position = Vector3.Lerp(mainCam.position, transform.position, speed * Time.deltaTime);
            mainCam.rotation = Quaternion.Lerp(mainCam.rotation, transform.rotation, speed * Time.deltaTime);
        }
    }
}
