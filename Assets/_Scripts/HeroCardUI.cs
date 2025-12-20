using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image heroPortrait;
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI descriptionText;
    public Button cardButton; // Nút bao quanh cả thẻ
    public Image borderImage; // Viền để sáng lên khi chọn

    private HeroData myData;
    private HeroSelectionManager manager;

    public void Setup(HeroData data, HeroSelectionManager mng)
    {
        // Kiểm tra Dữ liệu đầu vào
        if (data == null)
        {
            Debug.LogError("LỖI: Dữ liệu HeroData truyền vào bị Null!");
            return;
        }

        // Kiểm tra các biến UI (Nguyên nhân 1)
        if (heroPortrait == null) Debug.LogError("LỖI: Chưa kéo 'Hero Portrait' vào script HeroCardUI trong Prefab!");
        if (heroNameText == null) Debug.LogError("LỖI: Chưa kéo 'Hero Name Text' vào script HeroCardUI trong Prefab!");
        myData = data;
        manager = mng;

        // Điền thông tin
        heroPortrait.sprite = data.portrait;
        heroNameText.text = data.heroName;
        descriptionText.text = $"{data.description}\n<color=yellow>HP: {data.baseHP}</color>";

        // Mặc định tắt viền chọn
        if (borderImage) borderImage.enabled = false;

        // Gắn sự kiện click
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardClicked);
    }

    void OnCardClicked()
    {
        // Báo cho Manager biết là "Tôi được chọn rồi"
        manager.OnHeroSelected(myData, this);
    }

    // Hàm để bật/tắt viền sáng (Visual feedback)
    public void SetSelected(bool isSelected)
    {
        if (borderImage) borderImage.enabled = isSelected;
    }
}