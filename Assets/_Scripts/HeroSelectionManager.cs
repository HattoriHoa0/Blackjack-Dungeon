using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HeroSelectionManager : MonoBehaviour
{
    [Header("Data")]
    public List<HeroData> allHeroes; // Kéo 3 file Hero vào đây

    [Header("UI References")]
    public Transform cardsContainer;  // Nơi chứa các thẻ
    public GameObject heroCardPrefab; // Prefab thẻ tướng
    public TextMeshProUGUI feedbackText; // Dòng chữ "Đã chọn Hero..."
    public Button startGameButton;    // Nút xác nhận vào game

    private HeroData currentSelectedHero;
    private List<HeroCardUI> spawnedCards = new List<HeroCardUI>();

    void Start()
    {
        // 1. Reset UI
        feedbackText.text = "Vui lòng chọn Tướng";
        startGameButton.interactable = false; // Chưa chọn thì chưa cho bấm
        SpawnCards();

        // 2. Gắn sự kiện cho nút Start Game
        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    void SpawnCards()
    {
        // Xóa cũ (nếu có)
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);
        spawnedCards.Clear();

        // Tạo mới
        foreach (HeroData hero in allHeroes)
        {
            GameObject cardObj = Instantiate(heroCardPrefab, cardsContainer);
            HeroCardUI cardUI = cardObj.GetComponent<HeroCardUI>();
            cardUI.Setup(hero, this);
            spawnedCards.Add(cardUI);
        }
    }

    // Hàm này được gọi khi 1 thẻ tướng bị bấm
    public void OnHeroSelected(HeroData hero, HeroCardUI uiScript)
    {
        currentSelectedHero = hero;

        // Cập nhật Text thông báo
        feedbackText.text = $"Đã chọn Hero: <color=yellow>{hero.heroName}</color>";

        // Bật nút Start Game
        startGameButton.interactable = true;

        // Visual: Tắt viền tất cả, chỉ bật viền cái được chọn
        foreach (var card in spawnedCards) card.SetSelected(false);
        uiScript.SetSelected(true);
    }

    void OnStartGameClicked()
    {
        if (currentSelectedHero == null) return;

        // --- LƯU VÀO DATA HOLDER ---
        if (GameDataHolder.Instance != null)
        {
            GameDataHolder.Instance.selectedHero = currentSelectedHero;
        }

        // Chuyển cảnh
        SceneManager.LoadScene("SampleScene"); // Thay tên Scene game của bạn
    }
}