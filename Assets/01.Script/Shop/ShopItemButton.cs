using UnityEngine;
using UnityEngine.UI;

// 버튼 프리팹에 부착. 자신의 ShopItemData를 들고 있다가 클릭 시 컨트롤러에 전달.
[RequireComponent(typeof(Button))]
public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private ShopItemData itemData;
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);

        // 인스펙터에서 연결 안 됐으면 자식에서 자동 탐색
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
        }
    }

    // 동적 생성 시 외부에서 데이터 주입
    public void SetData(ShopItemData data)
    {
        itemData = data;
        if (iconImage != null && data != null)
        {
            iconImage.sprite = data.icon;
        }
    }

    private void OnClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopSelect();
        }

        ShopInfoPageController.Instance.ToggleInfo(itemData);
    }
}
