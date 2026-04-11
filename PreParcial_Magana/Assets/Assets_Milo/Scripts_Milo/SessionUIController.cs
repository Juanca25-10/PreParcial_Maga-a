using UnityEngine;
using TMPro; // Importante para usar TextMeshPro
using UnityEngine.UI;

public class SessionUIController : MonoBehaviour
{
    [Header("Panel de Inicio")]
    [SerializeField] private GameObject panelBienvenida;
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private TMP_InputField inputPresupuesto;

    [Header("HUD de Juego")]
    [SerializeField] private TextMeshProUGUI textoPresupuestoRestante;
    [SerializeField] private Button botonTerminar;

    void Start()
    {
        // Aseguramos que el panel de bienvenida esté activo al iniciar
        panelBienvenida.SetActive(true);

        // Conectamos el botón de terminar diseño
        if (botonTerminar != null)
            botonTerminar.onClick.AddListener(FinalizarDiseno);
    }

    // Este método se llama desde el botón "EMPEZAR" del panel de bienvenida
    public void ConfirmarDatosIniciales()
    {
        string nombre = inputNombre.text;
        float presupuesto = 0;

        // Validamos que el presupuesto sea un número válido
        if (float.TryParse(inputPresupuesto.text, out presupuesto))
        {
            // Le mandamos los datos al BudgetManager
            BudgetManager.Instance.IniciarSesion(nombre, presupuesto);

            // Cerramos el panel y actualizamos el HUD
            panelBienvenida.SetActive(false);
            ActualizarTextoPresupuesto();
        }
        else
        {
            Debug.LogError("Por favor, ingresa un presupuesto válido (solo números).");
        }
    }

    void Update()
    {
        // Si el panel de bienvenida está apagado, actualizamos el HUD constantemente
        if (!panelBienvenida.activeSelf)
        {
            ActualizarTextoPresupuesto();
        }
    }

    private void ActualizarTextoPresupuesto()
    {
        if (textoPresupuestoRestante != null)
        {
            // Usamos :N0 para que se vea como moneda sin decimales, o :F2 para dos decimales
            textoPresupuestoRestante.text = "Presupuesto: $" + BudgetManager.Instance.presupuestoRestante.ToString("N0");
        }
    }

    public void FinalizarDiseno()
    {
        BudgetManager.Instance.GuardarVersion();
        Debug.Log("Captura tomada y versión guardada.");
    }
}