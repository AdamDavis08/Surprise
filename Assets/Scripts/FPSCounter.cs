using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI displayText;

    private void Start()
    {
        displayText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        displayText.text = "FPS: " + current.ToString();
    }
}
