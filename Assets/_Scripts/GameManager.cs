using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Modules Quản Lý")]
    public DeckManager deckManager;
    public CharacterBase player;
    public CharacterBase enemy;
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
    public int levelsPerBuff = 1;

    [Header("UI Bàn Chơi")]
    public Transform playerHandArea;
    public Transform dealerHandArea;

    [Header("UI Thông Báo")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI enemyScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI betAmountText;
    public TextMeshProUGUI levelText;

    [Header("UI Nút Bấm & Panel")]
    public GameObject bettingPanel;
    public GameObject gameplayPanel;

    // Để public để các Ability Script có thể can thiệp
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

    // Các biến cục bộ
    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> dealerHand = new List<CardData>();
    private GameObject hiddenCardObject;

    private int currentBet = 0;
    public int currentLevel = 1; // Đổi thành public để Buff truy cập được Level
    private bool isDoubleActive = false;

    private EnemyData currentEnemyData;

    // --- CÁC HÀM PUBLIC HELPER (Cho Ability Script gọi) ---
    public int GetCurrentBet() { return currentBet; }
    public void UpdateResultText(string msg) { if (resultText) resultText.text = msg; }

    public void SpawnExtraCardForPlayer()
    {
        SpawnCard(playerHand, playerHandArea);
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
    }

    void SetupEnemyForLevel()
    {
        // 1. Chọn Quái
        if (currentLevel % 3 == 0)
            currentEnemyData = eliteEnemiesList[Random.Range(0, eliteEnemiesList.Count)];
        else
            currentEnemyData = normalEnemyData;

        // 2. Tính Máu
        float hpMultiplier = Mathf.Pow(1.1f, currentLevel - 1);
        int scaledHP = Mathf.RoundToInt(currentEnemyData.baseHP * hpMultiplier);

        // 3. Setup Visuals
        enemy.Initialize(scaledHP);
        enemy.SetupVisuals(currentEnemyData.enemyName, currentEnemyData.portrait);

        // --- [MỚI] HOOK BUFF: KHI BẮT ĐẦU LEVEL (Cho Buff "Chuẩn bị kĩ lưỡng") ---
        // Lưu ý: Cần đảm bảo CharacterBase có biến public activeBuffs
        if (player.activeBuffs != null)
        {
            foreach (var runtimeBuff in player.activeBuffs)
            {
                // PHẢI GỌI QUA .data VÌ activeBuffs LÀ LIST<RuntimeBuff>
                runtimeBuff.data.OnLevelStart(this, runtimeBuff.level);
            }
        }
        // ------------------------------------------------------------------------

        // 4. Setup Tooltip & Text
        string abilityDesc = "";

        if (currentEnemyData.abilityLogic != null)
        {
            abilityDesc = currentEnemyData.abilityLogic.description;
        }
        else
        {
            abilityDesc = "Quái vật thông thường.\nTăng máu theo cấp độ.";
        }

        if (enemyTooltipTrigger != null)
        {
            enemyTooltipTrigger.header = ""; // Ẩn tên quái đi cho đỡ lặp
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

        if (deckManager != null) deckManager.InitializeDeck();

        hitButton.interactable = true;
        standButton.interactable = true;
        doubleButton.interactable = (player.CurrentHP >= currentBet * 2);

        if (resultText) resultText.text = "VS";

        // --- HOOK ENEMY: ROUND START ---
        if (currentEnemyData.abilityLogic != null)
        {
            currentEnemyData.abilityLogic.OnRoundStart(this);
        }
        // -------------------------------

        SpawnCard(playerHand, playerHandArea);
        SpawnCard(dealerHand, dealerHandArea);
        SpawnCard(playerHand, playerHandArea);
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
        doubleButton.interactable = false;
        SpawnCard(playerHand, playerHandArea);

        // --- HOOK ENEMY: PLAYER HIT ---
        if (currentEnemyData.abilityLogic != null)
        {
            currentEnemyData.abilityLogic.OnPlayerHit(this);
        }
        // -----------------------------

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
        // --- HOOK ENEMY: DEALER STOP ---
        if (currentEnemyData.abilityLogic != null)
        {
            if (currentEnemyData.abilityLogic.OnDealerTurnStop(this))
            {
                ResolveCombat(false);
                return;
            }
        }
        // -----------------------------

        // --- [MỚI] SỬA LẠI VÒNG LẶP AN TOÀN TRÁNH CRASH ---
        int safety = 0;
        while (CalculateScore(dealerHand) < 17 && safety < 20)
        {
            if (deckManager.GetCardsRemaining() <= 0) break; // Check hết bài
            SpawnCard(dealerHand, dealerHandArea);
            UpdateDealerScoreUI(true);
            safety++;
        }
        // --------------------------------------------------

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

            // --- [MỚI] HOOK BUFF: TÍNH LẠI DAMAGE (Cho Buff "Trúng hay hụt") ---
            if (player.activeBuffs != null)
            {
                foreach (var runtimeBuff in player.activeBuffs)
                {
                    // PHẢI GỌI QUA .data
                    finalDamage = runtimeBuff.data.ModifyPlayerDamage(finalDamage, this, runtimeBuff.level);
                }
            }
            // ------------------------------------------------------------------

            // --- HOOK ENEMY: GIẢM DAMAGE (Iron Defense) ---
            if (currentEnemyData.abilityLogic != null)
            {
                finalDamage = currentEnemyData.abilityLogic.OnCalculateDamage(finalDamage, pScore);
            }
            // ---------------------------------------------

            if (enemyScoreText && currentEnemyData.abilityLogic != null && pScore <= 16 && currentEnemyData.abilityLogic.name.Contains("Iron"))
            {
                // Đoạn này check tên logic hơi hardcode, có thể cải thiện sau
                resultText.text = $"QUÁI ĐỠ ĐÒN! CHỈ GÂY {finalDamage} ST";
            }
            else
            {
                string winMsg = playerBlackjack ? "<color=yellow>BLACKJACK!</color> " : "THẮNG! ";
                resultText.text = $"{winMsg}GÂY {finalDamage} SÁT THƯƠNG";
            }

            enemy.TakeDamage(finalDamage);
            player.AddGold(10);
        }
        else if (enemyWin)
        {
            finalDamage = player.CalculateFinalIncomingDamage(currentBet, dScore);
            player.TakeDamage(finalDamage);
            resultText.text = $"THUA! NHẬN {finalDamage} SÁT THƯƠNG";
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
            // Thua -> Hiện bảng Game Over
            GameOver("BẠN THUA RỒI!");
        }
        else if (enemy.CurrentHP <= 0)
        {
            HandleLevelComplete();
        }
        else
        {
            // Chưa ai chết -> Tiếp tục chơi
            Invoke("ShowBettingPhase", 2f);
        }
    }

    void HandleLevelComplete()
    {
        if (currentLevel % levelsPerBuff == 0) Invoke("ShowBuffSelection", 1f);
        else
        {
            currentLevel++;
            SetupEnemyForLevel();
            Invoke("ShowBettingPhase", 2f);
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
        currentLevel++;
        SetupEnemyForLevel();
        ShowBettingPhase();
    }

    // --- CÁC HÀM CƠ BẢN (SPAWN, TÍNH ĐIỂM) ---
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
        while (score > 21 && aceCount > 0) { score -= 10; aceCount--; }
        return score;
    }

    void UpdateScoreUI() { if (scoreText) scoreText.text = "Bạn: " + CalculateScore(playerHand); }
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
            gameOverPanel.SetActive(true); // Bật bảng lên
            if (endTitleText) endTitleText.text = title;
        }

        // Tắt các panel khác để đỡ rối
        if (gameplayPanel) gameplayPanel.SetActive(false);
        if (bettingPanel) bettingPanel.SetActive(false);
    }

    // Gắn vào nút "CHƠI LẠI"
    public void OnRetryPressed()
    {
        // Load lại màn chơi hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Gắn vào nút "VỀ MENU"
    public void OnMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}