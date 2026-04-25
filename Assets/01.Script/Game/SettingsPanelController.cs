using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    public static SettingsPanelController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("BGM")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle bgmToggle;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    [Header("해상도")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("데이터 초기화")]
    [SerializeField] private GameObject confirmPanel;

    private static readonly int[] ResWidths = { 540, 576, 720 };
    private static readonly int[] ResHeights = { 960, 1024, 1280 };

    private int currentResolutionIndex = 0;

    private bool isInitializing;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        currentResolutionIndex = PlayerPrefs.GetInt("RESOLUTION_INDEX", 0);
        ApplyResolution(currentResolutionIndex, save: false);

        InitializeUI();

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
        if (bgmToggle != null)
        {
            bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        }
        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(new System.Collections.Generic.List<string>{ "540 x 960", "576 x 1024", "720 x 1280" });
            resolutionDropdown.onValueChanged.AddListener(OnResolutionSelected);
        }
    }

    private void InitializeUI()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        isInitializing = true;

        if (bgmSlider != null)
        {
            bgmSlider.value = SoundManager.Instance.IsBgmMuted() ? 0f : SoundManager.Instance.GetBgmVolume();
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.IsSfxMuted() ? 0f : SoundManager.Instance.GetSfxVolume();
        }
        if (bgmToggle != null)
        {
            bgmToggle.isOn = !SoundManager.Instance.IsBgmMuted();
        }
        if (sfxToggle != null)
        {
            sfxToggle.isOn = !SoundManager.Instance.IsSfxMuted();
        }

        isInitializing = false;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        }
            
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        bool next = !settingsPanel.activeSelf;
        if (next)
        {
            InitializeUI();
        }
        settingsPanel.SetActive(next);

        if (SoundManager.Instance != null)
        {
            if (next)
            {
                SoundManager.Instance.PlayButtonClick();
            }
            else
            {
                SoundManager.Instance.PlayButtonClose();
            }
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        settingsPanel.SetActive(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClose();
        }
    }

    private void OnBgmSliderChanged(float value)
    {
        if (isInitializing)
        {
            return;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBgmVolume(value);
        }
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isInitializing)
        {
            return;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSfxVolume(value);
        }
            
    }

    private void OnBgmToggleChanged(bool isOn)
    {
        if (isInitializing)
        {
            return;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBgmMute(!isOn);
            if (bgmSlider != null)
            {
                bgmSlider.SetValueWithoutNotify(isOn ? SoundManager.Instance.GetBgmVolume() : 0f);
            }
        }
    }

    private void OnSfxToggleChanged(bool isOn)
    {
        if (isInitializing)
        {
            return;
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSfxMute(!isOn);
            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(isOn ? SoundManager.Instance.GetSfxVolume() : 0f);
            }
        }
    }

    // 데이터 초기화
    public void OnResetButtonClicked()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
    }

    // 확인 팝업에서 "예" 클릭
    public void OnConfirmReset()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        // 포켓몬 / 골드
        if (EggEnhanceController.Instance != null)
        {
            EggEnhanceController.Instance.ResetAllGameData();
        }

        // 도감
        if (PokedexSaveManager.Instance != null)
        {
            PokedexSaveManager.Instance.ResetDex();
        }

        // 인벤토리
        if (PlayerGeneralInventory.Instance != null)
        {
            PlayerGeneralInventory.Instance.Reset();
        }

        if (PlayerEvolveInventory.Instance != null)
        {
            PlayerEvolveInventory.Instance.Reset();

        }
        // 가방 UI 갱신
        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.RefreshCounts();
        }

        // 튜토리얼
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ResetTutorial();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("Start");
    }

    public void OnCancelReset()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // 해상도 변경

    private void OnResolutionSelected(int index)
    {
        if (index == currentResolutionIndex)
        {
            return;
        }

        ApplyResolution(index, save: true);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopSelect();
        }
    }

    private void ApplyResolution(int index, bool save)
    {
        if (index < 0 || index >= ResWidths.Length)
        {
            return;
        }

        currentResolutionIndex = index;
        Screen.SetResolution(ResWidths[index], ResHeights[index], FullScreenMode.Windowed);

        if (save)
        {
            PlayerPrefs.SetInt("RESOLUTION_INDEX", index);
            PlayerPrefs.Save();
        }
    }
}
