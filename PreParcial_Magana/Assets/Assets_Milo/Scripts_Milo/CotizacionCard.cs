using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CotizacionCard : MonoBehaviour
{
    [SerializeField] private RawImage fotoPreview;
    [SerializeField] private TextMeshProUGUI txtNombre;
    [SerializeField] private TextMeshProUGUI txtCostoFinal;

    private VersionDiseño misDatos;

    public void Configurar(VersionDiseño datos)
    {
        misDatos = datos;
        fotoPreview.texture = datos.foto;
        txtNombre.text = $"Opción {datos.numeroOpcion}";
        txtCostoFinal.text = $"Total: ${datos.costoTotal:N0}";
    }

    // Se asigna al botón de la imagen para verla grande
    public void VerFotoGrande()
    {
        // Verificamos si la instancia existe antes de llamarla
        if (VisualizadorCotizacion.Instance != null && misDatos.foto != null)
        {
            VisualizadorCotizacion.Instance.AbrirFoto(misDatos.foto);
        }
        else
        {
            Debug.LogError("O el Visualizador no está activo en la escena o la foto no se guardó.");
        }
    }
}
