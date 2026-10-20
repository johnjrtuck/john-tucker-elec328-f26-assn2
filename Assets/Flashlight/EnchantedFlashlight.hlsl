// Custom Function for Shader Graph (Type: File, Name: EnchantedFlashlight)
// Outputs two masks (0..1):
//   Reveal   = 1 where the hidden texture should show (inside the beam, facing the light)
//   Boundary = 1 on the ring at the edge of the beam
#ifndef ENCHANTED_FLASHLIGHT_INCLUDED
#define ENCHANTED_FLASHLIGHT_INCLUDED

void EnchantedFlashlight_float(
    float3 PositionWS, float3 NormalWS,
    float3 Origin, float3 Direction, float ConeAngle,
    float BoundaryWidth, float EdgeFeather,
    out float Reveal, out float Boundary)
{
    // Unit vector from the flashlight to this surface point
    float3 L = normalize(PositionWS - Origin);
    float3 D = normalize(Direction);

    // Inside the cone test: dot(L, D) = cos(angle between beam axis and L).
    // Converting back to an angle lets BoundaryWidth/EdgeFeather be in degrees.
    float cosAngle = clamp(dot(L, D), -1.0, 1.0);
    float angle = degrees(acos(cosAngle));
    float edgeDist = ConeAngle - angle;          // > 0 inside the cone, < 0 outside

    // Objective D: only surfaces facing the flashlight get revealed.
    // Normal points toward the light when dot(N, -L) > 0.
    float facing = step(0.0, dot(normalize(NormalWS), -L));

    float f = max(EdgeFeather, 1e-4);
    float inCone = smoothstep(-f, f, edgeDist);                                   // whole cone
    float inCore = smoothstep(BoundaryWidth - f, BoundaryWidth + f, edgeDist);    // cone minus ring

    Reveal   = inCore * facing;                    // Objective C
    Boundary = saturate(inCone - inCore) * facing; // Objective E
}

// Half-precision overload so the node works at any graph precision
void EnchantedFlashlight_half(
    half3 PositionWS, half3 NormalWS,
    half3 Origin, half3 Direction, half ConeAngle,
    half BoundaryWidth, half EdgeFeather,
    out half Reveal, out half Boundary)
{
    float r, b;
    EnchantedFlashlight_float(PositionWS, NormalWS, Origin, Direction, ConeAngle,
                              BoundaryWidth, EdgeFeather, r, b);
    Reveal = r;
    Boundary = b;
}

#endif
