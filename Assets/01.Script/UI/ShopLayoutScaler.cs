using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class ShopLayoutScaler : MonoBehaviour
{
    [Header("Reference Width")]
    [SerializeField] private float referenceWidth = 1080f;

    [Header("Reference Spacing")]
    [SerializeField] private float referenceSpacing = 12f;

    [Header("Reference Child Size")]
    [SerializeField] private Vector2 referenceChildSize = new Vector2(192f, 192f);

    private HorizontalLayoutGroup layoutGroup;
    private RectTransform rectTransform;
    private RectTransform[] childRects;

    private void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        CacheChildren();
    }

    private void Start()
    {
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (layoutGroup == null || rectTransform == null)
        {
            return;
        }

        ApplyScale();
    }

    private void CacheChildren()
    {
        childRects = new RectTransform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            childRects[i] = transform.GetChild(i) as RectTransform;
        }
    }

    private void ApplyScale()
    {
        float currentWidth = rectTransform.rect.width;

        if (currentWidth <= 0f)
        {
            return;
        }

        float ratio = currentWidth / referenceWidth;

        layoutGroup.spacing = referenceSpacing * ratio;

        Vector2 scaledSize = referenceChildSize * ratio;

        for (int i = 0; i < childRects.Length; i++)
        {
            if (childRects[i] == null)
            {
                continue;
            }

            childRects[i].sizeDelta = scaledSize;
        }
    }
}
