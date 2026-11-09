#ifndef DIORAMA_WORLDFROMDEPTH_INCLUDED
#define DIORAMA_WORLDFROMDEPTH_INCLUDED

// Reconstruit la position monde d'un pixel à partir du depth buffer (pour la brume).
// Custom Function node : Type = File, Name = WorldFromDepth
// Entrées : UV (écran, 0-1), Depth (Scene Depth en mode RAW)
// Sortie  : WorldPos (Vector3)
void WorldFromDepth_float(float2 UV, float Depth, out float3 WorldPos)
{
    // La sortie est TOUJOURS initialisée en premier, quel que soit le chemin
    WorldPos = float3(0.0, 0.0, 0.0);

#ifndef SHADERGRAPH_PREVIEW
    WorldPos = ComputeWorldSpacePosition(UV, Depth, UNITY_MATRIX_I_VP);
#endif
}

void WorldFromDepth_half(half2 UV, half Depth, out half3 WorldPos)
{
    WorldPos = half3(0.0, 0.0, 0.0);

    float3 w = float3(0.0, 0.0, 0.0);
    WorldFromDepth_float(float2(UV), float(Depth), w);
    WorldPos = half3(w);
}

#endif
