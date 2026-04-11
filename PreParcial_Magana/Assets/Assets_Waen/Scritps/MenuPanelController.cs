using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MenuPanelController
/// ─────────────────────────────────────────────────────────────────────────────
/// Adjunta este script al Canvas principal (el que tiene el MenuPanel de la tienda).
/// 
/// FLUJO:
///   • Al seleccionar un mueble → MenuPanel hace fade out + aparece BtnVolver
///   • Al presionar BtnVolver  → MenuPanel hace fade in + se oculta BtnVolver
///                              + se deselecciona el mueble
/// </summary>
public class MenuPanelController : MonoBehaviour
{
    [Header("— Panel de la tienda —")]
    [Tooltip("Arrastra el GameObject 'MenuPanel'")]
    public CanvasGroup menuPanelGroup;

    [Header("— Botón de regreso —")]
    [Tooltip("Arrastra el botón pequeño de volver (empieza desactivado)")]
    public RectTransform btnVolver;

    [Header("— Referencia para deseleccionar —")]
    [Tooltip("Arrastra el GameObject que tiene FurnitureInteraction")]
    public FurnitureInteraction furnitureInteraction;

    [Header("— Configuración —")]
    public float duracionFade = 0.4f;
    public float alphaOculto = 0f;
    [Tooltip("Alpha del panel cuando está en segundo plano (0 = invisible, 0.15 = sutil)")]
    public float alphaMinimo = 0f;

    // ─────────────────────────────────────────────────────────────────────────
    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;

    void Awake()
    {
        // El botón de volver empieza oculto
        if (btnVolver != null)
        {
            var cg = EnsureCG(btnVolver);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        // El menú empieza visible
        if (menuPanelGroup != null)
            menuPanelGroup.alpha = 1f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LLAMADO DESDE FurnitureInteraction al seleccionar un mueble
    // ─────────────────────────────────────────────────────────────────────────

   public void AlSeleccionarMueble()
    {
        StopAllCoroutines();
        StartCoroutine(FadePanel(menuPanelGroup, 1f, alphaMinimo, duracionFade, EaseInCubic, bloquear: true));
       if (btnVolver != null)
            StartCoroutine(FadeBoton(btnVolver, 0f, 1f, duracionFade));
    }

   

    // ─────────────────────────────────────────────────────────────────────────
    // LLAMADO DESDE FurnitureInteraction al deseleccionar
    // ─────────────────────────────────────────────────────────────────────────

    public void AlDeseleccionar()
    {
        StopAllCoroutines();
        StartCoroutine(FadePanel(menuPanelGroup, alphaMinimo, 1f, duracionFade, EaseOutCubic, bloquear: false));
        if (btnVolver != null)
            StartCoroutine(FadeBoton(btnVolver, 1f, 0f, duracionFade * 0.6f));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BOTÓN VOLVER — conecta este método al onClick del BtnVolver en Unity
    // ─────────────────────────────────────────────────────────────────────────

    public void PresionarVolver()
    {
        if (furnitureInteraction != null)
            furnitureInteraction.Deseleccionar();
        // AlDeseleccionar() se llamará desde Deseleccionar() en FurnitureInteraction
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COROUTINES
    // ─────────────────────────────────────────────────────────────────────────

    IEnumerator FadePanel(CanvasGroup cg, float desde, float hasta,
                          float duracion, System.Func<float, float> easing, bool bloquear)
    {
        float tiempo = 0f;
        cg.alpha = desde;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, easing(Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }

        cg.alpha = hasta;
        cg.interactable = !bloquear;
        cg.blocksRaycasts = !bloquear;
    }

    IEnumerator FadeBoton(RectTransform rt, float desde, float hasta, float duracion)
    {
        var cg = EnsureCG(rt);
        float tiempo = 0f;
        cg.alpha = desde;

        // Al inicio del fade in, habilitar inmediatamente para que sea tappable
        if (hasta > desde)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, EaseOutCubic(Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }

        cg.alpha = hasta;

        if (hasta <= 0f)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    CanvasGroup EnsureCG(RectTransform rt)
    {
        var cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}
