using UnityEngine;
using UnityEngine.UI;

public class VisualizadorCotizacion : MonoBehaviour
{
    public static VisualizadorCotizacion Instance; // Singleton simple solo para UI
    [SerializeField] private RawImage imagenFull;
    [SerializeField] private GameObject panel;

    void Awake() => Instance = this;

    public void AbrirFoto(Texture2D foto)
    {
        imagenFull.texture = foto;
        panel.SetActive(true);
    }

    public void Cerrar() => panel.SetActive(false);
}
