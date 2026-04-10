using UnityEngine;
using UnityEngine.UI;

public class DexSlotUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject outline;

    private Sprite[] frames;

    public void Setup(int id, Sprite[] cachedFrames)
    {
        frames = cachedFrames;

        if (frames == null || frames.Length == 0)
        {
            iconImage.sprite = null;
            outline.SetActive(false);
            return;
        }

        bool owned = PokedexSaveManager.Instance != null &&
                     PokedexSaveManager.Instance.IsOwned(id);

        bool maxed = PokedexSaveManager.Instance != null &&
                     PokedexSaveManager.Instance.IsMaxEnhanced(id);

        iconImage.preserveAspect = true;
        iconImage.color = owned ? Color.white : Color.black;
        iconImage.sprite = frames[0];

        outline.SetActive(maxed);
    }

    public void UpdateFrame(int frameIndex)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        iconImage.sprite = frames[frameIndex % frames.Length];
    }
}
