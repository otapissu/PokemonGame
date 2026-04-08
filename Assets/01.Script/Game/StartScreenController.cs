using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image logoImage;
    [SerializeField] private TextMeshProUGUI touchToStartText;

    [Header("Fade Settings")]
    [SerializeField] private float logoFadeDuration = 2f;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainScene";

    private bool canTouch = false;
    private bool isLoading = false;
    private Coroutine blinkCoroutine;

    private void Start()
    {
        InitializeUI();
        StartCoroutine(FadeInLogoRoutine());
    }

    private void Update()
    {
        if (canTouch == false)
        {
            return;
        }

        if (isLoading == true)
        {
            return;
        }

        if (IsTouchInputDetected() == true)
        {
            StartCoroutine(LoadNextSceneRoutine());
        }
    }

    private void InitializeUI()
    {
        if (logoImage != null)
        {
            Color logoColor = logoImage.color;
            logoColor.a = 0f;
            logoImage.color = logoColor;
        }

        if (touchToStartText != null)
        {
            Color textColor = touchToStartText.color;
            textColor.a = 0f;
            touchToStartText.color = textColor;
            touchToStartText.gameObject.SetActive(false);
        }

        canTouch = false;
        isLoading = false;
    }

    private IEnumerator FadeInLogoRoutine()
    {
        if (logoImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color logoColor = logoImage.color;

        while (elapsed < logoFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / logoFadeDuration);

            logoColor.a = t;
            logoImage.color = logoColor;

            yield return null;
        }

        logoColor.a = 1f;
        logoImage.color = logoColor;

        EnableTouchStart();
    }

    private void EnableTouchStart()
    {
        canTouch = true;

        if (touchToStartText != null)
        {
            touchToStartText.gameObject.SetActive(true);

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }

            blinkCoroutine = StartCoroutine(BlinkTextRoutine());
        }
    }

    private IEnumerator BlinkTextRoutine()
    {
        while (true)
        {
            if (touchToStartText != null)
            {
                Color textColor = touchToStartText.color;
                textColor.a = 1f;
                touchToStartText.color = textColor;
            }

            yield return new WaitForSeconds(blinkInterval);

            if (touchToStartText != null)
            {
                Color textColor = touchToStartText.color;
                textColor.a = 0f;
                touchToStartText.color = textColor;
            }

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private bool IsTouchInputDetected()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        isLoading = true;
        canTouch = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }

        yield return null;

        SceneManager.LoadScene(nextSceneName);
    }
}