using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class RuntimeBuff
{
    public BuffData data;
    public int level;
    [HideInInspector] public GameObject iconObj; // [FIX] Thêm biến này để quản lý icon UI

    public RuntimeBuff(BuffData d, int l)
    {
        data = d;
        level = l;
    }
}

public class CharacterBase : MonoBehaviour
{
    [Header("Cấu hình")]
    public int baseMaxHP = 1000; // [FIX] Thêm biến gốc
    public int maxHP;            // Biến thực tế (sau khi cộng buff)

    [Header("UI Hiển thị")]
    public Image portraitImage;
    public Image hpBarFill;      // [FIX] Thêm biến thanh máu
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;

    [Header("Buff UI")]
    public Transform buffContainer;     // [FIX] Nơi chứa icon buff
    public GameObject buffIconPrefab;   // [FIX] Prefab icon buff

    [Header("Inventory")]
    public List<ItemData> inventory = new List<ItemData>();
    public int maxInventorySlots = 6;
    public int currentHP;
    public int currentGold = 0;

    public List<RuntimeBuff> activeBuffs = new List<RuntimeBuff>();

    public int CurrentHP => currentHP;

    public void Initialize(int overrideMaxHP = -1)
    {
        // Ưu tiên: override -> baseMaxHP inspector -> mặc định
        if (overrideMaxHP > 0) baseMaxHP = overrideMaxHP;
        else if (baseMaxHP <= 0) baseMaxHP = 1000; // Giá trị fallback

        // Tính toán chỉ số lần đầu
        RecalculateStats();

        currentHP = maxHP; // Hồi đầy máu lúc đầu
        UpdateUI();
        UpdateGoldUI();
    }

    public void SetupVisuals(string name, Sprite sprite)
    {
        if (nameText) nameText.text = name;
        if (portraitImage && sprite) portraitImage.sprite = sprite;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        if (goldText) goldText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    }

    // --- LOGIC BUFF ---
    public void AddOrUpgradeBuff(BuffData buffData)
    {
        RuntimeBuff existing = activeBuffs.Find(b => b.data.id == buffData.id);

        if (existing != null)
        {
            if (existing.level < 3)
            {
                existing.level++;
                Debug.Log($"Nâng cấp {buffData.buffName} lên cấp {existing.level}");

                // Cập nhật số level trên icon UI (nếu có)
                if (existing.iconObj)
                {
                    var txt = existing.iconObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt) txt.text = existing.level.ToString();
                }
            }
        }
        else
        {
            RuntimeBuff newBuff = new RuntimeBuff(buffData, 1);

            // Tạo Icon UI cho buff mới
            if (buffIconPrefab && buffContainer)
            {
                GameObject icon = Instantiate(buffIconPrefab, buffContainer);
                icon.GetComponent<Image>().sprite = buffData.icon;
                icon.GetComponentInChildren<TextMeshProUGUI>().text = "1";
                // Add TooltipTrigger ở đây nếu muốn
                newBuff.iconObj = icon;
            }

            activeBuffs.Add(newBuff);
            Debug.Log($"Nhận mới {buffData.buffName} cấp 1");
        }

        // [QUAN TRỌNG] Tính lại chỉ số sau khi thay đổi buff
        RecalculateStats();
    }

    public int GetBuffLevel(BuffData buffData)
    {
        foreach (var b in activeBuffs)
        {
            if (b.data.id == buffData.id) return b.level;
        }
        return 0;
    }

    public bool HasBuff(BuffData buffData) // [THÊM] Hàm tiện ích
    {
        return GetBuffLevel(buffData) > 0;
    }

    // --- LOGIC TÍNH TOÁN SÁT THƯƠNG ---
    public int CalculateFinalOutgoingDamage(int baseDamage, int myScore, bool isDouble)
    {
        int finalDmg = baseDamage;
        foreach (var b in activeBuffs)
        {
            finalDmg = b.data.OnCalculateOutgoingDamage(finalDmg, myScore, isDouble, b.level);
        }
        return finalDmg;
    }

    public int CalculateFinalIncomingDamage(int incomingDamage, int enemyScore)
    {
        int finalDmg = incomingDamage;
        foreach (var b in activeBuffs)
        {
            finalDmg = b.data.OnCalculateIncomingDamage(finalDmg, enemyScore, b.level);
        }
        return finalDmg;
    }

    // --- CÁC HÀM CƠ BẢN ---
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateUI();
        if (hpText) hpText.transform.DOShakePosition(0.5f, 10f);
        if (hpBarFill) hpBarFill.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }

    void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
            hpText.color = (currentHP < maxHP * 0.3f) ? Color.red : Color.white;
        }
        // [FIX] Cập nhật thanh máu trượt
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = (float)currentHP / maxHP;
        }
    }

    void UpdateGoldUI()
    {
        if (goldText) goldText.text = $"Gold: {currentGold}";
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateUI();
        if (hpText) hpText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        if (hpText) hpText.color = Color.green;
    }

    public bool IsInventoryFull()
    {
        return inventory.Count >= maxInventorySlots;
    }

    public void AddItem(ItemData item)
    {
        if (!IsInventoryFull())
        {
            inventory.Add(item);
            Debug.Log($"Đã nhận: {item.itemName}");
        }
    }

    public void RemoveItem(ItemData item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
        }
    }

    //HÀM TÍNH LẠI STATS
    public void RecalculateStats()
    {
        // 1. Lưu lại MaxHP cũ trước khi tính toán
        int oldMaxHP = maxHP;

        // 2. Reset về gốc
        maxHP = baseMaxHP;
        maxInventorySlots = 6; // Reset số ô túi mặc định

        // 3. Duyệt qua tất cả buff để cộng chỉ số mới
        if (activeBuffs != null)
        {
            foreach (var buffInfo in activeBuffs)
            {
                buffInfo.data.ApplyStatModifiers(this, buffInfo.level);
            }
        }

        // 4. Chỉ hồi máu nếu MaxHP thực sự tăng lên
        int difference = maxHP - oldMaxHP;

        // Nếu có sự chênh lệch dương (VD: từ 1000 lên 1100 -> diff = 100)
        // Thì cộng thêm 100 vào máu hiện tại
        if (difference > 0)
        {
            currentHP += difference;
        }

        // 5. Đảm bảo máu không vượt quá giới hạn (đề phòng trường hợp giảm MaxHP)
        if (currentHP > maxHP) currentHP = maxHP;

        UpdateUI();
    }
}