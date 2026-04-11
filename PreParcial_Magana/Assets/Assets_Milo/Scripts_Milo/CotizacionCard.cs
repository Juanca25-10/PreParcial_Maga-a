using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CotizacionCard : MonoBehaviour
{
    [SerializeField] private RawImage fotoPreview;
    [SerializeField] private TextMeshProUGUI txtNombre;
    [SerializeField] private TextMeshProUGUI txtPresupuesto;
    [SerializeField] private TextMeshProUGUI txtCostoFinal;

    public void Configurar(VersionDiseño datos)
    {
        // Asignamos la captura de pantalla
        fotoPreview.texture = datos.foto;

        // Datos principales
        txtNombre.text = datos.numeroOpcion > 0 ? $"Opción {datos.numeroOpcion}" : "Diseño";
        txtPresupuesto.text = $"Presupuesto: ${BudgetManager.Instance.presupuestoInicial:N0}";
        txtCostoFinal.text = $"Inversión: ${datos.costoTotal:N0}";
    }
}
