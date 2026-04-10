using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Scene Name")]
    [SerializeField] private string startSceneName = "Start";

    [Header("Start Scene BGM")]
    [SerializeField] private AudioClip startSceneBgm;

    [Header("Main BGM List")]
    [SerializeField] private List<AudioClip> mainBgmList = new List<AudioClip>();

    [Header("Shop BGM")]
    [SerializeField] private AudioClip shopBgm;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField] private AudioClip eggLevel15Sfx;
    [SerializeField] private AudioClip shinyAppearSfx;
    [SerializeField] private AudioClip pokedexOpenSfx;
    [SerializeField] private AudioClip shopOpenSfx;
    [SerializeField] private AudioClip bagOpenSfx;
    [SerializeField] private AudioClip shopSelectSfx;
    [SerializeField] private AudioClip shopBuySfx;
    [SerializeField] private AudioClip buttonCloseSfx;

    [Header("BGM Option")]
    [SerializeField] private bool shuffleMainBgm = false;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private int currentMainBgmIndex = -1;
    private bool isPlayingStartSceneBgm = false;
    private bool isPlayingShopBgm = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        RefreshBgmByScene();
    }

    private void Update()
    {
        TryPlayNextMainBgm();
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length >= 2)
        {
            bgmSource = sources[0];
            sfxSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            bgmSource = sources[0];
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = false;
        bgmSource.volume = bgmVolume;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPlayingShopBgm = false;
        RefreshBgmByScene();
    }

    private void RefreshBgmByScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == startSceneName)
        {
            PlayStartSceneBgm();
        }
        else
        {
            PlayMainBgmList();
        }
    }

    public void PlayStartSceneBgm()
    {
        if (startSceneBgm == null)
        {
            Debug.LogWarning("[SoundManager] startSceneBgm이 비어 있습니다.");
            return;
        }

        if (bgmSource == null)
        {
            Debug.LogWarning("[SoundManager] bgmSource가 없습니다.");
            return;
        }

        if (isPlayingStartSceneBgm == true && bgmSource.clip == startSceneBgm && bgmSource.isPlaying == true)
        {
            return;
        }

        isPlayingStartSceneBgm = true;
        isPlayingShopBgm = false;
        currentMainBgmIndex = -1;

        bgmSource.Stop();
        bgmSource.clip = startSceneBgm;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayMainBgmList()
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("[SoundManager] bgmSource가 없습니다.");
            return;
        }

        if (mainBgmList == null || mainBgmList.Count == 0)
        {
            Debug.LogWarning("[SoundManager] mainBgmList가 비어 있습니다.");
            return;
        }

        if (isPlayingStartSceneBgm == false && isPlayingShopBgm == false && bgmSource.isPlaying == true && bgmSource.clip != null)
        {
            return;
        }

        isPlayingStartSceneBgm = false;
        isPlayingShopBgm = false;

        if (shuffleMainBgm == true)
        {
            PlayRandomMainBgm();
            return;
        }

        currentMainBgmIndex = 0;
        PlayMainBgmByIndex(currentMainBgmIndex);
    }

    // 상점 오픈 시 호출 — 메인 BGM을 멈추고 상점 BGM을 루프 재생
    public void PlayShopBgm()
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("[SoundManager] bgmSource가 없습니다.");
            return;
        }

        if (shopBgm == null)
        {
            Debug.LogWarning("[SoundManager] shopBgm이 비어 있습니다.");
            return;
        }

        isPlayingStartSceneBgm = false;
        isPlayingShopBgm = true;
        currentMainBgmIndex = -1;

        bgmSource.Stop();
        bgmSource.clip = shopBgm;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 상점 닫힐 때 호출 — 상점 BGM을 멈추고 메인 BGM으로 복귀
    public void StopShopBgm()
    {
        if (isPlayingShopBgm == false)
        {
            return;
        }

        isPlayingShopBgm = false;

        bgmSource.Stop();
        bgmSource.clip = null;

        PlayMainBgmList();
    }

    public void StopBgm()
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("[SoundManager] bgmSource가 없습니다.");
            return;
        }

        bgmSource.Stop();
    }

    // ── SFX ──────────────────────────────────────────

    public void PlayButtonClick()
    {
        PlaySfxOneShot(buttonClickSfx);
    }

    public void PlayButtonClose()
    {
        PlaySfxOneShot(buttonCloseSfx);
    }

    public void PlayEggLevel15()
    {
        PlaySfxOneShot(eggLevel15Sfx);
    }

    public void PlayShinyAppear()
    {
        PlaySfxOneShot(shinyAppearSfx);
    }

    public void PlayPokedexOpen()
    {
        PlaySfxOneShot(pokedexOpenSfx);
    }

    public void PlayShopOpen()
    {
        PlaySfxOneShot(shopOpenSfx);
    }

    public void PlayBagOpen()
    {
        PlaySfxOneShot(bagOpenSfx);
    }

    public void PlayShopSelect()
    {
        PlaySfxOneShot(shopSelectSfx);
    }

    public void PlayShopBuy()
    {
        PlaySfxOneShot(shopBuySfx);
    }

    public void PlaySfxOneShot(AudioClip sfxClip, float volumeScale = 1f)
    {
        if (sfxClip == null)
        {
            Debug.LogWarning("[SoundManager] 재생할 효과음이 없습니다.");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("[SoundManager] sfxSource가 없습니다.");
            return;
        }

        sfxSource.PlayOneShot(sfxClip, Mathf.Clamp01(volumeScale) * sfxVolume);
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void TryPlayNextMainBgm()
    {
        if (bgmSource == null)
        {
            return;
        }

        if (isPlayingStartSceneBgm == true)
        {
            return;
        }

        if (isPlayingShopBgm == true)
        {
            return;
        }

        if (mainBgmList == null || mainBgmList.Count == 0)
        {
            return;
        }

        if (bgmSource.isPlaying == true)
        {
            return;
        }

        if (bgmSource.clip == null)
        {
            return;
        }

        if (shuffleMainBgm == true)
        {
            PlayRandomMainBgm();
            return;
        }

        currentMainBgmIndex++;

        if (currentMainBgmIndex >= mainBgmList.Count)
        {
            currentMainBgmIndex = 0;
        }

        PlayMainBgmByIndex(currentMainBgmIndex);
    }

    private void PlayMainBgmByIndex(int index)
    {
        if (mainBgmList == null || mainBgmList.Count == 0)
        {
            Debug.LogWarning("[SoundManager] mainBgmList가 비어 있습니다.");
            return;
        }

        if (index < 0 || index >= mainBgmList.Count)
        {
            Debug.LogWarning("[SoundManager] 잘못된 메인 BGM 인덱스입니다: " + index);
            return;
        }

        AudioClip clip = mainBgmList[index];

        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] mainBgmList의 클립이 null입니다. index: " + index);
            return;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = false;
        bgmSource.Play();
    }

    private void PlayRandomMainBgm()
    {
        if (mainBgmList == null || mainBgmList.Count == 0)
        {
            Debug.LogWarning("[SoundManager] mainBgmList가 비어 있습니다.");
            return;
        }

        int nextIndex = Random.Range(0, mainBgmList.Count);

        if (mainBgmList.Count > 1 && nextIndex == currentMainBgmIndex)
        {
            nextIndex = (nextIndex + 1) % mainBgmList.Count;
        }

        currentMainBgmIndex = nextIndex;
        PlayMainBgmByIndex(currentMainBgmIndex);
    }
}