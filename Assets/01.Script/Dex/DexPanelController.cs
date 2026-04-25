using UnityEngine;
using System.Collections;

public class DexPanelController : MonoBehaviour
{
    public GameObject pokedexRoot;
    public RectTransform dexPanel;
    public float animationDuration = 0.4f;
    public PokedexManager pokedexManager;
    public GameObject closeArea;

    [Header("Panel Position")]
    public Vector2 hiddenPos;
    public Vector2 shownPos = Vector2.zero;

    private bool isAnimating = false;
    private bool isOpen = false;

    private void Update()
    {
        if (!isOpen) { return; }
        if (Input.GetKeyDown(KeyCode.Escape)) { CloseDex(); }
    }

    private void Start()
    {
        if (dexPanel != null)
        {
            dexPanel.anchoredPosition = hiddenPos;
        }

        if (closeArea != null)
        {
            closeArea.SetActive(false);
        }

        if (pokedexManager != null)
        {
            pokedexManager.GenerateCurrentPage();
            pokedexManager.enabled = false;
        }
    }

    public void OpenDex()
    {
        EvolveBagPanelController.Instance?.Close();
        EvolveBagPanelController.Instance?.ClearSelection();

        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.CloseBag();
            GeneralBagPanelController.Instance.HideRevivalCostUI();
        }

        if (isAnimating || isOpen)
        {
            return;
        }

        if (dexPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPokedexOpen();
        }

        // 열릴 때 현재 페이지 데이터 갱신
        if (pokedexManager != null)
        {
            pokedexManager.GenerateCurrentPage();
        }

        StartCoroutine(SlideUp());
    }

    public void CloseDex()
    {
        if (isAnimating || !isOpen)
        {
            return;
        }

        if (DexInfoController.Instance != null && DexInfoController.Instance.IsInfoOpen)
        {
            DexInfoController.Instance.HideInfo();
            return;
        }

        if (dexPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClose();
        }

        StartCoroutine(SlideDown());
    }

    private IEnumerator SlideUp()
    {
        isAnimating = true;
        isOpen = true;

        if (pokedexManager != null)
        {
            pokedexManager.enabled = true;
        }

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            dexPanel.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, EaseOutCubic(t));

            yield return null;
        }

        dexPanel.anchoredPosition = shownPos;

        if (closeArea != null)
        {
            closeArea.SetActive(true);
        }

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

            dexPanel.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, EaseInCubic(t));

            yield return null;
        }

        dexPanel.anchoredPosition = hiddenPos;

        if (closeArea != null)
        {
            closeArea.SetActive(false);
        }

        if (pokedexManager != null)
        {
            pokedexManager.enabled = false;
        }

        isOpen = false;
        isAnimating = false;

        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.ShowRevivalCostUI();
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