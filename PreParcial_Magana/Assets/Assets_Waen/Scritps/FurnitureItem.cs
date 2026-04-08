using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureItem", menuName = "AR Furniture/Furniture Item")]
public class FurnitureItem : ScriptableObject
{
    public string nombreMueble;
    public GameObject prefab3D;
    public Sprite iconoUI;
}
