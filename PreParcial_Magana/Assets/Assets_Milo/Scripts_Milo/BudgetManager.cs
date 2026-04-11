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

    public void GuardarVersion()
    {
        StartCoroutine(ProcesoGuardarVersion());
    }

    private IEnumerator ProcesoGuardarVersion()
    {
        yield return new WaitForEndOfFrame();

        // 1. Captura
        Texture2D captura = ScreenCapture.CaptureScreenshotAsTexture();

        // 2. Crear Datos (POO)
        VersionDiseño nuevaVersion = new VersionDiseño
        {
            numeroOpcion = historialVersiones.Count + 1,
            costoTotal = presupuestoInicial - presupuestoRestante,
            foto = captura
        };

        historialVersiones.Add(nuevaVersion);

        // 3. Crear la Tarjeta en la UI
        GameObject nuevaCard = Instantiate(prefabTarjeta, contenedorMatriz);
        nuevaCard.GetComponent<CotizacionCard>().Configurar(nuevaVersion);

        // 4. Mostrar el panel de historial
        panelHistorial.SetActive(true);
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