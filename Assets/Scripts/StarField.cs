using UnityEngine;

public class StarField : MonoBehaviour
{
    public int maxStars = 1000;
    public int universeSize = 10;

    private static float starIntensity = 0.6f;
    public Material particleMaterial;
    public Material cloudsMaterial;

    private ParticleSystem.Particle[] points;

    private ParticleSystem particleSystem;
    private void Create()
    {
        points = new ParticleSystem.Particle[maxStars];

        for (int i = 0; i < maxStars; i++)
        {
            points[i].position = Random.onUnitSphere * universeSize;
            points[i].startSize = Random.Range(5f, 20f);
            points[i].startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }

        particleSystem = gameObject.GetComponent<ParticleSystem>();

        particleSystem.SetParticles(points, points.Length);
    }

    private void Start()
    {
        Create();
    }

    private void Update()
    {
        float sinValue = Mathf.Sin(DayNightCycle.time * Mathf.Deg2Rad);
        DayNightCycle.lightSource.intensity = sinValue;

        transform.position = Player.playerPosition;
        starIntensity = -sinValue;
        starIntensity = Mathf.Clamp(starIntensity, 0f, 0.6f);
        particleMaterial.color = new Color(particleMaterial.color.r, particleMaterial.color.g, particleMaterial.color.b, starIntensity);
        cloudsMaterial.SetFloat("_CloudsAlpha", Mathf.Clamp(sinValue, 0.1f, 1f));
    }
}