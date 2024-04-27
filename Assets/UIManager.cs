using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static GameObject crosshair;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            switch (child.gameObject.name)
            {
                case "Crosshair":
                    crosshair = child.gameObject;
                    break;
            }
        }
    }
    
    public static void SetCrosshairVisibility(bool value)
    {
        crosshair.SetActive(value);
    }
}
