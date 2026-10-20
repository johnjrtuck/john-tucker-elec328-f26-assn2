using UnityEngine;

// Turns a gas lamp on/off: the Point Light AND the glowing lamp head.
// Hook Toggle() to XR Simple Interactable > Interactable Events > Select Entered.
public class LampToggle : MonoBehaviour
{
    [Tooltip("The lamp's Point Light")]
    public Light lampLight;

    [Tooltip("The glowing lamp head (its material's emission gets turned on/off)")]
    public Renderer glowRenderer;

    [Tooltip("Emission color when the lamp is on")]
    [ColorUsage(false, true)] public Color onEmission = new Color(2f, 1.2f, 0.47f);

    public bool isOn = true;

    [Tooltip("Optional: other lamps that switch together with this one")]
    public LampToggle[] alsoToggle;

    void Start() => Apply();

    public void Toggle()
    {
        isOn = !isOn;
        Apply();
        foreach (var other in alsoToggle)
        {
            if (other == null || other == this) continue;
            other.isOn = isOn;
            other.Apply();
        }
    }

    void Apply()
    {
        if (lampLight != null) lampLight.enabled = isOn;

        if (glowRenderer != null)
        {
            Material m = glowRenderer.material; // per-lamp copy, so other lamps aren't affected
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", isOn ? onEmission : Color.black);
        }
    }
}
