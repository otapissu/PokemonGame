using UnityEngine;
using System.Collections;

public class ShopPanelController : MonoBehaviour
{
    public RectTransform shopPanel;
    public float animationDuration = 0.4f;
    public GameObject closeArea;

    [Header("Panel Position")]
    public Vector2 hiddenPos;
    public Vector2 shownPos = Vector2.zero;

    [Header("Tab Pages")]
    public GameObject genPage;
    public GameObject evolvePage;

    private bool isAnimating = false;
    private bool isOpen = false;

    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.anchoredPosition = hiddenPos;
        }

        if (closeArea != null)
        {
            closeArea.SetActive(false);
        }

        ShowGenPage();
    }

    public void OpenShop()
    {
        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.CloseBag();
            GeneralBagPanelController.Instance.HideRevivalCostUI();
        }

        if (isAnimating || isOpen)
        {
            return;
        }

        if (shopPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopOpen();
            SoundManager.Instance.PlayShopBgm();
        }

        ShowGenPage();
        StartCoroutine(SlideUp());
    }

    public void CloseShop()
    {
        if (isAnimating || !isOpen)
        {
            return;
        }

        if (shopPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClose();
            SoundManager.Instance.StopShopBgm();
        }

        ShopInfoPageController.Instance?.HideInfo();
        StartCoroutine(SlideDown());
    }

    public void ShowGenPage()
    {
        ShopInfoPageController.Instance?.HideInfo();
        if (genPage != null) genPage.SetActive(true);
        if (evolvePage != null) evolvePage.SetActive(false);
    }

    public void ShowEvolvePage()
    {
        ShopInfoPageController.Instance?.HideInfo();
        if (genPage != null) genPage.SetActive(false);
        if (evolvePage != null) evolvePage.SetActive(true);
    }

    private IEnumerator SlideUp()
    {
        isAnimating = true;
        isOpen = true;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            shopPanel.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, EaseOutCubic(t));

            yield return null;
        }

        shopPanel.anchoredPosition = shownPos;

        if (closeArea != null)
        {
            closeArea.SetActive(true);
        }

        isAnimating = false;
    }

    private IEnumerator SlideDown()
    {
        isAnimating = true;

        if (closeArea != null)
        {
            closeArea.SetActive(false);
        }

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            shopPanel.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, EaseInCubic(t));

            yield return null;
        }

        shopPanel.anchoredPosition = hiddenPos;
        isOpen = false;
        isAnimating = false;

        if (GeneralBagPanelController.Instance != null)
            GeneralBagPanelController.Instance.ShowRevivalCostUI();
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
