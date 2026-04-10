using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class DexGridScaler : MonoBehaviour
{
    [Header("Reference Width")]
    [SerializeField] private float referenceWidth = 1080f;

    [Header("Reference Cell")]
    [SerializeField] private Vector2 referenceCellSize = new Vector2(172f, 172f);

    [Header("Reference Spacing")]
    [SerializeField] private Vector2 referenceSpacing = new Vector2(24f, 20f);

    [Header("Reference Padding")]
    [SerializeField] private int referencePaddingLeft = 60;
    [SerializeField] private int referencePaddingRight = 0;
    [SerializeField] private int referencePaddingTop = 0;
    [SerializeField] private int referencePaddingBottom = 0;

    [Header("Reference Position Y")]
    [SerializeField] private float referencePosY = 158f;

    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        StartCoroutine(ApplyScaleNextFrame());
    }

    private IEnumerator ApplyScaleNextFrame()
    {
        yield return new WaitForEndOfFrame();
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (grid == null || rectTransform == null)
        {
            return;
        }

        ApplyScale();
    }

    private void ApplyScale()
    {
        float currentWidth = rectTransform.rect.width;

        if (currentWidth <= 0f)
        {
            return;
        }

        float ratio = currentWidth / referenceWidth;

        grid.cellSize = referenceCellSize * ratio;
        grid.spacing = referenceSpacing * ratio;
        grid.padding.left = Mathf.RoundToInt(referencePaddingLeft * ratio);
        grid.padding.right = Mathf.RoundToInt(referencePaddingRight * ratio);
        grid.padding.top = Mathf.RoundToInt(referencePaddingTop * ratio);
        grid.padding.bottom = Mathf.RoundToInt(referencePaddingBottom * ratio);

        float scaledCellH = referenceCellSize.y * ratio;
        float scaledSpacingY = referenceSpacing.y * ratio;
        float totalHeight = scaledCellH * 5 + scaledSpacingY * 4;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = referencePosY * ratio;
        rectTransform.anchoredPosition = pos;
    }
}
