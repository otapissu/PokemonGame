using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AspectRatioHeight : MonoBehaviour
{
    [SerializeField] private float referenceWidth = 1080f;
    [SerializeField] private float referenceHeight = 1480f;

    public float ReferenceWidth => referenceWidth;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ApplyHeight();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform == null)
        {
            return;
        }

        ApplyHeight();
    }

    private void ApplyHeight()
    {
        float width = rectTransform.rect.width;

        if (width <= 0f)
        {
            return;
        }

        float height = width * (referenceHeight / referenceWidth);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
