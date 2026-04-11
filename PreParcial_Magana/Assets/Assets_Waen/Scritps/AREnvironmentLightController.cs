using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(ARCameraManager))]
public class AREnvironmentLightController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private Light directionalLight;
    [SerializeField] private AREnvironmentProbeManager environmentProbeManager;

    [Header("Configuracion")]
    [SerializeField] private bool actualizarLuzDireccional = true;
    [SerializeField] private bool actualizarLuzAmbiente = true;
    [SerializeField] private bool activarReflejosEntorno = true;
    [SerializeField] private bool activarProbesAutomaticos = true;
    [SerializeField] private bool solicitarHDRParaProbes = true;
    [SerializeField] private FilterMode filtroReflexionEntorno = FilterMode.Trilinear;
    [SerializeField] private float intensidadMinima = 0.03f;
    [SerializeField] private float intensidadMaxima = 2.2f;
    [SerializeField] private bool usarTemperaturaColor = true;
    [SerializeField] private float temperaturaMinima = 1800f;
    [SerializeField] private float temperaturaMaxima = 12000f;
    [SerializeField] private float suavizado = 8f;

    private float intensidadObjetivo = 1f;
    private Color colorObjetivo = Color.white;
    private Quaternion rotacionObjetivo = Quaternion.identity;
    private float temperaturaObjetivo = 6500f;
    private Cubemap reflexionNegra;

    private void Reset()
    {
        arCameraManager = GetComponent<ARCameraManager>();

        if (directionalLight == null)
            directionalLight = FindFirstObjectByType<Light>();

        if (environmentProbeManager == null)
        {
            XROrigin xrOrigin = GetComponentInParent<XROrigin>();
            if (xrOrigin != null)
            {
                environmentProbeManager = xrOrigin.GetComponent<AREnvironmentProbeManager>();
            }
        }
    }

    private void Awake()
    {
        if (arCameraManager == null)
            arCameraManager = GetComponent<ARCameraManager>();

        if (arCameraManager != null)
        {
            arCameraManager.requestedLightEstimation =
                LightEstimation.AmbientIntensity |
                LightEstimation.AmbientColor |
                LightEstimation.AmbientSphericalHarmonics |
                LightEstimation.MainLightDirection |
                LightEstimation.MainLightIntensity;
        }

        if (directionalLight != null)
        {
            intensidadObjetivo = directionalLight.intensity;
            colorObjetivo = directionalLight.color;
            rotacionObjetivo = directionalLight.transform.rotation;
            temperaturaObjetivo = directionalLight.colorTemperature;
        }

        ConfigurarEnvironmentProbeManager();
        ActualizarDisponibilidadReflejosEntorno();
        ActualizarReflexionGlobal();
    }

    private void OnEnable()
    {
        if (arCameraManager != null)
        {
            arCameraManager.requestedLightEstimation =
                LightEstimation.AmbientIntensity |
                LightEstimation.AmbientColor |
                LightEstimation.AmbientSphericalHarmonics |
                LightEstimation.MainLightDirection |
                LightEstimation.MainLightIntensity;

            arCameraManager.frameReceived += OnCameraFrameReceived;
        }

        if (environmentProbeManager != null)
        {
            environmentProbeManager.trackablesChanged.AddListener(OnEnvironmentProbesChanged);
            ConfigurarEnvironmentProbeManager();
            ActualizarDisponibilidadReflejosEntorno();
            ActualizarReflexionGlobal();
        }
    }

    private void OnDisable()
    {
        if (arCameraManager != null)
            arCameraManager.frameReceived -= OnCameraFrameReceived;

        if (environmentProbeManager != null)
            environmentProbeManager.trackablesChanged.RemoveListener(OnEnvironmentProbesChanged);

        FurnitureRenderSetup.SetReflejosEntornoDisponibles(false);
        AplicarReflexionGlobal(reflexionNegra, false);
    }

    private void Update()
    {
        if (!actualizarLuzDireccional || directionalLight == null)
            return;

        float t = Time.deltaTime * suavizado;

        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, intensidadObjetivo, t);
        directionalLight.color = Color.Lerp(directionalLight.color, colorObjetivo, t);
        directionalLight.transform.rotation =
            Quaternion.Slerp(directionalLight.transform.rotation, rotacionObjetivo, t);

        if (usarTemperaturaColor)
        {
            directionalLight.useColorTemperature = true;
            directionalLight.colorTemperature =
                Mathf.Lerp(directionalLight.colorTemperature, temperaturaObjetivo, t);
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        var lightEstimation = args.lightEstimation;

        if (actualizarLuzDireccional && directionalLight != null)
        {
            float intensidadEstimacion = 1f;

            if (lightEstimation.averageMainLightBrightness.HasValue)
            {
                intensidadEstimacion = lightEstimation.averageMainLightBrightness.Value;
            }
            else if (lightEstimation.averageBrightness.HasValue)
            {
                intensidadEstimacion = lightEstimation.averageBrightness.Value;
            }

            intensidadObjetivo = Mathf.Clamp(intensidadEstimacion, intensidadMinima, intensidadMaxima);

            if (lightEstimation.mainLightColor.HasValue)
            {
                colorObjetivo = lightEstimation.mainLightColor.Value;
            }
            else if (lightEstimation.colorCorrection.HasValue)
            {
                colorObjetivo = lightEstimation.colorCorrection.Value;
            }

            if (lightEstimation.mainLightDirection.HasValue)
            {
                rotacionObjetivo = Quaternion.LookRotation(-lightEstimation.mainLightDirection.Value);
            }

            if (usarTemperaturaColor && lightEstimation.averageColorTemperature.HasValue)
            {
                temperaturaObjetivo = Mathf.Clamp(
                    lightEstimation.averageColorTemperature.Value,
                    temperaturaMinima,
                    temperaturaMaxima
                );
            }
        }

        if (!actualizarLuzAmbiente)
            return;

        if (lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = lightEstimation.ambientSphericalHarmonics.Value;
            DynamicGI.UpdateEnvironment();
        }
        else if (lightEstimation.colorCorrection.HasValue)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = lightEstimation.colorCorrection.Value;
        }

        if (lightEstimation.averageBrightness.HasValue)
        {
            float intensidad = Mathf.Clamp(
                lightEstimation.averageBrightness.Value,
                intensidadMinima,
                intensidadMaxima
            );

            RenderSettings.ambientIntensity = intensidad;
        }
    }

    private void ConfigurarEnvironmentProbeManager()
    {
        if (!activarReflejosEntorno)
        {
            FurnitureRenderSetup.SetReflejosEntornoDisponibles(false);
            AplicarReflexionGlobal(reflexionNegra, false);
            return;
        }

        if (environmentProbeManager == null)
        {
            XROrigin xrOrigin = GetComponentInParent<XROrigin>();
            if (xrOrigin != null)
            {
                environmentProbeManager = xrOrigin.GetComponent<AREnvironmentProbeManager>();
                if (environmentProbeManager == null)
                {
                    environmentProbeManager = xrOrigin.gameObject.AddComponent<AREnvironmentProbeManager>();
                }
            }
        }

        if (environmentProbeManager == null) return;

        environmentProbeManager.enabled = true;
        environmentProbeManager.automaticPlacementRequested = activarProbesAutomaticos;
        environmentProbeManager.environmentTextureHDRRequested = solicitarHDRParaProbes;
        environmentProbeManager.environmentTextureFilterMode = filtroReflexionEntorno;
    }

    private void OnEnvironmentProbesChanged(ARTrackablesChangedEventArgs<AREnvironmentProbe> _)
    {
        ActualizarDisponibilidadReflejosEntorno();
        ActualizarReflexionGlobal();
    }

    private void ActualizarDisponibilidadReflejosEntorno()
    {
        if (!activarReflejosEntorno || environmentProbeManager == null)
        {
            FurnitureRenderSetup.SetReflejosEntornoDisponibles(false);
            return;
        }

        bool hayProbesActivos = false;
        foreach (AREnvironmentProbe probe in environmentProbeManager.trackables)
        {
            if (probe == null) continue;

            ReflectionProbe reflectionProbe = probe.GetComponent<ReflectionProbe>();
            if (reflectionProbe != null && reflectionProbe.customBakedTexture != null)
            {
                hayProbesActivos = true;
                break;
            }
        }

        FurnitureRenderSetup.SetReflejosEntornoDisponibles(hayProbesActivos);
    }

    private void ActualizarReflexionGlobal()
    {
        if (!activarReflejosEntorno || environmentProbeManager == null)
        {
            AplicarReflexionGlobal(reflexionNegra, false);
            return;
        }

        Texture reflexionEntorno = null;
        foreach (AREnvironmentProbe probe in environmentProbeManager.trackables)
        {
            if (probe == null) continue;

            ReflectionProbe reflectionProbe = probe.GetComponent<ReflectionProbe>();
            if (reflectionProbe == null || reflectionProbe.customBakedTexture == null) continue;

            reflexionEntorno = reflectionProbe.customBakedTexture;
            break;
        }

        AplicarReflexionGlobal(reflexionEntorno != null ? reflexionEntorno : reflexionNegra, reflexionEntorno != null);
    }

    private void AplicarReflexionGlobal(Texture texturaReflexion, bool disponible)
    {
        if (reflexionNegra == null)
            reflexionNegra = CrearCubemapNegro();

        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflectionTexture = texturaReflexion != null ? texturaReflexion : reflexionNegra;
        RenderSettings.reflectionIntensity = disponible ? 1f : 0f;
    }

    private static Cubemap CrearCubemapNegro()
    {
        Cubemap cubemap = new Cubemap(16, TextureFormat.RGBA32, false);
        Color[] colores = new Color[16 * 16];

        for (int i = 0; i < colores.Length; i++)
        {
            colores[i] = Color.black;
        }

        foreach (CubemapFace cara in System.Enum.GetValues(typeof(CubemapFace)))
        {
            if (cara == CubemapFace.Unknown) continue;
            cubemap.SetPixels(colores, cara);
        }

        cubemap.Apply();
        return cubemap;
    }
}
