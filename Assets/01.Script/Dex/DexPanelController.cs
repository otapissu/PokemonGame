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

        // pokedexRoot는 항상 켜진 상태 유지 (SetActive 토글 안 함)
        // 최초에 슬롯 풀 생성 및 첫 페이지 데이터 로드
        if (pokedexManager != null)
        {
            pokedexManager.GenerateCurrentPage();
            pokedexManager.enabled = false;
        }
    }

    public void OpenDex()
    {
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

        // 열릴 때 현재 페이지 데이터 갱신 (소유 상태 등 반영)
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