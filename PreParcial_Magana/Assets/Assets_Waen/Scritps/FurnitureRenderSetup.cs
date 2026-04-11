using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class FurnitureRenderSetup
{
    private static readonly HashSet<Renderer> renderersRegistrados = new HashSet<Renderer>();
    private static bool reflejosEntornoDisponibles;

    public static void Configurar(GameObject mueble)
    {
        if (mueble == null) return;

        Renderer[] renderers = mueble.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderer.allowOcclusionWhenDynamic = true;
            _ = renderer.materials;

            renderersRegistrados.Add(renderer);
            AplicarModoReflexion(renderer);
        }
    }

    public static void SetReflejosEntornoDisponibles(bool disponibles)
    {
        if (reflejosEntornoDisponibles == disponibles && renderersRegistrados.Count > 0) return;

        reflejosEntornoDisponibles = disponibles;

        List<Renderer> renderersInvalidos = null;
        foreach (Renderer renderer in renderersRegistrados)
        {
            if (renderer == null)
            {
                renderersInvalidos ??= new List<Renderer>();
                renderersInvalidos.Add(renderer);
                continue;
            }

            AplicarModoReflexion(renderer);
        }

        if (renderersInvalidos == null) return;

        foreach (Renderer renderer in renderersInvalidos)
        {
            renderersRegistrados.Remove(renderer);
        }
    }

    private static void AplicarModoReflexion(Renderer renderer)
    {
        renderer.reflectionProbeUsage = reflejosEntornoDisponibles
            ? ReflectionProbeUsage.BlendProbes
            : ReflectionProbeUsage.Off;

        foreach (Material material in renderer.materials)
        {
            if (material == null) continue;

            if (material.HasProperty("_EnvironmentReflections"))
            {
                material.SetFloat("_EnvironmentReflections", reflejosEntornoDisponibles ? 1f : 0f);
            }
        }
    }
}
