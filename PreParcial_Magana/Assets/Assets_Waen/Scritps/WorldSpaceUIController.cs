using UnityEngine;
using System.Collections;

public class WorldSpaceUIController : MonoBehaviour
{
    [Header("Referencia a la camara")]
    [SerializeField] private Camera arCamera;

    [Header("Configuracion de posicion")]
    [SerializeField] private float distanciaAlObjeto = 0.6f;
    [SerializeField] private float alturaOffset = 0.5f;

    [Header("Animacion")]
    [SerializeField] private float duracionAnimacion = 0.25f;

    private Transform objetoSeguido;
    private bool activo = false;
    private Vector3 escalaOriginal;

    void Awake()
    {
        escalaOriginal = transform.localScale;
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!activo || objetoSeguido == null) return;

        // Posicion: izquierda del objeto sin importar su rotacion
        Vector3 direccionIzquierda = -arCamera.transform.right;
        Vector3 posicionObjetivo = objetoSeguido.position
            + direccionIzquierda * distanciaAlObjeto
            + Vector3.up * alturaOffset;

        transform.position = posicionObjetivo;

        // Siempre mira a la camara pero sin inclinarse
        Vector3 direccionACamara = arCamera.transform.position - transform.position;
        direccionACamara.y = 0;
        transform.rotation = Quaternion.LookRotation(-direccionACamara);
    }

    public void Mostrar(Transform objetivo)
    {
        objetoSeguido = objetivo;
        activo = true;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimarEntrada());
    }

    public void Ocultar()
    {
        activo = false;
        StopAllCoroutines();
        StartCoroutine(AnimarSalida());
    }

    private IEnumerator AnimarEntrada()
    {
        transform.localScale = Vector3.zero;
        float tiempo = 0f;

        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionAnimacion;
            // Curva elastica para el pop
            float escala = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
            transform.localScale = escalaOriginal * Mathf.LerpUnclamped(0f, escala, t);
            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    private IEnumerator AnimarSalida()
    {
        float tiempo = 0f;
        Vector3 escalaInicial = transform.localScale;

        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionAnimacion;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
        objetoSeguido = null;
    }
}
