using UnityEngine;

// Objective A: sends the flashlight's world-space origin, direction and cone angle
// to every shader each frame as GLOBAL shader properties.
public class FlashlightController : MonoBehaviour
{
    [Tooltip("Point at the lens. Its forward (blue Z axis) is the beam direction. Leave empty to use this object.")]
    public Transform beamOrigin;

    [Tooltip("Half-angle of the cone in degrees (center line to edge).")]
    [Range(1f, 89f)] public float coneAngle = 20f;

    [Tooltip("Optional real Spot Light so the beam also lights the scene.")]
    public Light spotLight;

    public bool flashlightOn = true;

    static readonly int OriginID    = Shader.PropertyToID("_Flashlight_Origin");
    static readonly int DirectionID = Shader.PropertyToID("_Flashlight_Direction");
    static readonly int ConeAngleID = Shader.PropertyToID("_Flashlight_ConeAngle");

    // LateUpdate so we read the pose after XR tracking/grab has moved the object this frame.
    void LateUpdate()
    {
        Transform t = beamOrigin != null ? beamOrigin : transform;

        Shader.SetGlobalVector(OriginID, t.position);
        Shader.SetGlobalVector(DirectionID, t.forward);
        // Angle 0 = nothing is inside the cone, so "off" just reveals nothing.
        Shader.SetGlobalFloat(ConeAngleID, flashlightOn ? coneAngle : 0f);

        if (spotLight != null)
        {
            spotLight.enabled = flashlightOn;
            spotLight.spotAngle = coneAngle * 2f; // Unity's spot angle is the FULL angle
        }
    }

    public void Toggle() => flashlightOn = !flashlightOn;
}
