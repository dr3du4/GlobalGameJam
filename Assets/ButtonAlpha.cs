using UnityEngine;
using UnityEngine.UI;

public class ButtonAlpha : MonoBehaviour
{
    void Awake()
    {
        var img = GetComponent<Image>();
        img.alphaHitTestMinimumThreshold = 0.2f; // 0..1 (im wyżej, tym mniej “przezroczyste” klika)
    }
}