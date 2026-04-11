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

        // 1. Tomar captura
        Texture2D captura = ScreenCapture.CaptureScreenshotAsTexture();

        // 2. Crear la versión (POO)
        VersionDiseño nuevaVersion = new VersionDiseño
        {
            numeroOpcion = historialVersiones.Count + 1,
            costoTotal = presupuestoInicial - presupuestoRestante,
            foto = captura,
            mueblesUsados = new List<string>()
        };

        // 3. Listar qué muebles se usaron
        foreach (GameObject g in mueblesEnEscena)
        {
            var data = g.GetComponent<FurnitureData>();
            if (data) nuevaVersion.mueblesUsados.Add(data.nombreMueble);
        }

        historialVersiones.Add(nuevaVersion);
        Debug.Log($"¡Opción {nuevaVersion.numeroOpcion} guardada!");
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