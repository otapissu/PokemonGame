using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DexInfoController : MonoBehaviour
{
    public static DexInfoController Instance { get; private set; }

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button closeButton;

    [Header("Gender Icon")]
    [SerializeField] private Image genderIcon;
    [SerializeField] private Sprite maleIconSprite;
    [SerializeField] private Sprite femaleIconSprite;

    [Header("Shiny Icon")]
    [SerializeField] private GameObject shinyIcon;

    // Current view state
    private PokemonData _data;
    private Gender _currentGender;
    private int _currentFormIndex;
    private bool _isShiny;
    private bool _owned;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInfo);
        }
    }

    private void Update()
    {
        if (infoPanel == null || !infoPanel.activeSelf)
        {
            return;
        }

        if (!_owned || _data == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGender();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            NextForm();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleShiny();
        }
    }

    public void ShowInfo(string pokemonName, Sprite sprite, bool owned, PokemonData data = null, Gender gender = Gender.None, int formIndex = 0, bool isShiny = false)
    {
        _data = data;
        _currentGender = gender;
        _currentFormIndex = formIndex;
        _isShiny = isShiny;
        _owned = owned;

        if (nameText != null)
        {
            nameText.text = owned ? pokemonName : "???";
        }

        if (pokemonImage != null)
        {
            pokemonImage.sprite = sprite;
            pokemonImage.preserveAspect = true;
            pokemonImage.color = owned ? Color.white : Color.black;
        }

        UpdateGenderIcon();
        UpdateShinyIcon();

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    // G: 반대 성별로 전환. DifferentVisual이면 스프라이트도 변경, SameVisual이면 아이콘만 변경, None이면 무시
    private void ToggleGender()
    {
        if (_data.genderVisualType == GenderVisualType.None)
        {
            return;
        }

        _currentGender = (_currentGender == Gender.Male) ? Gender.Female : Gender.Male;

        if (_data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            // 새 성별에서 현재 폼이 없으면 첫 번째 폼으로 리셋
            List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

            if (!forms.Contains(_currentFormIndex))
            {
                _currentFormIndex = forms.Count > 0 ? forms[0] : 0;
            }

            RefreshSprite();
        }
        else
        {
            // SameVisual: 스프라이트/색상은 그대로, 아이콘만 변경
            UpdateGenderIcon();
            UpdateShinyIcon();
        }
    }

    // F: 다음 폼으로 변경. 폼이 하나뿐이면 무시
    private void NextForm()
    {
        List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

        if (forms.Count <= 1)
        {
            return;
        }

        int idx = forms.IndexOf(_currentFormIndex);
        idx = (idx + 1) % forms.Count;
        _currentFormIndex = forms[idx];

        RefreshSprite();
    }

    // S: 이로치/비이로치 전환
    private void ToggleShiny()
    {
        _isShiny = !_isShiny;

        // 이로치 상태에서 현재 폼이 없으면 첫 번째 폼으로 리셋
        List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

        if (!forms.Contains(_currentFormIndex))
        {
            _currentFormIndex = forms.Count > 0 ? forms[0] : 0;
        }

        RefreshSprite();
    }

    private void RefreshSprite()
    {
        if (_data == null || pokemonImage == null)
        {
            return;
        }

        string path = PokemonFormUtility.GetLoadPath(_data, _currentGender, _isShiny, _currentFormIndex);
        Sprite[] loaded = Resources.LoadAll<Sprite>(path);

        if (loaded != null && loaded.Length > 0)
        {
            pokemonImage.sprite = loaded[0];
        }

        bool seen = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsFormSeen(_data.id, _currentGender, _currentFormIndex, _isShiny, _data.genderVisualType);
        pokemonImage.color = seen ? Color.white : Color.black;

        UpdateGenderIcon();
        UpdateShinyIcon();
    }

    private void UpdateShinyIcon()
    {
        if (shinyIcon == null)
        {
            return;
        }

        shinyIcon.SetActive(_isShiny);
    }

    private void UpdateGenderIcon()
    {
        if (genderIcon == null)
        {
            return;
        }

        if (_data == null || _data.genderVisualType == GenderVisualType.None)
        {
            genderIcon.gameObject.SetActive(false);
            return;
        }

        genderIcon.gameObject.SetActive(true);
        genderIcon.sprite = _currentGender == Gender.Female ? femaleIconSprite : maleIconSprite;
    }
}
