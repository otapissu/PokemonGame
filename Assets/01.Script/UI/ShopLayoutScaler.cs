using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ShopArea에 부착.
// referenceWidth는 같은 오브젝트의 AspectRatioHeight에서 자동으로 가져옴.
[RequireComponent(typeof(RectTransform))]
public class ShopLayoutScaler : MonoBehaviour
{
    [Header("Button Layout Target")]
    [Tooltip("HorizontalLayoutGroup이 붙어있는 버튼 컨테이너 오브젝트")]
    [SerializeField] private RectTransform buttonLayoutTarget;
    [SerializeField] private float referenceSpacing = 12f;
    [SerializeField] private Vector2 referenceChildSize = new Vector2(192f, 192f);
    [SerializeField] private float buttonLayoutReferencePosY = 240f;

    [Header("Info Page (기준 크기 512 x 384)")]
    [Tooltip("bottom-center 앵커 Info 패널 — 화면 비율에 따라 localScale로 비례 축소/확대")]
    [SerializeField] private RectTransform infoPage;

    [Header("Tab Buttons")]
    [Tooltip("탭 버튼 컨테이너 — localScale로 비례 축소/확대")]
    [SerializeField] private RectTransform tabButtons;
    [Tooltip("기준 해상도에서의 anchoredPosition (top-left 앵커 기준)")]
    [SerializeField] private Vector2 tabButtonsReferencePos = new Vector2(72f, -294f);

    [Header("Evolve Buttons (ScrollView 부모)")]
    [Tooltip("ScrollView의 부모 — posY만 스케일")]
    [SerializeField] private RectTransform evolveButtonsParent;
    [SerializeField] private float evolveButtonsParentReferencePosY = 0f;
    [SerializeField] private float evolveButtonsParentReferenceHeight = 1040f;

    [Header("Evolve Scroll View (top-stretch)")]
    [Tooltip("top-stretch 앵커 ScrollView — height와 posY만 스케일")]
    [SerializeField] private RectTransform evolveScrollView;
    [Tooltip("기준 해상도에서의 height (sizeDelta.y)")]
    [SerializeField] private float evolveScrollViewReferenceHeight = 1040f;
    [Tooltip("기준 해상도에서의 anchoredPosition.y")]
    [SerializeField] private float evolveScrollViewReferencePosY = -196f;
    [Tooltip("GridLayoutGroup이 붙은 Content 오브젝트")]
    [SerializeField] private GridLayoutGroup evolveGridLayout;
    [Tooltip("기준 해상도에서의 GridLayoutGroup cellSize")]
    [SerializeField] private Vector2 evolveGridReferenceCellSize = new Vector2(192f, 192f);
    [Tooltip("기준 해상도에서의 GridLayoutGroup spacing")]
    [SerializeField] private Vector2 evolveGridReferenceSpacing = new Vector2(12f, 12f);
    [Tooltip("기준 해상도에서의 GridLayoutGroup padding.left")]
    [SerializeField] private int evolveGridReferencePaddingLeft = 20;

    private RectTransform rectTransform;
    private AspectRatioHeight aspectRatioHeight;
    private HorizontalLayoutGroup layoutGroup;
    private RectTransform[] childRects;
    private Vector3 infoPageReferenceScale;
    private Vector3 tabButtonsReferenceScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        aspectRatioHeight = GetComponent<AspectRatioHeight>();

        if (infoPage != null)
        {
            infoPageReferenceScale = infoPage.localScale;
        }

        if (tabButtons != null)
        {
            tabButtonsReferenceScale = tabButtons.localScale;
        }

        if (buttonLayoutTarget != null)
        {
            layoutGroup = buttonLayoutTarget.GetComponent<HorizontalLayoutGroup>();
            CacheChildren();
        }
    }

    private void Start()
    {
        StartCoroutine(ApplyScaleEndOfFrame());
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform == null)
        {
            return;
        }
        ApplyScale();
    }

    // 버튼 동적 생성 후 외부에서 즉시 갱신 호출용
    public void RefreshScale() => ApplyScale();

    private IEnumerator ApplyScaleEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        ApplyScale();
    }

    private void CacheChildren()
    {
        int count = buttonLayoutTarget.childCount;
        childRects = new RectTransform[count];
        for (int i = 0; i < count; i++)
        {
            childRects[i] = buttonLayoutTarget.GetChild(i) as RectTransform;
        }
    }

    private void ApplyScale()
    {
        float currentWidth = rectTransform.rect.width;
        if (currentWidth <= 0f)
        {
            return;
        }

        // referenceWidth는 AspectRatioHeight에서 가져옴 (없으면 현재 너비 = ratio 1.0)
        float refWidth = aspectRatioHeight != null ? aspectRatioHeight.ReferenceWidth : currentWidth;
        float ratio = currentWidth / refWidth;

        // 버튼 레이아웃 스케일
        if (layoutGroup != null && childRects != null)
        {
            buttonLayoutTarget.anchoredPosition = new Vector2(0f, buttonLayoutReferencePosY * ratio);
            layoutGroup.spacing = referenceSpacing * ratio;
            Vector2 scaledSize = referenceChildSize * ratio;

            for (int i = 0; i < childRects.Length; i++)
            {
                if (childRects[i] != null)
                {
                    childRects[i].sizeDelta = scaledSize;
                }
            }
        }

        // Info 페이지 비례 스케일 (에디터 기준 scale × ratio, 자식 포함)
        if (infoPage != null)
        {
            infoPage.localScale = infoPageReferenceScale * ratio;
        }

        // 탭 버튼 비례 스케일 + 위치
        if (tabButtons != null)
        {
            tabButtons.localScale = tabButtonsReferenceScale * ratio;
            tabButtons.anchoredPosition = tabButtonsReferencePos * ratio;
        }

        // Evolve 부모 — posY + height 스케일
        if (evolveButtonsParent != null)
        {
            evolveButtonsParent.anchoredPosition = new Vector2(0f, evolveButtonsParentReferencePosY * ratio);
            evolveButtonsParent.sizeDelta = new Vector2(0f, evolveButtonsParentReferenceHeight * ratio);
        }

        // Evolve ScrollView — height와 posY만 스케일 (가로 stretch 유지)
        if (evolveScrollView != null)
        {
            evolveScrollView.sizeDelta = new Vector2(0f, evolveScrollViewReferenceHeight * ratio);
            evolveScrollView.anchoredPosition = new Vector2(0f, evolveScrollViewReferencePosY * ratio);
        }

        // Evolve GridLayoutGroup — cellSize / spacing / padding.left 비례 스케일
        if (evolveGridLayout != null)
        {
            evolveGridLayout.cellSize     = evolveGridReferenceCellSize * ratio;
            evolveGridLayout.spacing      = evolveGridReferenceSpacing  * ratio;
            evolveGridLayout.padding.left = Mathf.RoundToInt(evolveGridReferencePaddingLeft * ratio);
        }
    }
}
