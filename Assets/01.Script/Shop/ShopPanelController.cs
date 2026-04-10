using UnityEngine;
using System.Collections;

public class ShopPanelController : MonoBehaviour
{
    public GameObject shopRoot;
    public RectTransform shopPanel;
    public float animationDuration = 0.4f;

    [Header("Panel Position")]
    public Vector2 hiddenPos;
    public Vector2 shownPos = Vector2.zero;

    [Header("Tab Pages")]
    public GameObject genPage;
    public GameObject evolvePage;

    private bool isAnimating = false;

    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.anchoredPosition = hiddenPos;
        }

        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
    }

    public void OpenShop()
    {
        if (isAnimating)
        {
            return;
        }

        if (shopRoot == null || shopPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopOpen();
            SoundManager.Instance.PlayShopBgm();
        }

        ShowGenPage();

        shopRoot.SetActive(true);
        shopPanel.anchoredPosition = hiddenPos;

        StartCoroutine(SlideUp());
    }

    public void CloseShop()
    {
        if (isAnimating)
        {
            return;
        }

        if (shopRoot == null || shopPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClose();
            SoundManager.Instance.StopShopBgm();
        }

        StartCoroutine(SlideDown());
    }

    public void ShowGenPage()
    {
        if (genPage != null) genPage.SetActive(true);
        if (evolvePage != null) evolvePage.SetActive(false);
    }

    public void ShowEvolvePage()
    {
        if (genPage != null) genPage.SetActive(false);
        if (evolvePage != null) evolvePage.SetActive(true);
    }

    private IEnumerator SlideUp()
    {
        isAnimating = true;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            shopPanel.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, EaseOutCubic(t));

            yield return null;
        }

        shopPanel.anchoredPosition = shownPos;
        isAnimating = false;
    }

    private IEnumerator SlideDown()
    {
        isAnimating = true;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            shopPanel.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, EaseInCubic(t));

            yield return null;
        }

        shopPanel.anchoredPosition = hiddenPos;
        isAnimating = false;

        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }
}
