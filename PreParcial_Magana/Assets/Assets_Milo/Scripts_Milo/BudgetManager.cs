using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BudgetManager : MonoBehaviour
{
    public static BudgetManager Instance;

    [Header("Configuración Inicial")]
    public string nombreUsuario;
    public float presupuestoInicial;
    public float presupuestoRestante;

    [Header("Estado Actual")]
    private List<GameObject> mueblesEnEscena = new List<GameObject>();

    [Header("UI Historial")]
    [SerializeField] private GameObject prefabTarjeta;
    [SerializeField] private Transform contenedorMatriz;
    [SerializeField] private GameObject panelHistorial;

    // Lista de versiones (Opciones de diseño)
    public List<VersionDiseño> historialVersiones = new List<VersionDiseño>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Inicializa la sesión (llamar desde el panel de bienvenida)
    public void IniciarSesion(string nombre, float presupuesto)
    {
        nombreUsuario = nombre;
        presupuestoInicial = presupuesto;
        presupuestoRestante = presupuesto;
    }

    public bool PuedeComprar(float precio)
    {
        return presupuestoRestante >= precio;
    }

    public void RegistrarCompra(GameObject mueble, float precio)
    {
        mueblesEnEscena.Add(mueble);
        presupuestoRestante -= precio;
        Debug.Log($"Compra exitosa. Quedan: ${presupuestoRestante}");
    }

    private bool estaGuardando = false; // Bandera para evitar el doble clic y duplicados

    public void GuardarVersion()
    {
        // Si ya se está procesando una captura, no permitas otra
        if (estaGuardando) return;

        StartCoroutine(ProcesoGuardarVersion());
    }

    private IEnumerator ProcesoGuardarVersion()
    {
        estaGuardando = true;

        // Esperamos al final del frame para que la captura no salga con menús parpadeando
        yield return new WaitForEndOfFrame();

        // 1. Captura de pantalla
        Texture2D captura = ScreenCapture.CaptureScreenshotAsTexture();

        // 2. Crear Datos (POO)
        VersionDiseño nuevaVersion = new VersionDiseño
        {
            numeroOpcion = historialVersiones.Count + 1,
            // Calculamos cuánto se gastó realmente
            costoTotal = presupuestoInicial - presupuestoRestante,
            foto = captura
        };

        historialVersiones.Add(nuevaVersion);

        // 3. Crear la Tarjeta en la UI
        if (prefabTarjeta != null && contenedorMatriz != null)
        {
            GameObject nuevaCard = Instantiate(prefabTarjeta, contenedorMatriz);

            // Verificamos que el componente exista para evitar el NullReference
            CotizacionCard scriptCard = nuevaCard.GetComponent<CotizacionCard>();
            if (scriptCard != null)
            {
                scriptCard.Configurar(nuevaVersion);
            }
            else
            {
                Debug.LogError("¡El prefab de la tarjeta no tiene el script CotizacionCard!");
            }
        }

        // 4. Mostrar el panel de historial
        if (panelHistorial != null)
        {
            panelHistorial.SetActive(true);
        }

        estaGuardando = false; // Liberamos el bloqueo
    }

    public void IniciarNuevaCotizacion()
    {
        // 1. Buscamos todos los objetos en la escena
        GameObject[] todosLosObjetos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Obtenemos el número de la capa por su nombre
        int layerMuebles = LayerMask.NameToLayer("capaMuebles");

        foreach (GameObject m in todosLosObjetos)
        {
            // Si el objeto pertenece a la capa "capaMuebles", lo borramos
            if (m.layer == layerMuebles)
            {
                Destroy(m);
            }
        }

        // 2. Resetear el presupuesto al valor inicial
        presupuestoRestante = presupuestoInicial;

        // 3. Limpiar la lista interna
        mueblesEnEscena.Clear();

        // 4. Cerramos panel
        panelHistorial.SetActive(false);

        Debug.Log("Escena limpia usando capas. ¡Nueva cotización iniciada!");
    }



}

[System.Serializable]
public class VersionDiseño
{
    public int numeroOpcion;
    public float costoTotal;
    public List<string> mueblesUsados;
    public Texture2D foto;
}