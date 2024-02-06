using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterEffect : MonoBehaviour
{
    public float depth = -4f;
    public GameObject underwaterPR;
    public Material underwaterMaterial;

    private void Awake()
    {
        if (underwaterPR == null || underwaterMaterial == null) this.enabled = false;
    }

    private void Update()
    {
        bool underwater = transform.position.y <= depth;
        underwaterPR.SetActive(underwater);
        RenderSettings.fog = underwater;
    }
}
