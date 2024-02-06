using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Transform moon;
    public static float time = 50f;
    public float moonDistance = 100f;
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

        moon.GetChild(0).localPosition = new Vector3(0f, 0f, moonDistance);
        moon.position = ThirdPersonMovement.playerPosition;
        moon.rotation = Quaternion.Euler(time + 180f, 0f, 0f);
        moon.LookAt(ThirdPersonMovement.playerPosition);
    }
}
