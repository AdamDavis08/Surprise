using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static float time = 270f;
    public float timeIncrement = 1f;

    public static Light lightSource;

    private void Start()
    {
        lightSource = GetComponent<Light>();
    }

    private void Update()
    {
        time += timeIncrement * Time.deltaTime;
        transform.rotation = Quaternion.Euler(time, 0f, 0f);
    }
}
