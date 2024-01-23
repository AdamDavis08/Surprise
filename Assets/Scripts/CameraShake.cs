using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static IEnumerator Shake(Transform camTransform, float duration, float magnitude)
    {
        GameObject anchor = new GameObject("Anchor");
        anchor.transform.position = camTransform.position;
        camTransform.parent = anchor.transform;

        Vector3 originalPos = camTransform.localPosition;

        float elapsed = 0.0f;

        while(elapsed < duration)
        {
            float x  = Random.Range(-1f, 1f) * magnitude;
            float y  = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
        camTransform.parent = null;
        Destroy(anchor);
    }
}
