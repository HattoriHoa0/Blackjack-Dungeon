using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("Modules Quản Lý")]
    public DeckManager deckManager;
    public CharacterBase player;
    public CharacterBase enemy;

    [Header("Cấu hình Bài")]
    public GameObject cardPrefab;
    public Sprite cardBackSprite;

    [Header("UI Bàn Chơi")]
    public Transform playerHandArea;
    public Transform dealerHandArea;

    [Header("UI Thông Báo & Cược")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI betAmountText;

    [Header("UI Nút Bấm & Panel")]
    public GameObject bettingPanel;
    public GameObject gameplayPanel;

    public Button hitButton;
    public Button standButton;
    public Button doubleButton;

    // MỚI: List chứa các nút cược để tắt bật
    [Header("Danh sách nút cược (Kéo vào đây)")]
    public Button[] betButtons; // Kéo 5 nút cược (10, 25, 50...) vào đây theo thứ tự
    public int[] betValues;     // Điền giá trị tương ứng (10, 25, 50...) vào đây

    // Các biến cục bộ
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> dealerHand = new List<CardData>();
    private GameObject hiddenCardObject;
    private int currentBet = 0;

    void Start()
    {
        if (player) player.Initialize();
        if (enemy) enemy.Initialize();

        ShowBettingPhase();
    }

    // --- PHA 1: ĐẶT CƯỢC (ĐÃ CẬP NHẬT) ---
    void ShowBettingPhase()
    {
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in dealerHandArea) Destroy(child.gameObject);
        playerHand.Clear();
        dealerHand.Clear();

        if (resultText) resultText.text = "CHỌN MỨC CƯỢC";
        if (scoreText) scoreText.text = "";
        if (betAmountText) betAmountText.text = "DMG: 0";

        if (bettingPanel) bettingPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);

        // MỚI: Kiểm tra từng nút cược, nếu không đủ máu thì khóa lại
        CheckBetButtons();
    }

    // Hàm kiểm tra và khóa nút cược
    void CheckBetButtons()
    {
        if (betButtons == null || betValues == null) return;

        int playerCurrentHP = player.CurrentHP;

        for (int i = 0; i < betButtons.Length; i++)
        {
            if (i < betValues.Length)
            {
                int cost = betValues[i];
                // Nếu máu hiện tại < giá cược -> Khóa nút (Interactable = false)
                if (playerCurrentHP < cost)
                {
                    betButtons[i].interactable = false;
                }
                else
                {
                    betButtons[i].interactable = true;
                }
            }
        }
    }

    public void OnBetSelected(int amount)
    {
        // Kiểm tra lại lần nữa cho chắc (chống hack/lag)
        if (player.CurrentHP < amount)
        {
            Debug.Log("Không đủ máu để cược mức này!");
            return;
        }

        currentBet = amount;
        UpdateBetUI();
        StartRound();
    }

    // --- PHA 2: CHIA BÀI & CHƠI ---
    public void StartRound()
    {
        if (bettingPanel) bettingPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(true);

        if (deckManager != null) deckManager.InitializeDeck();

        hitButton.interactable = true;
        standButton.interactable = true;

        // MỚI: Chỉ cho Double nếu đủ máu trả cược gấp đôi (Tổng cược x2 <= HP hiện tại)
        // Ví dụ: Đang cược 50, muốn Double lên 100 thì phải có >= 100 máu
        if (player.CurrentHP >= currentBet * 2)
        {
            doubleButton.interactable = true;
        }
        else
        {
            doubleButton.interactable = false; // Máu yếu không cho Double
        }

        if (resultText) resultText.text = "VS";

        SpawnCard(playerHand, playerHandArea);
        SpawnCard(dealerHand, dealerHandArea);
        SpawnCard(playerHand, playerHandArea);
        SpawnHiddenCard();

        UpdateScoreUI();

        if (CalculateScore(playerHand) == 21)
        {
            ResolveCombat(false);
        }
    }

    // --- CÁC HÀM XỬ LÝ NÚT BẤM ---

    public void OnHitPressed()
    {
        doubleButton.interactable = false;
        SpawnCard(playerHand, playerHandArea);
        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
    }

    public void OnDoublePressed()
    {
        // Kiểm tra lại lần cuối xem đủ máu không
        if (player.CurrentHP < currentBet * 2) return;

        currentBet *= 2;
        UpdateBetUI();

        SpawnCard(playerHand, playerHandArea);
        UpdateScoreUI();

        hitButton.interactable = false;
        standButton.interactable = false;
        doubleButton.interactable = false;

        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
        else OnStandPressed();
    }

    public void OnStandPressed()
    {
        hitButton.interactable = false;
        standButton.interactable = false;
        doubleButton.interactable = false;

        RevealHoleCard();
        Invoke("DealerTurnLogic", 0.5f);
    }

    void DealerTurnLogic()
    {
        while (CalculateScore(dealerHand) < 17)
        {
            SpawnCard(dealerHand, dealerHandArea);
        }
        ResolveCombat(false);
    }

    // --- PHA 3: TÍNH DAMAGE ---
    void ResolveCombat(bool playerBusted)
    {
        hitButton.interactable = false;
        standButton.interactable = false;
        doubleButton.interactable = false;

        int pScore = CalculateScore(playerHand);
        int dScore = CalculateScore(dealerHand);
        int finalDamage = currentBet;

        bool isBlackjack = (pScore == 21 && playerHand.Count == 2);
        if (isBlackjack) finalDamage = Mathf.RoundToInt(currentBet * 1.5f);

        if (playerBusted)
        {
            player.TakeDamage(currentBet);
            resultText.text = $"QUẮC! NHẬN {currentBet} ST";
        }
        else if (dScore > 21)
        {
            enemy.TakeDamage(finalDamage);
            resultText.text = $"ĐỊCH QUẮC! GÂY {finalDamage} ST";
        }
        else if (pScore > dScore)
        {
            enemy.TakeDamage(finalDamage);
            string msg = isBlackjack ? "BLACKJACK! " : "THẮNG! ";
            resultText.text = $"{msg}GÂY {finalDamage} ST";
        }
        else if (dScore > pScore)
        {
            player.TakeDamage(currentBet);
            resultText.text = $"THUA! NHẬN {currentBet} ST";
        }
        else
        {
            resultText.text = "HÒA! THU HỒI CƯỢC";
        }

        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (player.CurrentHP <= 0)
        {
            resultText.text = "GAME OVER";
        }
        else if (enemy.CurrentHP <= 0)
        {
            resultText.text = "VICTORY";
        }
        else
        {
            Invoke("ShowBettingPhase", 2f);
        }
    }

    // ... (Giữ nguyên các hàm SpawnCard, SpawnHiddenCard, RevealHoleCard, CalculateScore, UpdateScoreUI, UpdateBetUI cũ) ...

    // ----------- BỔ SUNG CÁC HÀM CŨ ĐỂ CODE CHẠY ĐƯỢC (COPY LẠI TỪ BÀI TRƯỚC) ------------
    void SpawnCard(List<CardData> handData, Transform handArea)
    {
        if (deckManager == null) return;
        CardData card = deckManager.GetNextCard();
        if (card == null) return;
        handData.Add(card);
        GameObject newCard = Instantiate(cardPrefab, handArea);
        newCard.GetComponent<Image>().sprite = card.cardImage;
        newCard.GetComponentInChildren<TextMeshProUGUI>().text = card.value.ToString();
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    void SpawnHiddenCard()
    {
        if (deckManager == null) return;
        CardData card = deckManager.GetNextCard();
        if (card == null) return;
        dealerHand.Add(card);
        GameObject newCard = Instantiate(cardPrefab, dealerHandArea);
        hiddenCardObject = newCard;
        newCard.GetComponent<Image>().sprite = cardBackSprite;
        newCard.GetComponentInChildren<TextMeshProUGUI>().text = "";
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    void RevealHoleCard()
    {
        if (hiddenCardObject != null && dealerHand.Count >= 2)
        {
            CardData hiddenData = dealerHand[1];
            hiddenCardObject.transform.DOScale(new Vector3(0, 1, 1), 0.2f).OnComplete(() =>
            {
                hiddenCardObject.GetComponent<Image>().sprite = hiddenData.cardImage;
                hiddenCardObject.GetComponentInChildren<TextMeshProUGUI>().text = hiddenData.value.ToString();
                hiddenCardObject.transform.DOScale(Vector3.one, 0.2f);
            });
        }
    }
    int CalculateScore(List<CardData> hand)
    {
        int score = 0; int aceCount = 0;
        foreach (var card in hand)
        {
            score += card.value;
            if (card.cardName.Contains("Ace") || card.value == 11) aceCount++;
        }
        while (score > 21 && aceCount > 0) { score -= 10; aceCount--; }
        return score;
    }
    void UpdateScoreUI() { if (scoreText) scoreText.text = "Score: " + CalculateScore(playerHand); }
    void UpdateBetUI() { if (betAmountText) betAmountText.text = "DMG: " + currentBet; }
}