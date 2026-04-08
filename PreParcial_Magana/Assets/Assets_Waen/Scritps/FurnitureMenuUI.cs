using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FurnitureMenuUI : MonoBehaviour
{
    [Header("Datos de muebles disponibles")]
    [SerializeField] private FurnitureItem[] muebles;

    [Header("Referencias UI")]
    [SerializeField] private Transform contenedorItems;
    [SerializeField] private GameObject itemPrefabUI;

    [Header("Placer")]
    [SerializeField] private FurnitureDragPlacer dragPlacer;

    void Start()
    {
        GenerarMenu();
    }

    private void GenerarMenu()
    {
        foreach (FurnitureItem mueble in muebles)
        {
            GameObject itemUI = Instantiate(itemPrefabUI, contenedorItems);

            // Asignar icono
            Image icono = itemUI.transform.Find("Icono").GetComponent<Image>();
            if (icono != null && mueble.iconoUI != null)
                icono.sprite = mueble.iconoUI;

            // Asignar nombre
            TextMeshProUGUI nombre = itemUI.transform.Find("Nombre").GetComponent<TextMeshProUGUI>();
            if (nombre != null)
                nombre.text = mueble.nombreMueble;

            // Asignar evento de arrastre
            FurnitureItem muebleCapturado = mueble;
            EventTrigger trigger = itemUI.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((_) =>
            {
                dragPlacer.IniciarArrastre(muebleCapturado.prefab3D);
            });

            trigger.triggers.Add(entry);
        }
    }
}
