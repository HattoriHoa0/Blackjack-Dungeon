using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Modules Quản Lý")]
    public DeckManager playerDeck;
    public DeckManager enemyDeck;
    public CharacterBase player;
    public CharacterBase enemy;

    // [MỚI] Tham chiếu đến ShopManager
    [Header("Shop System")]
    public ShopManager shopManager;
    public InventoryUI inventoryUI;

    [Header("UI Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI endTitleText;

    [Header("Cấu hình Bài")]
    public GameObject cardPrefab;
    public Sprite cardBackSprite;

    [Header("Cấu hình Quái Vật")]
    public EnemyData normalEnemyData;
    public List<EnemyData> eliteEnemiesList;
    public TextMeshProUGUI enemyAbilityText;

    [Header("Cấu hình Buff & Level")]
    public List<BuffData> allBuffsLibrary;
    public GameObject buffCardPrefab;
    public GameObject buffSelectionPanel;
    public Transform buffContainer;
    public int levelsPerBuff = 2;

    [Header("UI Bàn Chơi")]
    public Transform playerHandArea;
    public Transform dealerHandArea;

    [Header("UI Thông Báo")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI enemyScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI betAmountText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI notificationText;

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

    [Header("UI Tooltip")]
    public TooltipTrigger enemyTooltipTrigger;

    [Header("Item System State")]
    public int tempScoreBonus = 0;
    public float tempDamageMultiplier = 1f;
    public int tempBlock = 0;
    public bool hasPlayerHit = false;

    // Các biến cục bộ
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> dealerHand = new List<CardData>();
    private GameObject hiddenCardObject;

    private int currentBet = 0;
    public int currentLevel = 1;
    private bool isDoubleActive = false;

    private EnemyData currentEnemyData;

    // --- CÁC HÀM PUBLIC HELPER ---
    public int GetCurrentBet() { return currentBet; }
    public void UpdateResultText(string msg) { if (resultText) resultText.text = msg; }

    public void SpawnExtraCardForPlayer()
    {
        SpawnCard(playerHand, playerHandArea, playerDeck);
        if (resultText) resultText.text = "TRƯỢT TAY RÚT THÊM!";
        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
    }
    // -----------------------------------------------------

    void Start()
    {
        if (player) player.Initialize();
        SetupEnemyForLevel();
        if (buffSelectionPanel) buffSelectionPanel.SetActive(false);
        ShowBettingPhase();
        if (notificationText) notificationText.alpha = 0f;
    }

    void SetupEnemyForLevel()
    {
        if (playerDeck) playerDeck.InitializeDeck();
        if (enemyDeck) enemyDeck.InitializeDeck();

        if (currentLevel % 3 == 0)
            currentEnemyData = eliteEnemiesList[Random.Range(0, eliteEnemiesList.Count)];
        else
            currentEnemyData = normalEnemyData;

        float hpMultiplier = Mathf.Pow(1.1f, currentLevel - 1);
        int scaledHP = Mathf.RoundToInt(currentEnemyData.baseHP * hpMultiplier);

        enemy.Initialize(scaledHP);
        enemy.SetupVisuals(currentEnemyData.enemyName, currentEnemyData.portrait);

        if (player.activeBuffs != null)
        {
            foreach (var runtimeBuff in player.activeBuffs)
            {
                runtimeBuff.data.OnLevelStart(this, runtimeBuff.level);
            }
        }

        string abilityDesc = (currentEnemyData.abilityLogic != null) ?
            currentEnemyData.abilityLogic.description : "Quái vật thông thường.\nTăng máu theo cấp độ.";

        if (enemyTooltipTrigger != null)
        {
            enemyTooltipTrigger.header = "";
            enemyTooltipTrigger.content = abilityDesc;
        }

        if (enemyAbilityText)
            enemyAbilityText.text = (currentEnemyData.abilityLogic != null) ?
                $"<color=red>⚠️ {abilityDesc}</color>" : "";
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
        if (enemyScoreText) enemyScoreText.text = "";

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
        currentBet = 0; UpdateBetUI(); CheckChipButtons(); dealButton.interactable = false;
    }

    public void OnDealPressed() { if (currentBet > 0) StartRound(); }

    // --- PHA 2: CHIA BÀI & CHƠI ---
    public void StartRound()
    {
        if (bettingPanel) bettingPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(true);

        if (playerDeck.GetCardsRemaining() == 0) playerDeck.InitializeDeck();
        if (enemyDeck.GetCardsRemaining() == 0) enemyDeck.InitializeDeck();

        hitButton.interactable = true;
        standButton.interactable = true;
        doubleButton.interactable = (player.CurrentHP >= currentBet * 2);

        if (resultText) resultText.text = "VS";

        // Reset Item State
        tempScoreBonus = 0;
        tempDamageMultiplier = 1f;
        tempBlock = 0;
        hasPlayerHit = false;

        if (currentEnemyData.abilityLogic != null)
        {
            currentEnemyData.abilityLogic.OnRoundStart(this);
        }

        SpawnCard(playerHand, playerHandArea, playerDeck);
        SpawnCard(dealerHand, dealerHandArea, enemyDeck);
        SpawnCard(playerHand, playerHandArea, playerDeck);
        SpawnHiddenCard();

        UpdateScoreUI();
        UpdateDealerScoreUI(false);

        if (CalculateScore(playerHand) == 21)
        {
            RevealHoleCard();
            ResolveCombat(false);
        }
    }

    public void OnHitPressed()
    {
        hasPlayerHit = true;
        doubleButton.interactable = false;
        SpawnCard(playerHand, playerHandArea, playerDeck);

        if (currentEnemyData.abilityLogic != null)
        {
            currentEnemyData.abilityLogic.OnPlayerHit(this);
        }

        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
    }

    public void OnDoublePressed()
    {
        if (player.CurrentHP < currentBet * 2) return;
        hasPlayerHit = true;

        currentBet *= 2; isDoubleActive = true; UpdateBetUI();
        hitButton.interactable = false; standButton.interactable = false; doubleButton.interactable = false;

        SpawnCard(playerHand, playerHandArea, playerDeck);

        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) Invoke("DoDoubleBust", 0.5f); else Invoke("OnStandPressed", 1f);
    }
    void DoDoubleBust() { ResolveCombat(true); }

    public void OnStandPressed()
    {
        hasPlayerHit = true;
        hitButton.interactable = false; standButton.interactable = false; doubleButton.interactable = false;
        RevealHoleCard();
        Invoke("DealerTurnLogic", 0.5f);
    }

    void DealerTurnLogic()
    {
        if (currentEnemyData.abilityLogic != null)
        {
            if (currentEnemyData.abilityLogic.OnDealerTurnStop(this))
            {
                ResolveCombat(false);
                return;
            }
        }

        int safety = 0;
        while (CalculateScore(dealerHand) < 17 && safety < 20)
        {
            if (enemyDeck.GetCardsRemaining() <= 0) break;
            SpawnCard(dealerHand, dealerHandArea, enemyDeck);
            UpdateDealerScoreUI(true);
            safety++;
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

        if (playerBusted) enemyWin = true;
        else if (dScore > 21) playerWin = true;
        else if (pScore > dScore) playerWin = true;
        else if (dScore > pScore) enemyWin = true;

        if (playerBlackjack && playerWin) baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);

        if (playerWin)
        {
            finalDamage = player.CalculateFinalOutgoingDamage(baseDamage, pScore, isDoubleActive);

            if (tempDamageMultiplier > 1f)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * tempDamageMultiplier);
            }

            if (player.activeBuffs != null)
            {
                foreach (var runtimeBuff in player.activeBuffs)
                {
                    finalDamage = runtimeBuff.data.ModifyPlayerDamage(finalDamage, this, runtimeBuff.level);
                }
            }

            if (currentEnemyData.abilityLogic != null)
            {
                finalDamage = currentEnemyData.abilityLogic.OnCalculateDamage(finalDamage, pScore);
            }

            if (enemyScoreText && currentEnemyData.abilityLogic != null && pScore <= 16 && currentEnemyData.abilityLogic.name.Contains("Iron"))
            {
                resultText.text = $"QUÁI ĐỠ ĐÒN! CHỈ GÂY {finalDamage} ST";
            }
            else
            {
                string winMsg = playerBlackjack ? "<color=yellow>BLACKJACK!</color> " : "THẮNG! ";
                string rageMsg = (tempDamageMultiplier > 1f) ? $" (x{tempDamageMultiplier}) " : "";
                resultText.text = $"{winMsg}GÂY {finalDamage}{rageMsg} ST";
            }

            enemy.TakeDamage(finalDamage);

        }
        else if (enemyWin)
        {
            finalDamage = player.CalculateFinalIncomingDamage(currentBet, dScore);

            if (tempDamageMultiplier > 1f)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * tempDamageMultiplier);
                UpdateResultText($"PHẢN TÁC DỤNG! (NHẬN x{tempDamageMultiplier} ST)");
            }

            if (tempBlock > 0)
            {
                int blocked = Mathf.Min(finalDamage, tempBlock);
                finalDamage -= blocked;
                if (finalDamage < 0) finalDamage = 0;
                resultText.text = $"THUA! KHIÊN ĐỠ {blocked}, NHẬN {finalDamage} ST";
            }
            else
            {
                resultText.text = $"THUA! NHẬN {finalDamage} SÁT THƯƠNG";
            }

            player.TakeDamage(finalDamage);
        }
        else
        {
            resultText.text = "HÒA! THU HỒI CƯỢC";
        }

        if (playerDeck) playerDeck.AddToDiscardPile(playerHand);
        if (enemyDeck) enemyDeck.AddToDiscardPile(dealerHand);

        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (player.CurrentHP <= 0)
        {
            GameOver("BẠN THUA RỒI!");
        }
        else if (enemy.CurrentHP <= 0)
        {
            // [MỚI] CỘNG TIỀN Ở ĐÂY (Chỉ khi quái chết mới được tiền)
            int goldReward = (currentLevel % 3 == 0) ? 80 : 50;
            player.AddGold(goldReward);

            // Cập nhật thông báo cho người chơi sướng
            UpdateResultText($"ĐỊCH ĐÃ BẠI! (+{goldReward} Vàng)");

            // Gọi hàm chuyển cảnh (có delay 1 giây để người chơi kịp đọc thông báo trên)
            HandleLevelComplete();
        }
        else
        {
            Invoke("ShowBettingPhase", 2f);
        }
    }

    void HandleLevelComplete()
    {
        // [MỚI] Logic hiển thị: Win -> (Buff?) -> Shop -> Next Level
        if (currentLevel % levelsPerBuff == 0)
        {
            // Nếu có Buff: Chọn buff xong sẽ mở Shop (xem hàm OnBuffSelected)
            Invoke("ShowBuffSelection", 1f);
        }
        else
        {
            // Nếu không có Buff: Mở Shop luôn
            Invoke("ShowShop", 1f);
        }
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

    void OnBuffSelected(BuffData buff)
    {
        if (buff != null) player.AddOrUpgradeBuff(buff);
        buffSelectionPanel.SetActive(false);

        // [THAY ĐỔI] Thay vì vào level mới ngay, ta mở Shop
        ShowShop();
    }

    // [MỚI] Hàm mở Shop
    void ShowShop()
    {
        if (gameplayPanel) gameplayPanel.SetActive(false);
        if (shopManager) shopManager.OpenShop();
    }

    // [MỚI] Hàm này được ShopManager gọi khi đóng shop để tiếp tục chơi
    public void ProceedToNextLevel()
    {
        currentLevel++;
        SetupEnemyForLevel();
        ShowBettingPhase();
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

    void SpawnCard(List<CardData> handData, Transform handArea, DeckManager sourceDeck)
    {
        if (sourceDeck == null) return;
        CardData card = sourceDeck.GetNextCard();
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
        if (enemyDeck == null) return;
        CardData card = enemyDeck.GetNextCard();
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
                UpdateDealerScoreUI(true);
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

        if (hand == playerHand)
        {
            score += tempScoreBonus;
        }

        while (score > 21 && aceCount > 0) { score -= 10; aceCount--; }
        return score;
    }

    void UpdateScoreUI()
    {
        if (scoreText)
        {
            int score = CalculateScore(playerHand);
            string bonusText = "";
            if (tempScoreBonus > 0) bonusText = $" <color=green>(+{tempScoreBonus})</color>";
            else if (tempScoreBonus < 0) bonusText = $" <color=red>({tempScoreBonus})</color>";

            scoreText.text = "Bạn: " + score + bonusText;
        }
    }

    void UpdateBetUI() { if (betAmountText) betAmountText.text = "DMG: " + currentBet; }
    void UpdateDealerScoreUI(bool isRevealed)
    {
        if (enemyScoreText == null) return;
        if (isRevealed) enemyScoreText.text = "Địch: " + CalculateScore(dealerHand);
        else if (dealerHand.Count > 0) enemyScoreText.text = "Địch: " + dealerHand[0].value + " + ?";
        else enemyScoreText.text = "";
    }

    void GameOver(string title)
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            if (endTitleText) endTitleText.text = title;
        }
        if (gameplayPanel) gameplayPanel.SetActive(false);
        if (bettingPanel) bettingPanel.SetActive(false);
    }

    public void OnRetryPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ========================================================================
    // --- HỆ THỐNG ITEM API ---
    // ========================================================================

    // [MỚI] Hàm cầu nối để ItemSlot trong shop gọi sang ShopManager
    public void TryBuyItemFromShop(ItemData item, ItemSlot slot)
    {
        if (shopManager) shopManager.TryBuyItem(item, slot);
    }

    public void TryUseItem(ItemData item)
    {
        if (gameplayPanel.activeSelf == false) return;

        if (hasPlayerHit)
        {
            // [SỬA] Dùng hàm mới
            ShowNotification("ĐÃ RÚT BÀI! KHÔNG THỂ DÙNG!");
            return;
        }

        if (hitButton.interactable == false && standButton.interactable == false)
        {
            // [SỬA] Dùng hàm mới
            ShowNotification("KHÔNG PHẢI LƯỢT CỦA BẠN!");
            return;
        }

        if (item.OnUse(this))
        {
            Debug.Log($"Đã dùng {item.itemName}");
            player.RemoveItem(item);
            if (inventoryUI) inventoryUI.RefreshInventoryUI();
        }
        else
        {
            // [SỬA] Dùng hàm mới
            ShowNotification("KHÔNG CẦN DÙNG LÚC NÀY!");
        }
    }

    public void AddTempScoreBonus(int amount)
    {
        tempScoreBonus += amount;
        UpdateScoreUI();
        string msg = (amount > 0) ? "UỐNG THUỐC TĂNG TRƯỞNG (+3)" : "UỐNG THUỐC LINH HOẠT (-3)";
        ShowNotification(msg); // [ĐÃ SỬA] Dùng ShowNotification
    }

    public void MultiplyTempDamage(float multiplier)
    {
        tempDamageMultiplier *= multiplier;
        ShowNotification($"RAGE MODE: DMG x{tempDamageMultiplier}"); // [ĐÃ SỬA]
    }

    public void AddTempBlock(int amount)
    {
        tempBlock += amount;
        ShowNotification($"ĐÃ BẬT KHIÊN ({amount} GIÁP)"); // [ĐÃ SỬA]
    }

    public CardData PeekNextCard()
    {
        if (playerDeck.drawPile.Count > 0) return playerDeck.drawPile[0];
        return null;
    }

    public void ShowSpyglassUI(CardData card)
    {
        // [ĐÃ SỬA] Hiện thông báo lá bài tiếp theo
        // Lưu ý: Nếu card.value là 11/12/13 bạn có thể muốn hiển thị J/Q/K/A cho đẹp hơn
        ShowNotification($"SOI THẤY LÁ: {card.value}");
    }

    public void UseTimeClockItem()
    {
        playerDeck.AddToDiscardPile(playerHand);

        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        playerHand.Clear();

        SpawnCard(playerHand, playerHandArea, playerDeck);
        SpawnCard(playerHand, playerHandArea, playerDeck);

        UpdateScoreUI();
        ShowNotification("ĐẢO NGƯỢC THỜI GIAN!"); // [ĐÃ SỬA]
    }

    // Hàm hiện thông báo lỗi (Tự động biến mất sau 1.5 giây)
    public void ShowNotification(string msg)
    {
        if (notificationText == null) return;

        notificationText.text = msg;
        notificationText.alpha = 1f; // Hiện lên
        notificationText.transform.localScale = Vector3.one;

        // Reset các hiệu ứng cũ đang chạy dở
        notificationText.DOKill();

        // Hiệu ứng: Rung nhẹ 1 cái -> Chờ 1s -> Mờ dần đi
        notificationText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        notificationText.DOFade(0f, 1f).SetDelay(1f);
    }
}