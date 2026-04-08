using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    private Material materialValido;
    private Material materialInvalido;
    private Renderer[] renderers;
    private bool esValido = false;

    public void Inicializar(Material valido, Material invalido)
    {
        materialValido = valido;
        materialInvalido = invalido;
        renderers = GetComponentsInChildren<Renderer>();
        SetValido(false);
    }

    public void SetValido(bool valido)
    {
        esValido = valido;
        Material mat = valido ? materialValido : materialInvalido;

        foreach (var r in renderers)
        {
            // Creamos instancia del material para no modificar el asset original
            r.material = new Material(mat);
        }
    }

    public bool EsValido() => esValido;
}
