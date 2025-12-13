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

    [Header("Cấu hình Buff & Level")]
    public List<BuffData> allBuffsLibrary;
    public GameObject buffCardPrefab;
    public GameObject buffSelectionPanel;
    public Transform buffContainer;
    public int levelsPerBuff = 1;

    [Header("UI Bàn Chơi")]
    public Transform playerHandArea;
    public Transform dealerHandArea;

    [Header("UI Thông Báo")]
    public TextMeshProUGUI scoreText;      // Điểm người chơi
    public TextMeshProUGUI enemyScoreText; // MỚI: Điểm kẻ địch
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI betAmountText;
    public TextMeshProUGUI levelText;

    [Header("UI Nút Bấm & Panel")]
    public GameObject bettingPanel;
    public GameObject gameplayPanel;

    public Button hitButton;
    public Button standButton;
    public Button doubleButton;

    [Header("Nút chức năng Cược")]
    public Button dealButton;
    public Button clearButton;

    [Header("Danh sách nút Phỉnh")]
    public Button[] betButtons;
    public int[] betValues;

    // Các biến cục bộ
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> dealerHand = new List<CardData>();
    private GameObject hiddenCardObject;

    private int currentBet = 0;
    private int currentLevel = 1;
    private bool isDoubleActive = false;

    void Start()
    {
        if (player) player.Initialize();
        if (enemy) enemy.Initialize();
        if (buffSelectionPanel) buffSelectionPanel.SetActive(false);

        ShowBettingPhase();
    }

    // --- PHA 1: ĐẶT CƯỢC ---
    void ShowBettingPhase()
    {
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in dealerHandArea) Destroy(child.gameObject);
        playerHand.Clear();
        dealerHand.Clear();

        if (resultText) resultText.text = "";
        if (levelText) levelText.text = "LEVEL " + currentLevel;
        if (scoreText) scoreText.text = "";
        if (enemyScoreText) enemyScoreText.text = ""; // Reset điểm địch

        currentBet = 0;
        UpdateBetUI();

        if (bettingPanel) bettingPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);

        isDoubleActive = false;
        dealButton.interactable = false;
        clearButton.interactable = true;
        CheckChipButtons();
    }

    void CheckChipButtons()
    {
        if (betButtons == null || betValues == null) return;
        int playerHP = player.CurrentHP;
        for (int i = 0; i < betButtons.Length; i++)
        {
            if (i < betValues.Length)
            {
                int potentialBet = currentBet + betValues[i];
                betButtons[i].interactable = (potentialBet <= playerHP);
            }
        }
    }

    public void OnChipSelected(int amount)
    {
        if (player.CurrentHP < currentBet + amount) return;
        currentBet += amount;
        betAmountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f);
        UpdateBetUI();
        CheckChipButtons();
        if (currentBet > 0) dealButton.interactable = true;
    }

    public void OnClearBetPressed()
    {
        currentBet = 0;
        UpdateBetUI();
        CheckChipButtons();
        dealButton.interactable = false;
    }

    public void OnDealPressed()
    {
        if (currentBet <= 0) return;
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
        doubleButton.interactable = (player.CurrentHP >= currentBet * 2);

        if (resultText) resultText.text = "VS";

        SpawnCard(playerHand, playerHandArea);
        SpawnCard(dealerHand, dealerHandArea);
        SpawnCard(playerHand, playerHandArea);
        SpawnHiddenCard();

        UpdateScoreUI();
        UpdateDealerScoreUI(false); // Chưa lật bài -> false

        // Check Blackjack
        if (CalculateScore(playerHand) == 21)
        {
            int dealerScore = CalculateScore(dealerHand);
            if (dealerScore == 21)
            {
                RevealHoleCard();
                ResolveCombat(false);
            }
            else
            {
                RevealHoleCard();
                ResolveCombat(false);
            }
        }
    }

    public void OnHitPressed()
    {
        doubleButton.interactable = false;
        SpawnCard(playerHand, playerHandArea);
        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
    }

    public void OnDoublePressed()
    {
        if (player.CurrentHP < currentBet * 2) return;
        currentBet *= 2; isDoubleActive = true; UpdateBetUI();
        hitButton.interactable = false; standButton.interactable = false; doubleButton.interactable = false;
        SpawnCard(playerHand, playerHandArea); UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) Invoke("DoDoubleBust", 0.5f); else Invoke("OnStandPressed", 1f);
    }
    void DoDoubleBust() { ResolveCombat(true); }

    public void OnStandPressed()
    {
        hitButton.interactable = false; standButton.interactable = false; doubleButton.interactable = false;
        RevealHoleCard();
        Invoke("DealerTurnLogic", 0.5f);
    }

    void DealerTurnLogic()
    {
        while (CalculateScore(dealerHand) < 17)
        {
            SpawnCard(dealerHand, dealerHandArea);
            UpdateDealerScoreUI(true); // Đã lật bài -> true, cập nhật liên tục khi rút
        }
        ResolveCombat(false);
    }

    // --- PHA 3: TÍNH DAMAGE ---
    void ResolveCombat(bool playerBusted)
    {
        int pScore = CalculateScore(playerHand);
        int dScore = CalculateScore(dealerHand);
        int baseDamage = currentBet;
        int finalDamage = 0;

        bool playerWin = false;
        bool enemyWin = false;

        bool playerBlackjack = (pScore == 21 && playerHand.Count == 2);
        bool dealerBlackjack = (dScore == 21 && dealerHand.Count == 2);

        if (playerBlackjack && dealerBlackjack) { /* Hòa */ }
        else if (playerBlackjack) { playerWin = true; baseDamage = Mathf.RoundToInt(baseDamage * 1.5f); }
        else if (playerBusted) enemyWin = true;
        else if (dScore > 21) playerWin = true;
        else if (pScore > dScore) playerWin = true;
        else if (dScore > pScore) enemyWin = true;

        if (playerWin)
        {
            finalDamage = player.CalculateFinalOutgoingDamage(baseDamage, pScore, isDoubleActive);
            enemy.TakeDamage(finalDamage);
            string winMsg = playerBlackjack ? "<color=yellow>BLACKJACK!</color> " : "THẮNG! ";
            resultText.text = $"{winMsg}SÁT THƯƠNG {finalDamage}";
            player.AddGold(10);
        }
        else if (enemyWin)
        {
            finalDamage = player.CalculateFinalIncomingDamage(currentBet, dScore);
            player.TakeDamage(finalDamage);
            resultText.text = $"THUA! NHẬN {finalDamage} ST";
        }
        else
        {
            string tieMsg = (playerBlackjack && dealerBlackjack) ? "HÒA (CÙNG BLACKJACK)!" : "HÒA!";
            resultText.text = $"{tieMsg} THU HỒI CƯỢC";
        }

        CheckWinCondition();
    }

    // ... (Giữ nguyên CheckWinCondition, HandleLevelComplete, Buff System...) ...
    void CheckWinCondition()
    {
        if (player.CurrentHP <= 0) resultText.text = "GAME OVER";
        else if (enemy.CurrentHP <= 0) HandleLevelComplete();
        else Invoke("ShowBettingPhase", 2f);
    }
    void HandleLevelComplete()
    {
        enemy.Initialize();
        if (currentLevel % levelsPerBuff == 0) Invoke("ShowBuffSelection", 1f);
        else { currentLevel++; Invoke("ShowBettingPhase", 2f); }
    }
    void ShowBuffSelection()
    {
        if (buffSelectionPanel) buffSelectionPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);
        foreach (Transform child in buffContainer) Destroy(child.gameObject);
        List<BuffData> validBuffs = new List<BuffData>();
        foreach (var buff in allBuffsLibrary) { if (player.GetBuffLevel(buff) < 3) validBuffs.Add(buff); }
        int cardsToSpawn = Mathf.Min(3, validBuffs.Count);
        for (int i = 0; i < cardsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, validBuffs.Count);
            BuffData selectedBuff = validBuffs[randomIndex];
            validBuffs.RemoveAt(randomIndex);
            CreateBuffCardUI(selectedBuff);
        }
        if (cardsToSpawn == 0) OnBuffSelected(null);
    }
    void CreateBuffCardUI(BuffData buff)
    {
        GameObject cardObj = Instantiate(buffCardPrefab, buffContainer);
        int nextLvl = player.GetBuffLevel(buff) + 1;
        TextMeshProUGUI[] texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 1)
        {
            string title = buff.buffName;
            if (nextLvl > 1) title += " " + nextLvl;
            texts[0].text = title;
        }
        if (texts.Length >= 2) texts[1].text = buff.GetDescription(nextLvl);
        Image[] images = cardObj.GetComponentsInChildren<Image>();
        if (images.Length >= 2) images[1].sprite = buff.icon;
        Button btn = cardObj.GetComponentInChildren<Button>();
        if (btn) btn.onClick.AddListener(() => OnBuffSelected(buff));
    }
    void OnBuffSelected(BuffData buff)
    {
        if (buff != null) player.AddOrUpgradeBuff(buff);
        buffSelectionPanel.SetActive(false);
        currentLevel++;
        ShowBettingPhase();
    }

    // --- HÀM TÍNH ĐIỂM & SPAWN BÀI ---

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

    void UpdateScoreUI() { if (scoreText) scoreText.text = "Bạn: " + CalculateScore(playerHand); }
    void UpdateBetUI() { if (betAmountText) betAmountText.text = "DMG: " + currentBet; }

    // MỚI: Hàm cập nhật điểm Địch
    void UpdateDealerScoreUI(bool isRevealed)
    {
        if (enemyScoreText == null) return;

        if (isRevealed)
        {
            // Nếu đã lật: Hiện tổng điểm
            enemyScoreText.text = "Địch: " + CalculateScore(dealerHand);
        }
        else
        {
            // Nếu chưa lật: Chỉ hiện điểm lá đầu tiên (nếu có)
            if (dealerHand.Count > 0)
            {
                int visibleScore = dealerHand[0].value;
                // Xử lý riêng: Nếu lá ngửa là Ace (11) thì hiển thị là 11
                enemyScoreText.text = "Địch: " + visibleScore + " + ?";
            }
            else
            {
                enemyScoreText.text = "";
            }
        }
    }

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

                // MỚI: Lật xong thì cập nhật lại điểm full
                UpdateDealerScoreUI(true);
            });
        }
    }
}