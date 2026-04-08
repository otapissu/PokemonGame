using UnityEngine;
using System.Collections;

public class DexPanelController : MonoBehaviour
{
    public GameObject pokedexRoot;
    public RectTransform dexPanel;
    public float animationDuration = 0.4f;
    public PokedexManager pokedexManager;

    [Header("Panel Position")]
    public Vector2 hiddenPos;
    public Vector2 shownPos = Vector2.zero;

    private bool isAnimating = false;

    private void Start()
    {
        if (dexPanel != null)
        {
            dexPanel.anchoredPosition = hiddenPos;
        }

        if (pokedexRoot != null)
        {
            pokedexRoot.SetActive(false);
        }
    }

    public void OpenDex()
    {
        if (isAnimating)
        {
            return;
        }

        if (pokedexRoot == null || dexPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPokedexOpen();
        }

        pokedexRoot.SetActive(true);
        dexPanel.anchoredPosition = hiddenPos;

        StartCoroutine(SlideUp());
    }

    public void CloseDex()
    {
        if (isAnimating)
        {
            return;
        }

        if (pokedexRoot == null || dexPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPokedexClose();
        }

        StartCoroutine(SlideDown());
    }

    private IEnumerator SlideUp()
    {
        isAnimating = true;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            dexPanel.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, EaseOutCubic(t));

            yield return null;
        }

        dexPanel.anchoredPosition = shownPos;

        if (pokedexManager != null)
        {
            pokedexManager.GenerateCurrentPage();
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
        isAnimating = false;

        if (pokedexRoot != null)
        {
            pokedexRoot.SetActive(false);
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