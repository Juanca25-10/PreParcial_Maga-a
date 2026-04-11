using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AdditionalSettingsAnimator
/// ─────────────────────────────────────────────────────────────────────────────
/// Adjunta este script al GameObject raíz del Canvas (CanvasWorldUI).
/// 
/// FLUJO:
///   1. Al aparecer el canvas → textos y botones animan su entrada (igual que TiendaAnimator)
///   2. Al pulsar un botón    → botones suben y desaparecen, sub-panel baja y aparece
///   3. Al volver atrás       → sub-panel sube y desaparece, botones bajan y aparecen
/// 
/// JERARQUÍA ESPERADA:
///   CanvasWorldUI
///   └── PanelPrincipal
///       ├── [textos: título, subtítulo, footer…]
///       ├── BtnMover
///       ├── BtnRotar
///       ├── BtnColor
///       ├── SubPanelMover
///       ├── SubPanelRotar
///       └── SubPanelColor
/// </summary>
public class AdditionalSettingsAnimator : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────────────────────────────────────

    [Header("— Botones principales —")]
    [Tooltip("Arrastra los 3 botones: Mover, Rotar, Color")]
    public List<RectTransform> botonesPrincipales;

    [Header("— Sub-paneles (mismo orden que los botones) —")]
    [Tooltip("Arrastra: SubPanelMover, SubPanelRotar, SubPanelColor")]
    public List<RectTransform> subPaneles;

    [Header("— Textos que se animan al entrar —")]
    [Tooltip("Título, subtítulo, footer, etc. en el orden que quieres que aparezcan")]
    public List<RectTransform> textosAnimados;

    [Header("— Configuración de animación —")]
    public float delayInicial = 0.15f;
    public float duracionFade = 0.45f;
    public float delayEntreTextos = 0.05f;
    public float delayEntreBotones = 0.08f;
    public float duracionTransicion = 0.40f;

    [Tooltip("Píxeles que se desplazan los botones al salir (hacia arriba)")]
    public float offsetSlideBotones = 60f;

    [Tooltip("Píxeles desde donde entra el sub-panel (desde abajo)")]
    public float offsetSlidePanel = 80f;

    [Tooltip("Píxeles desde donde entran los textos (desde abajo)")]
    public float offsetSlideTextos = 30f;

    // ─────────────────────────────────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────────────────

    private int panelActivo = -1;   // -1 = ninguno abierto
    private bool enTransicion = false;

    // Posiciones originales (se guardan al inicio para poder volver)
    private Vector2[] posOriginalBotones;
    private Vector2[] posOriginalSubPaneles;

    // ─────────────────────────────────────────────────────────────────────────
    // EASING
    // ─────────────────────────────────────────────────────────────────────────

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private static float EaseInCubic(float t) => t * t * t;

    // ─────────────────────────────────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Guardar posiciones originales antes de tocar nada
        posOriginalBotones = new Vector2[botonesPrincipales.Count];
        posOriginalSubPaneles = new Vector2[subPaneles.Count];

        for (int i = 0; i < botonesPrincipales.Count; i++)
            if (botonesPrincipales[i] != null)
                posOriginalBotones[i] = botonesPrincipales[i].anchoredPosition;

        for (int i = 0; i < subPaneles.Count; i++)
            if (subPaneles[i] != null)
            {
                posOriginalSubPaneles[i] = subPaneles[i].anchoredPosition;
                // Sub-paneles empiezan ocultos
                OcultarSubPanel(i);
            }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUNTO DE ENTRADA — llamado por WorldSpaceUIController
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Llama este método desde WorldSpaceUIController.Mostrar()
    /// después de activar el GameObject del canvas.
    /// </summary>
    public void AnimarEntrada()
    {
        StopAllCoroutines();
        panelActivo = -1;
        enTransicion = false;

        // Resetear todo a estado inicial oculto
        OcultarElementosParaEntrada();

        StartCoroutine(SecuenciaEntrada());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OCULTAR PARA ANIMACIÓN DE ENTRADA
    // ─────────────────────────────────────────────────────────────────────────

    void OcultarElementosParaEntrada()
    {
        foreach (var txt in textosAnimados)
        {
            if (txt == null) continue;
            EnsureCanvasGroup(txt).alpha = 0f;
            txt.anchoredPosition += new Vector2(0f, -offsetSlideTextos);
        }

        foreach (var btn in botonesPrincipales)
        {
            if (btn == null) continue;
            EnsureCanvasGroup(btn).alpha = 0f;
            btn.anchoredPosition += new Vector2(0f, -offsetSlideTextos);
        }

        for (int i = 0; i < subPaneles.Count; i++)
            OcultarSubPanel(i);
    }

    void OcultarSubPanel(int i)
    {
        if (i < 0 || i >= subPaneles.Count || subPaneles[i] == null) return;
        var cg = EnsureCanvasGroup(subPaneles[i]);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        subPaneles[i].anchoredPosition = posOriginalSubPaneles[i] + new Vector2(0f, -offsetSlidePanel);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECUENCIA DE ENTRADA (textos → botones)
    // ─────────────────────────────────────────────────────────────────────────

    IEnumerator SecuenciaEntrada()
    {
        yield return new WaitForSeconds(delayInicial);

        // 1. Textos en cascada (suben desde abajo)
        foreach (var txt in textosAnimados)
        {
            if (txt == null) continue;
            StartCoroutine(AnimarEntradaElemento(txt, offsetSlideTextos, duracionFade, EaseOutCubic));
            yield return new WaitForSeconds(delayEntreTextos);
        }

        yield return new WaitForSeconds(0.05f);

        // 2. Botones principales en cascada
        foreach (var btn in botonesPrincipales)
        {
            if (btn == null) continue;
            StartCoroutine(AnimarEntradaElemento(btn, offsetSlideTextos, duracionFade + 0.05f, EaseOutQuart));
            yield return new WaitForSeconds(delayEntreBotones);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ACCIÓN DE BOTÓN — llama desde los botones en el Inspector (onClick)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Conecta este método al onClick de cada botón.
    /// índice: 0 = Mover, 1 = Rotar, 2 = Color
    /// </summary>
    public void AlPresionarBoton(int indice)
    {
        if (enTransicion) return;

        if (panelActivo == indice)
        {
            // Ya está abierto → volver a la vista de botones
            StartCoroutine(CerrarSubPanel());
        }
        else
        {
            // Si hay otro abierto, cerrarlo primero y luego abrir el nuevo
            if (panelActivo != -1)
                StartCoroutine(CambiarSubPanel(indice));
            else
                StartCoroutine(AbrirSubPanel(indice));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TRANSICIONES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sub-panel aparece desde abajo. Botones no se mueven.
    /// </summary>
    IEnumerator AbrirSubPanel(int indice)
    {
        enTransicion = true;

        var panel = subPaneles[indice];
        var cg = EnsureCanvasGroup(panel);
        cg.interactable = true;
        cg.blocksRaycasts = true;

        Vector2 desde = posOriginalSubPaneles[indice] + new Vector2(0f, -offsetSlidePanel);
        Vector2 hasta = posOriginalSubPaneles[indice];
        panel.anchoredPosition = desde;

        StartCoroutine(DeslizarFade(panel, desde, hasta, 0f, 1f, duracionTransicion, EaseOutQuart));

        yield return new WaitForSeconds(duracionTransicion);

        panelActivo = indice;
        enTransicion = false;
    }

    /// <summary>
    /// Sub-panel se oculta. Botones no se mueven.
    /// </summary>
    IEnumerator CerrarSubPanel()
    {
        enTransicion = true;
        int indiceCierre = panelActivo;
        panelActivo = -1;

        var panel = subPaneles[indiceCierre];

        StartCoroutine(DeslizarFade(
            panel,
            panel.anchoredPosition,
            panel.anchoredPosition + new Vector2(0f, -offsetSlidePanel),
            1f, 0f,
            duracionTransicion * 0.5f,
            EaseInCubic
        ));

        yield return new WaitForSeconds(duracionTransicion * 0.5f);

        var cg = EnsureCanvasGroup(panel);
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panel.anchoredPosition = posOriginalSubPaneles[indiceCierre] + new Vector2(0f, -offsetSlidePanel);

        enTransicion = false;
    }

    /// <summary>
    /// Swap directo entre sub-paneles. Botones no se mueven.
    /// </summary>
    IEnumerator CambiarSubPanel(int nuevoIndice)
    {
        enTransicion = true;
        int indiceAnterior = panelActivo;
        panelActivo = -1;

        // Panel viejo sale hacia abajo
        var panelViejo = subPaneles[indiceAnterior];
        StartCoroutine(DeslizarFade(
            panelViejo,
            panelViejo.anchoredPosition,
            panelViejo.anchoredPosition + new Vector2(0f, -offsetSlidePanel),
            1f, 0f,
            duracionTransicion * 0.4f,
            EaseInCubic
        ));

        yield return new WaitForSeconds(duracionTransicion * 0.3f);

        var cgViejo = EnsureCanvasGroup(panelViejo);
        cgViejo.interactable = false;
        cgViejo.blocksRaycasts = false;
        panelViejo.anchoredPosition = posOriginalSubPaneles[indiceAnterior] + new Vector2(0f, -offsetSlidePanel);

        // Panel nuevo entra desde abajo
        var panelNuevo = subPaneles[nuevoIndice];
        var cgNuevo = EnsureCanvasGroup(panelNuevo);
        cgNuevo.interactable = true;
        cgNuevo.blocksRaycasts = true;

        Vector2 desde = posOriginalSubPaneles[nuevoIndice] + new Vector2(0f, -offsetSlidePanel);
        Vector2 hasta = posOriginalSubPaneles[nuevoIndice];
        panelNuevo.anchoredPosition = desde;

        StartCoroutine(DeslizarFade(panelNuevo, desde, hasta, 0f, 1f, duracionTransicion * 0.55f, EaseOutQuart));

        yield return new WaitForSeconds(duracionTransicion * 0.55f);

        panelActivo = nuevoIndice;
        enTransicion = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COROUTINES BASE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Anima la entrada de un elemento desde abajo (para la secuencia inicial).
    /// </summary>
    IEnumerator AnimarEntradaElemento(RectTransform rt, float offset, float duracion, System.Func<float, float> easing)
    {
        Vector2 posInicio = rt.anchoredPosition;
        Vector2 posDestino = posInicio + new Vector2(0f, offset); // quitar el offset que se añadió en OcultarElementosParaEntrada
        var cg = EnsureCanvasGroup(rt);

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float te = easing(Mathf.Clamp01(tiempo / duracion));
            rt.anchoredPosition = Vector2.LerpUnclamped(posInicio, posDestino, te);
            cg.alpha = te;
            yield return null;
        }

        rt.anchoredPosition = posDestino;
        cg.alpha = 1f;
    }

    /// <summary>
    /// Mueve y hace fade de un RectTransform entre dos posiciones y dos alphas.
    /// </summary>
    IEnumerator DeslizarFade(RectTransform rt, Vector2 posDesde, Vector2 posHasta,
                              float alphaDesde, float alphaHasta,
                              float duracion, System.Func<float, float> easing)
    {
        var cg = EnsureCanvasGroup(rt);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float te = easing(Mathf.Clamp01(tiempo / duracion));
            rt.anchoredPosition = Vector2.LerpUnclamped(posDesde, posHasta, te);
            cg.alpha = Mathf.Lerp(alphaDesde, alphaHasta, te);
            yield return null;
        }

        rt.anchoredPosition = posHasta;
        cg.alpha = alphaHasta;
    }

    /// <summary>Sobrecarga sin posHasta explícito (solo fade).</summary>
    IEnumerator DeslizarFade(RectTransform rt, Vector2 posDesde, Vector2 posHasta,
                              float alphaDesde, float alphaHasta,
                              System.Func<float, float> easing)
    {
        yield return DeslizarFade(rt, posDesde, posHasta, alphaDesde, alphaHasta, duracionTransicion, easing);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────────

    CanvasGroup EnsureCanvasGroup(RectTransform rt)
    {
        var cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}