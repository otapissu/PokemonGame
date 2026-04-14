using UnityEngine;
using UnityEngine.UI;

public class DexSlotUI : MonoBehaviour
{
    public Image iconImage;
    public Image outlineImage;

    [Header("아웃라인 스프라이트 (순서 고정)")]
    public Sprite defaultOutlineSprite;
    public Sprite normalAnyMaxSprite;
    public Sprite normalAllMaxSprite;
    public Sprite legendaryAnyMaxSprite;
    public Sprite legendaryAllMaxSprite;

    private Sprite[] frames;

    public void Setup(int id, Sprite[] cachedFrames, PokemonData data)
    {
        frames = cachedFrames;

        if (frames == null || frames.Length == 0)
        {
            iconImage.sprite = null;
            outlineImage.enabled = false;
            return;
        }

        bool owned     = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsOwned(id);
        bool anyMaxed  = owned && PokedexSaveManager.Instance.IsAnyFormMaxed(id);
        bool allMaxed  = anyMaxed && data != null && (PokedexSaveManager.Instance.IsAllFormsMaxed(id, data) || PokedexSaveManager.Instance.IsChainAllMaxed(id));
        bool legendary = data != null && data.isLegendary;

        iconImage.preserveAspect = true;
        iconImage.color  = owned ? Color.white : Color.black;
        iconImage.sprite = frames[0];

        outlineImage.enabled = true;
        outlineImage.sprite  = GetOutlineSprite(anyMaxed, allMaxed, legendary);
    }

    public void UpdateFrame(int frameIndex)
    {
        if (frames == null || frames.Length == 0) return;
        iconImage.sprite = frames[frameIndex % frames.Length];
    }

    private Sprite GetOutlineSprite(bool anyMaxed, bool allMaxed, bool legendary)
    {
        if (!anyMaxed)            return defaultOutlineSprite;
        if (legendary)            return allMaxed ? legendaryAllMaxSprite : legendaryAnyMaxSprite;
        return                           allMaxed ? normalAllMaxSprite    : normalAnyMaxSprite;
    }
}
