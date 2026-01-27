using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ForceCanvasSorting : MonoBehaviour
{
    public string sortingLayerName = "UI";
    public int orderInLayer = 100;

    void Awake()
    {
        var c = GetComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingLayerName = sortingLayerName;
        c.sortingOrder = orderInLayer;
    }
}
