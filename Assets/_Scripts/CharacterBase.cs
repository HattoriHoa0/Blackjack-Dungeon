using UnityEngine;
using UnityEngine.UI; // Để dùng Image
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class RuntimeBuff
{
    public BuffData data;
    public int level;

    public RuntimeBuff(BuffData d, int l)
    {
        data = d;
        level = l;
    }
}

public class CharacterBase : MonoBehaviour
{
    [Header("Cấu hình")]
    public int maxHP = 50;

    [Header("UI Hiển thị")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;

    [Header("Inventory")]
    public List<ItemData> inventory = new List<ItemData>(); // Túi đồ
    public int maxInventorySlots = 6; // Giới hạn 6 ô
    public int currentHP;
    public int currentGold = 0;

    public List<RuntimeBuff> activeBuffs = new List<RuntimeBuff>();

    public int CurrentHP => currentHP;

    public void Initialize(int overrideMaxHP = -1)
    {
        if (overrideMaxHP > 0) maxHP = overrideMaxHP;
        currentHP = maxHP;
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
        foreach (var runtimeBuff in activeBuffs)
        {
            if (runtimeBuff.data.id == buffData.id)
            {
                if (runtimeBuff.level < 3)
                {
                    runtimeBuff.level++;
                    Debug.Log($"Nâng cấp {buffData.buffName} lên cấp {runtimeBuff.level}");
                }
                return;
            }
        }
        activeBuffs.Add(new RuntimeBuff(buffData, 1));
        Debug.Log($"Nhận mới {buffData.buffName} cấp 1");
    }

    public int GetBuffLevel(BuffData buffData)
    {
        foreach (var b in activeBuffs)
        {
            if (b.data.id == buffData.id) return b.level;
        }
        return 0;
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
    }

    void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
            hpText.color = (currentHP < maxHP * 0.3f) ? Color.red : Color.white;
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
        if (hpText) hpText.color = Color.green; // Hiệu ứng nháy xanh khi hồi máu
    }
    // Hàm kiểm tra túi đầy chưa
    public bool IsInventoryFull()
    {
        return inventory.Count >= maxInventorySlots;
    }

    // Hàm thêm đồ vào túi
    public void AddItem(ItemData item)
    {
        if (!IsInventoryFull())
        {
            inventory.Add(item);
            Debug.Log($"Đã nhận: {item.itemName}");
        }
    }

    // Hàm xóa đồ khỏi túi (khi dùng)
    public void RemoveItem(ItemData item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
        }
    }
}