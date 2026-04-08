using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DexSlotUI : MonoBehaviour
{
    public Image iconImage;
    public GameObject outline;

    private Coroutine loopCoroutine;

    public void Setup(int id)
    {
        bool owned = PokedexSaveManager.Instance != null &&
                     PokedexSaveManager.Instance.IsOwned(id);

        bool maxed = PokedexSaveManager.Instance != null &&
                     PokedexSaveManager.Instance.IsMaxEnhanced(id);

        string iconPath = "Icon/" + id.ToString("D3");
        Sprite[] frames = Resources.LoadAll<Sprite>(iconPath);

        if (frames.Length == 0)
        {
            iconImage.sprite = null;
            return;
        }

        iconImage.preserveAspect = true;

        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        loopCoroutine = StartCoroutine(PlayLoop(frames));

        // ⭐ 등록 안 된 건 검정으로
        iconImage.color = owned ? Color.white : Color.black;

        outline.SetActive(maxed);
    }

    IEnumerator PlayLoop(Sprite[] frames)
    {
        int index = 0;

        while (true)
        {
            iconImage.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(0.4f);
        }
    }
}