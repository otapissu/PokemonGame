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
    [SerializeField] private AudioClip enhanceSuccessSfx;
    [SerializeField] private AudioClip enhanceFailSfx;
    [SerializeField] private AudioClip enhanceDestroySfx;

    [Header("BGM Option")]
    [SerializeField] private bool shuffleMainBgm = false;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private bool bgmMuted = false;
    private bool sfxMuted = false;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private int currentMainBgmIndex = -1;
    private bool isPlayingStartSceneBgm = false;
    private bool isPlayingShopBgm = false;
    private float savedMainBgmTime = 0f;

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
        LoadSettings();
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
        bgmSource.volume = bgmMuted ? 0f : bgmVolume;

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
        bgmSource.volume = bgmMuted ? 0f : bgmVolume;
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

        if (!isPlayingStartSceneBgm && !isPlayingShopBgm && bgmSource.isPlaying)
        {
            savedMainBgmTime = bgmSource.time;
        }

        isPlayingStartSceneBgm = false;
        isPlayingShopBgm = true;

        bgmSource.Stop();
        bgmSource.clip = shopBgm;
        bgmSource.volume = bgmMuted ? 0f : bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 상점 닫힐 때 호출 — 상점 BGM을 멈추고 메인 BGM을 멈췄던 위치부터 재개
    public void StopShopBgm()
    {
        if (isPlayingShopBgm == false)
        {
            return;
        }

        isPlayingShopBgm = false;

        if (currentMainBgmIndex >= 0 && mainBgmList != null && currentMainBgmIndex < mainBgmList.Count)
        {
            AudioClip clip = mainBgmList[currentMainBgmIndex];

            if (clip != null)
            {
                bgmSource.Stop();
                bgmSource.clip = clip;
                bgmSource.volume = bgmMuted ? 0f : bgmVolume;
                bgmSource.loop = false;
                bgmSource.time = savedMainBgmTime;
                bgmSource.Play();
                return;
            }
        }

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

    // SFX
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

    public void PlayEnhanceSuccess()
    {
        PlaySfxOneShot(enhanceSuccessSfx);
    }

    public void PlayEnhanceFail()
    {
        PlaySfxOneShot(enhanceFailSfx);
    }

    public void PlayEnhanceDestroy()
    {
        PlaySfxOneShot(enhanceDestroySfx);
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

        if (sfxMuted) return;
        sfxSource.PlayOneShot(sfxClip, Mathf.Clamp01(volumeScale) * sfxVolume);
    }

    public float GetBgmVolume() => bgmVolume;
    public float GetSfxVolume() => sfxVolume;
    public bool IsBgmMuted() => bgmMuted;
    public bool IsSfxMuted() => sfxMuted;
    public string GetCurrentBgmName() => bgmSource != null && bgmSource.clip != null ? bgmSource.clip.name : "";

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmMuted ? 0f : bgmVolume;
        }

        SaveSettings();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveSettings();
    }

    public void SetBgmMute(bool mute)
    {
        bgmMuted = mute;

        if (bgmSource != null)
        {
            bgmSource.volume = bgmMuted ? 0f : bgmVolume;
        }

        SaveSettings();
    }

    public void SetSfxMute(bool mute)
    {
        sfxMuted = mute;
        SaveSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("SOUND_BGM_VOL",  bgmVolume);
        PlayerPrefs.SetFloat("SOUND_SFX_VOL",  sfxVolume);
        PlayerPrefs.SetInt("SOUND_BGM_MUTE",   bgmMuted ? 1 : 0);
        PlayerPrefs.SetInt("SOUND_SFX_MUTE",   sfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("SOUND_BGM_VOL", bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat("SOUND_SFX_VOL", sfxVolume);
        bgmMuted  = PlayerPrefs.GetInt("SOUND_BGM_MUTE", 0) == 1;
        sfxMuted  = PlayerPrefs.GetInt("SOUND_SFX_MUTE", 0) == 1;
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
        bgmSource.volume = bgmMuted ? 0f : bgmVolume;
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