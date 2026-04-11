using UnityEngine;
using System.Collections;

public class WorldSpaceUIController : MonoBehaviour
{
    [Header("Referencia a la cámara")]
    [SerializeField] private Camera arCamera;

    [Header("Configuración de posición")]
    [Tooltip("Distancia extra desde el borde del mueble")]
    [SerializeField] private float margenExtra = 0.1f;
    [SerializeField] private float alturaOffset = 0.5f;

    [Header("Animación")]
    [SerializeField] private float duracionAnimacion = 0.25f;

    private Transform objetoSeguido;
    private Collider colliderObjetivo;
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

        Vector3 centroBase = objetoSeguido.position;
        float radioObjeto = 0.3f;

        if (colliderObjetivo != null)
        {
            centroBase = colliderObjetivo.bounds.center;

            radioObjeto = Mathf.Max(colliderObjetivo.bounds.extents.x, colliderObjetivo.bounds.extents.z);
        }

        Vector3 direccionIzquierda = -arCamera.transform.right;
        Vector3 posicionObjetivo = centroBase
            + (direccionIzquierda * (radioObjeto + margenExtra))
            + (Vector3.up * alturaOffset);

        transform.position = posicionObjetivo;

        Vector3 direccionACamara = arCamera.transform.position - transform.position;
        direccionACamara.y = 0;
        transform.rotation = Quaternion.LookRotation(-direccionACamara);
    }

    public void Mostrar(Transform objetivo)
    {
        objetoSeguido = objetivo;
        colliderObjetivo = objetivo.GetComponentInChildren<Collider>();
        activo = true;
        gameObject.SetActive(true);
        GetComponent<AdditionalSettingsAnimator>()?.AnimarEntrada();
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