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

    [Header("Elite States")]
    public bool hasSlimeRevived = false;
    public int poisonStacks = 0;

    [Header("Hero System")]
    private HeroData currentHero; // Biến private để lưu hero hiện tại

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

    [Header("UI Intro")]
    public BattleIntroUI battleIntroUI;

    [Header("Cấu hình Buff & Level")]
    public List<BuffData> allBuffsLibrary;
    public GameObject buffCardPrefab;
    public GameObject buffSelectionPanel;
    public Transform buffContainer;
    public int levelsPerBuff = 2;
    [Header("Sudden Death Config")]
    public int currentBattleTurn = 0; // Đếm số lượt trong trận hiện tại
    public int turnLimitBeforeBurn = 5; // Giới hạn 5 lượt
    public int burnDamageAmount = 50; // Sát thương thiêu đốt
    //REROLL CONFIG ---
    [Space(10)]
    public Button buffRerollButton; // Kéo nút Reroll vào đây
    private bool hasRerolled = false; // Biến kiểm tra xem đã reroll chưa
    private const int REROLL_COST = 5; // Giá cố định 5 Vàng

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
    public Image heroPortraitUI;

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
    public List<CardData> playerHand = new List<CardData>();
    public List<CardData> dealerHand = new List<CardData>();
    private GameObject hiddenCardObject;

    public int currentBet = 0;
    public int currentLevel = 1;
    private bool isDoubleActive = false;

    private EnemyData currentEnemyData;

    // --- CÁC HÀM PUBLIC HELPER ---
    public int GetCurrentBet() { return currentBet; }
    public void UpdateResultText(string msg) { if (resultText) resultText.text = msg; }

    public void SpawnExtraCardForPlayer()
    {
        SpawnCard(playerHand, playerHandArea, playerDeck);
        if (resultText) ShowNotification("RÚT DÍNH BÀI");
        UpdateScoreUI();
        if (CalculateScore(playerHand) > 21) ResolveCombat(true);
    }
    // -----------------------------------------------------

    void Start()
    {
        // Biến lưu máu khởi đầu (mặc định là -1 để CharacterBase tự hiểu là lấy gốc)
        int startingHP = -1;

        // 1. LẤY HERO TỪ DATA HOLDER
        if (GameDataHolder.Instance != null && GameDataHolder.Instance.selectedHero != null)
        {
            currentHero = GameDataHolder.Instance.selectedHero;
            Debug.Log($"Đang chơi Hero: {currentHero.heroName}");

            // Lưu lại máu của Hero để dùng cho hàm Initialize bên dưới
            startingHP = currentHero.baseHP;

            // Update UI Portrait
            if (heroPortraitUI != null)
            {
                heroPortraitUI.sprite = currentHero.portrait;
                heroPortraitUI.preserveAspect = true;
            }
        }
        else
        {
            Debug.LogWarning("Chưa chọn Hero! Load Hero mặc định để test.");
        }

        // 2. KHỞI TẠO PLAYER (SỬA ĐOẠN NÀY)
        if (player)
        {
            // Truyền thẳng máu vào hàm Initialize
            // Nếu startingHP là 900 -> Set máu 900
            // Nếu startingHP là -1 (chưa chọn hero) -> Set máu mặc định (1000)
            player.Initialize(startingHP);
        }

        // 3. SAU KHI INIT XONG MỚI ADD BUFF (Để đảm bảo chỉ số đúng rồi mới cộng buff)
        if (currentHero != null && currentHero.startingBuff != null)
        {
            player.AddOrUpgradeBuff(currentHero.startingBuff);
        }

        SetupEnemyForLevel();
        if (buffSelectionPanel) buffSelectionPanel.SetActive(false);
        ShowBettingPhase();
        if (notificationText) notificationText.alpha = 0f;
    }

    void SetupEnemyForLevel()
    {
        currentBattleTurn = 0; //Reset bộ đếm lượt
        hasSlimeRevived = false;
        poisonStacks = 0;

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
            currentEnemyData.abilityLogic.description : "Quái thường.";

        if (enemyTooltipTrigger != null)
        {
            enemyTooltipTrigger.header = "";
            enemyTooltipTrigger.content = abilityDesc;
        }

        if (enemyAbilityText)
            enemyAbilityText.text = (currentEnemyData.abilityLogic != null) ?
                $"<color=red>⚠️ {abilityDesc}</color>" : "";

        // SAU KHI ĐÃ CHỌN ĐƯỢC QUÁI (currentEnemyData)
        // Kiểm tra xem đây có phải Elite hay Boss không?
        // (Bạn có thể thêm biến bool isElite trong EnemyData để check cho chuẩn)
        bool isEliteOrBoss = eliteEnemiesList.Contains(currentEnemyData);

        if (isEliteOrBoss && battleIntroUI != null)
        {
            // Ẩn UI game tạm thời để chiếu Intro
            if (gameplayPanel) gameplayPanel.SetActive(false);
            if (bettingPanel) bettingPanel.SetActive(false);

            // CHẠY INTRO
            battleIntroUI.PlayIntroSequence(currentHero, currentEnemyData, OnIntroFinished);
        }
        else
        {
            // Nếu là quái thường thì vào thẳng game như cũ
            OnIntroFinished();
        }
    }

    // Hàm callback được gọi sau khi Intro chạy xong
    void OnIntroFinished()
    {
        // Hiện bảng cược để bắt đầu chơi
        ShowBettingPhase();

        // Hiện lại các UI cần thiết
        if (notificationText) notificationText.alpha = 0f;
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

        // Cập nhật text Level kèm theo số lượt
        if (levelText)
        {
            string color = (currentBattleTurn >= turnLimitBeforeBurn - 1) ? "red" : "white";
            levelText.text = $"LEVEL {currentLevel} <size=60%><color={color}>(Turn {currentBattleTurn + 1}/{turnLimitBeforeBurn})</color></size>";
        }
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

        // Vì buff Ngũ Linh có thể kết thúc game ngay lập tức, ta cần check
        bool gameEndedByBuff = false;

        if (player.activeBuffs != null)
        {
            foreach (var buffInfo in player.activeBuffs)
            {
                // Gọi vào hàm mới ta vừa thêm
                buffInfo.data.OnPostPlayerHit(this, buffInfo.level);

                // Kiểm tra xem game đã kết thúc chưa (nếu buff đã gọi ResolveCombat)
                // Cách đơn giản nhất là check activeSelf của gameplayPanel hoặc biến flag
                if (!gameplayPanel.activeSelf)
                {
                    gameEndedByBuff = true;
                    break;
                }
            }
        }

        if (gameEndedByBuff) return;

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
    public void ResolveCombat(bool playerBusted)
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

        // --- XỬ LÝ THẮNG THUA & TRỪ MÁU BÀI (Giai đoạn 1) ---
        if (playerWin)
        {
            finalDamage = player.CalculateFinalOutgoingDamage(baseDamage, pScore, isDoubleActive);
            if (tempDamageMultiplier > 1f) finalDamage = Mathf.RoundToInt(finalDamage * tempDamageMultiplier);

            if (player.activeBuffs != null)
                foreach (var b in player.activeBuffs) finalDamage = b.data.ModifyPlayerDamage(finalDamage, this, b.level);

            // Logic tính dmg cũ của ability (giữ lại để tương thích)
            if (currentEnemyData.abilityLogic != null)
                finalDamage = currentEnemyData.abilityLogic.OnCalculateDamage(finalDamage, pScore);

            // [MỚI] SLIME: Hook giảm sát thương nhận vào
            if (currentEnemyData.abilityLogic != null)
            {
                finalDamage = currentEnemyData.abilityLogic.OnModifyIncomingDamage(finalDamage, this);
            }

            string noti = "THẮNG! GÂY " + finalDamage + " ST";
            if (playerBlackjack) noti = "<color=yellow>BLACKJACK!</color> " + noti;
            ShowNotification(noti);

            enemy.TakeDamage(finalDamage);
        }
        else if (enemyWin)
        {
            finalDamage = player.CalculateFinalIncomingDamage(currentBet, dScore);
            if (tempDamageMultiplier > 1f) finalDamage = Mathf.RoundToInt(finalDamage * tempDamageMultiplier);

            if (tempBlock > 0)
            {
                int blocked = Mathf.Min(finalDamage, tempBlock);
                finalDamage -= blocked;
                if (finalDamage < 0) finalDamage = 0;
            }

            // --- [MỚI] VAMPIRE & SKELETON: Can thiệp damage đầu ra của quái ---
            if (currentEnemyData.abilityLogic != null)
            {
                finalDamage = currentEnemyData.abilityLogic.OnModifyOutgoingDamage(finalDamage, this);
            }

            if (tempBlock > 0)
                ShowNotification($"THUA! (ĐỠ {tempBlock}) NHẬN {finalDamage} ST");
            else
                ShowNotification($"THUA! NHẬN {finalDamage} SÁT THƯƠNG");
            player.TakeDamage(finalDamage);
            // [MỚI] NHỆN ĐỘC: Hook tích độc khi quái đánh trúng
            if (currentEnemyData.abilityLogic != null)
            {
                currentEnemyData.abilityLogic.OnEnemyDealsDamage(this);
            }
        }
        else
        {
            ShowNotification("HÒA! THU HỒI CƯỢC");
        }

        if (playerDeck) playerDeck.AddToDiscardPile(playerHand);
        if (enemyDeck) enemyDeck.AddToDiscardPile(dealerHand);

        // --- THAY ĐỔI Ở ĐÂY: Gọi quy trình kết thúc lượt (Có độ trễ) ---
        // Coroutine này sẽ xử lý tiếp: Trừ độc -> Thiêu đốt -> Hồi sinh Slime -> Check Win/Loss
        StartCoroutine(EndTurnSequence());
    }

    void CheckWinCondition()
    {
        if (player.CurrentHP <= 0)
        {
            GameOver("BẠN THUA RỒI!");
        }
        else if (enemy.CurrentHP <= 0)
        {
            // SLIME: Check hồi sinh trước khi cho chết ---
            if (currentEnemyData.abilityLogic != null)
            {
                // Nếu hàm OnTryRevive trả về true -> Nghĩa là nó sống lại -> Return luôn, ko thắng nữa
                if (currentEnemyData.abilityLogic.OnTryRevive(this))
                {
                    Invoke("ShowBettingPhase", 2f); // Vào lại vòng cược tiếp theo
                    return;
                }
            }
            // 1. Logic thưởng tiền cơ bản (Code cũ giữ nguyên)
            int goldReward = (currentLevel % 3 == 0) ? 70 : 30; // Ví dụ
            player.AddGold(goldReward);

            // 2. --- [MỚI] GỌI BUFF ON ENEMY KILLED ---
            if (player.activeBuffs != null)
            {
                // Lưu ý: currentEnemyData là biến bạn dùng trong SetupEnemyForLevel
                foreach (var b in player.activeBuffs)
                {
                    b.data.OnEnemyKilled(this, currentEnemyData, b.level);
                }
            }
            // ------------------------------------------

            ShowNotification($"ĐỊCH ĐÃ BẠI! (+{goldReward} Vàng)");
            HandleLevelComplete();
        }
        else
        {
            Invoke("ShowBettingPhase", 2f);
        }
    }

    void HandleLevelComplete()
    {
        //HERO PASSIVE: GERNAS (Hồi máu)
        if (currentHero != null && currentHero.passiveType == HeroPassiveType.HealOnLevel)
        {
            int healAmount = 50;
            player.Heal(healAmount);
        }
        //Logic hiển thị: Win -> (Buff?) -> Shop -> Next Level
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

    // 1. Hàm sinh buff (Tách ra để dùng lại)
    void GenerateRandomBuffs()
    {
        // Xóa cũ
        foreach (Transform child in buffContainer) Destroy(child.gameObject);

        // Lọc buff khả dụng
        List<BuffData> validBuffs = new List<BuffData>();
        foreach (var buff in allBuffsLibrary)
        {
            if (player.GetBuffLevel(buff) < 3) validBuffs.Add(buff);
        }

        // Random 3 cái
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

    // 2. Hàm xử lý nút Reroll
    public void OnRerollBuffsPressed()
    {
        // Kiểm tra an toàn: Nếu đã reroll rồi thì chặn luôn
        if (hasRerolled) return;

        if (player.currentGold >= REROLL_COST)
        {
            // Trừ tiền
            player.AddGold(-REROLL_COST);

            // Đánh dấu đã dùng -> Khóa nút ngay lập tức
            hasRerolled = true;
            if (buffRerollButton) buffRerollButton.interactable = false;

            // Sinh lại buff mới
            GenerateRandomBuffs();

            ShowNotification($"ĐÃ ĐỔI LẠI! (-{REROLL_COST}G)");
        }
        else
        {
            ShowNotification("KHÔNG ĐỦ 5G!");
            // Rung nút cảnh báo
            if (buffRerollButton) buffRerollButton.transform.DOPunchPosition(Vector3.right * 10, 0.3f, 10);
        }
    }

    void ShowBuffSelection()
    {
        if (buffSelectionPanel) buffSelectionPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);

        // [MỚI] Reset quyền Reroll cho lần chọn này
        hasRerolled = false;

        // Kiểm tra tiền để bật/tắt nút ban đầu
        if (buffRerollButton)
        {
            // Chỉ sáng nút nếu Đủ tiền
            buffRerollButton.interactable = (player.currentGold >= REROLL_COST);
        }

        // Gọi hàm sinh buff
        GenerateRandomBuffs();
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

    public int CalculateScore(List<CardData> hand)
    {
        int score = 0;
        int aceCount = 0;

        foreach (var card in hand)
        {
            score += card.value;
            // Kiểm tra Ace để tính 11 điểm
            if (card.cardName.Contains("Ace") || card.value == 11) aceCount++;
        }

        // --- LOGIC CHO PLAYER ---
        if (hand == playerHand)
        {
            score += tempScoreBonus; // Cộng điểm từ Item

            // Nội tại Hero (Kbruh)
            if (currentHero != null && currentHero.passiveType == HeroPassiveType.ScorePlusOne)
            {
                score += 1;
            }
        }

        // --- [MỚI] LOGIC CHO DEALER / ELITE ---
        // Nếu đây là bài của Dealer (Quái) và có Ability đặc biệt (VD: Hiệp sĩ Xương)
        if (hand == dealerHand && currentEnemyData != null && currentEnemyData.abilityLogic != null)
        {
            // Gọi hàm cộng điểm từ Ability (Skeleton trả về 1)
            score += currentEnemyData.abilityLogic.OnCalculateScoreBonus(score);
        }
        // --------------------------------------

        // Xử lý Ace: Nếu điểm > 21 thì coi Ace là 1 (trừ đi 10)
        while (score > 21 && aceCount > 0)
        {
            score -= 10;
            aceCount--;
        }

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
        string msg = (amount > 0) ? "UỐNG THUỐC TĂNG TRƯỞNG (+2)" : "UỐNG THUỐC LINH HOẠT (-3)";
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
        notificationText.DOFade(0f, 05f).SetDelay(1f);
    }

    public HeroData GetCurrentHero()
    {
        return currentHero;
    }

    // Hàm xử lý riêng cho cơ chế Thiêu Đốt (Sudden Death)
    void ApplySuddenDeath()
    {
        // Chỉ tính nếu quái VẪN CÒN SỐNG sau khi đánh xong (nghĩa là trận đấu chưa kết thúc)
        // Lưu ý: Nếu quái chết bởi đòn đánh trên thì currentHP <= 0, ta không cần thiêu đốt nữa
        if (enemy.CurrentHP > 0)
        {
            currentBattleTurn++; // Tăng lượt

            // Nếu vượt quá giới hạn lượt (VD: 5)
            if (currentBattleTurn >= turnLimitBeforeBurn)
            {
                // 1. Trừ máu ngay lập tức
                player.TakeDamage(burnDamageAmount);

                // 2. Hiển thị thông báo cảnh báo (chạy sau thông báo thắng thua 1 chút)
                string warningMsg = $"<color=red>QUÁ GIỜ! THIÊU ĐỐT -{burnDamageAmount} HP ({currentBattleTurn}/{turnLimitBeforeBurn})</color>";
                StartCoroutine(ShowBurnWarning(warningMsg));
            }
        }
    }

    System.Collections.IEnumerator ShowBurnWarning(string msg)
    {
        yield return new WaitForSeconds(1.5f); // Chờ thông báo thắng/thua cũ hiện xong
        ShowNotification(msg);

        // Rung màn hình hoặc hiệu ứng âm thanh ở đây nếu muốn
        if (notificationText) notificationText.transform.DOShakePosition(0.5f, 10f);
    }

    System.Collections.IEnumerator EndTurnSequence()
    {
        // 1. Nếu một trong hai bên đã chết sau đòn đánh bài, bỏ qua thiêu đốt để Game Over luôn cho nhanh
        if (enemy.CurrentHP > 0 && player.CurrentHP > 0)
        {
            currentBattleTurn++; // Tăng lượt

            // Nếu đến giờ thiêu đốt
            if (currentBattleTurn >= turnLimitBeforeBurn)
            {
                // --- CHỜ 1.5 GIÂY (Để người chơi nhìn thấy dmg của ván bài trước) ---
                yield return new WaitForSeconds(1.5f);

                // --- [MỚI] NHỆN: Trừ máu độc ---
                if (currentEnemyData.abilityLogic != null && poisonStacks > 0)
                {
                    currentEnemyData.abilityLogic.OnTurnEnd(this);
                    yield return new WaitForSeconds(1.0f); // Chờ cho người chơi nhìn thấy dmg độc
                }

                // --- HIỂN THỊ THÔNG BÁO THIÊU ĐỐT ---
                string warningMsg = $"<color=red>THIÊU ĐỐT! (-{burnDamageAmount} HP)</color>";
                ShowNotification(warningMsg);

                // Rung chữ cảnh báo
                if (notificationText) notificationText.transform.DOShakePosition(0.5f, 10f);

                // --- CHỜ 0.5 GIÂY (Cho người chơi đọc chữ "Thiêu đốt") ---
                yield return new WaitForSeconds(0.5f);

                // --- NHẬN DMG THIÊU ĐỐT (Giai đoạn 2) ---
                player.TakeDamage(burnDamageAmount);
            }
        }

        // 2. Kiểm tra thắng thua cuối cùng
        // (Nếu player chết bởi thiêu đốt, hàm này sẽ xử lý Game Over)
        CheckWinCondition();
    }
}