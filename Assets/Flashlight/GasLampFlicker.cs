using UnityEngine;

// Put on a Point Light to make it flicker gently like a gas lamp.
[RequireComponent(typeof(Light))]
public class GasLampFlicker : MonoBehaviour
{
    [Tooltip("How much the brightness wobbles (0 = steady)")]
    [Range(0f, 1f)] public float flickerAmount = 0.15f;
    [Tooltip("How fast it flickers")]
    public float speed = 3f;

    Light lamp;
    float baseIntensity;
    float seed;

    void Start()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        seed = Random.value * 100f;   // so each lamp flickers differently
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed); // smooth 0..1 noise
        lamp.intensity = baseIntensity * (1f - flickerAmount + n * flickerAmount * 2f);
    }
}
