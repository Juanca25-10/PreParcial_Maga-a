using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TiendaAnimator — Anima la interfaz de la tienda al estilo de una web moderna.
/// El panel blanco (Tienda) permanece estático; solo se animan botones y textos.
/// Adjunta este script a cualquier GameObject activo en la escena (ej: Canvas o AR Session).
/// Asigna las referencias en el Inspector.
/// </summary>
public class TiendaAnimator : MonoBehaviour
{
    [Header("— Botones de categoría (Content del ScrollView) —")]
    [Tooltip("Arrastra aquí: BtMueble, BtSalaCompleta, BtLampara, BtEscritorio, BtSilla, BtComedor")]
    public List<RectTransform> botonesCategoría;

    [Header("— Textos del Panel —")]
    [Tooltip("Arrastra aquí todos los Text (TMP) del Panel en el orden que quieras que aparezcan")]
    public List<RectTransform> textosPanel;

    [Header("— Configuración de animación —")]
    [Tooltip("Tiempo entre la aparición de cada botón")]
    public float delayEntreBotones = 0.08f;

    [Tooltip("Tiempo entre la aparición de cada texto")]
    public float delayEntreTextos = 0.04f;

    [Tooltip("Duración de cada fade-in")]
    public float duracionFade = 0.35f;

    [Tooltip("Distancia desde la que entra cada elemento (en píxeles)")]
    public float offsetSlide = 40f;

    [Tooltip("Tiempo de espera antes de arrancar la animación completa")]
    public float delayInicial = 0.2f;

    // ─────────────────────────────────────────────────────────────────────────────
    // Curvas de easing (imitan cubic-bezier de CSS)
    // ─────────────────────────────────────────────────────────────────────────────
    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);

    // ─────────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Esconder todo antes de animar
        OcultarTodo();
        StartCoroutine(SecuenciaEntrada());
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // OCULTAR
    // ─────────────────────────────────────────────────────────────────────────────

    void OcultarTodo()
    {
        // Botones: fuera de posición + transparentes
        foreach (var btn in botonesCategoría)
        {
            if (btn == null) continue;
            SetAlpha(btn, 0f);
            btn.anchoredPosition += new Vector2(-offsetSlide * 2f, 0f); // entran desde la izquierda
        }

        // Textos: fuera de posición + transparentes
        foreach (var txt in textosPanel)
        {
            if (txt == null) continue;
            SetAlpha(txt, 0f);
            txt.anchoredPosition += new Vector2(0f, -offsetSlide); // entran desde abajo
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SECUENCIA PRINCIPAL
    // ─────────────────────────────────────────────────────────────────────────────

    IEnumerator SecuenciaEntrada()
    {
        yield return new WaitForSeconds(delayInicial);

        // Paso 1. Textos del panel — slide desde abajo, muy escalonados (efecto cascada)
        for (int i = 0; i < textosPanel.Count; i++)
        {
            if (textosPanel[i] == null) continue;

            // Los primeros textos (título, filtros) entran más rápido
            float velocidad = (i < 3) ? duracionFade + 0.1f : duracionFade;

            StartCoroutine(AnimarElemento(
                textosPanel[i],
                new Vector2(0f, -offsetSlide),         // offset de origen
                velocidad,
                EaseOutCubic
            ));
            yield return new WaitForSeconds(delayEntreTextos);
        }

        yield return new WaitForSeconds(0.1f);

        // Paso 2. Botones de categoría — slide desde la izquierda, escalonados
        for (int i = 0; i < botonesCategoría.Count; i++)
        {
            if (botonesCategoría[i] == null) continue;
            StartCoroutine(AnimarElemento(
                botonesCategoría[i],
                new Vector2(-offsetSlide * 2f, 0f),   // offset de origen
                duracionFade + 0.05f,
                EaseOutQuart
            ));
            yield return new WaitForSeconds(delayEntreBotones);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // COROUTINES AUXILIARES
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mueve un RectTransform desde (posición actual + offsetOrigen) hasta su posición real,
    /// y hace fade de 0 a 1, con la curva de easing indicada.
    /// </summary>
    IEnumerator AnimarElemento(RectTransform rect, Vector2 offsetOrigen, float duracion, System.Func<float, float> easing)
    {
        Vector2 posDestino = rect.anchoredPosition;           // posición "real" en el layout
        Vector2 posInicio = posDestino + offsetOrigen;       // ya sumamos el offset en OcultarTodo, así que reajustamos

        // Reajuste: OcultarTodo ya desplazó el rect, así que posInicio ES la posición actual
        posInicio = rect.anchoredPosition;
        posDestino = posInicio - offsetOrigen;                // destino = quitar el offset

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float te = easing(t);

            rect.anchoredPosition = Vector2.LerpUnclamped(posInicio, posDestino, te);
            SetAlpha(rect, te);

            yield return null;
        }

        rect.anchoredPosition = posDestino;
        SetAlpha(rect, 1f);
    }

    /// <summary>
    /// Hace fade de alpha en un CanvasGroup.
    /// </summary>
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, EaseOutCubic(Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }
        cg.alpha = hasta;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HELPERS: alpha
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Pone el alpha en todos los Graphic y CanvasGroup hijos del RectTransform.
    /// Funciona con Image, TextMeshProUGUI, etc.
    /// </summary>
    void SetAlpha(RectTransform rt, float alpha)
    {
        // CanvasGroup (lo más limpio, no toca child CanvasGroups separados)
        CanvasGroup cg = rt.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
            return;
        }

        // Si no tiene CanvasGroup, agrega uno temporalmente para no tocar colores
        cg = rt.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // MÉTODO PÚBLICO: reproducir animación desde código externo
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Llama a este método para repetir la animación de entrada (útil en AR al reabrir la tienda).
    /// </summary>
    public void ReproducirAnimacion()
    {
        StopAllCoroutines();
        OcultarTodo();
        StartCoroutine(SecuenciaEntrada());
    }
}