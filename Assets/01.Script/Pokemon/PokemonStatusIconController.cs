using UnityEngine;
using UnityEngine.UI;

public class PokemonStatusIconController : MonoBehaviour
{
    [Header("성별 이미지")]
    public Image genderImage;
    public Sprite maleSprite;
    public Sprite femaleSprite;

    [Header("아이콘 (상태)")]
    public Image iconImage;
    public Sprite newSprite;    // 1: 처음 본 폼
    public Sprite seenSprite;   // 2: 본 적 있지만 미15강
    public Sprite maxedSprite;  // 3: 15강 달성

    [Header("이로치 아이콘")]
    [SerializeField] private GameObject shinyIcon;

    public void Setup(PokemonInstance instance)
    {
        if (instance == null || instance.data == null)
        {
            Hide();
            return;
        }

        SetupGender(instance.gender);
        SetupIcon(instance);
        SetupShinyIcon(instance.isShiny);
    }

    public void Hide()
    {
        if (genderImage != null)
        {
            genderImage.enabled = false;
        }
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }
        if (shinyIcon != null)
        {
            shinyIcon.SetActive(false);
        }
    }

    private void SetupGender(Gender gender)
    {
        if (genderImage == null) return;

        switch (gender)
        {
            case Gender.Male:
                genderImage.enabled = true;
                genderImage.sprite = maleSprite;
                break;
            case Gender.Female:
                genderImage.enabled = true;
                genderImage.sprite = femaleSprite;
                break;
            default:
                genderImage.enabled = false;
                break;
        }
    }

    private void SetupShinyIcon(bool isShiny)
    {
        if (shinyIcon != null)
        {
            shinyIcon.SetActive(isShiny);
        }
    }

    private void SetupIcon(PokemonInstance instance)
    {
        if (iconImage == null) return;

        int id = instance.data.id;
        Gender gender = instance.gender;
        int formIndex = instance.formIndex;
        bool isShiny = instance.isShiny;
        GenderVisualType gvt = instance.data.genderVisualType;

        bool isMaxed = PokedexSaveManager.Instance != null &&
            (PokedexSaveManager.Instance.IsFormMaxed(id, gender, formIndex, isShiny, gvt) ||
             PokedexSaveManager.Instance.IsFormMaxedExact(id, gender, formIndex, isShiny));

        bool isSeen = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsFormSeen(id, gender, formIndex, isShiny, gvt);

        iconImage.enabled = true;

        if (isMaxed)
        {
            iconImage.sprite = maxedSprite;
        }
        else if (isSeen)
        {
            iconImage.sprite = seenSprite;
        }
        else
        {
            iconImage.sprite = newSprite;
        }
    }
}
