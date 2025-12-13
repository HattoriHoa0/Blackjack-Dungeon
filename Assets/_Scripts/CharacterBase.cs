using UnityEngine;
using TMPro;
using System.Collections.Generic; // Để dùng List
using DG.Tweening; // Thư viện hiệu ứng

// MỚI: Class phụ để lưu trạng thái buff kèm cấp độ
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
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;

    private int currentHP;
    private int currentGold = 0;

    // MỚI: Thay vì List<BuffData>, ta dùng List<RuntimeBuff> để lưu cả Level
    public List<RuntimeBuff> activeBuffs = new List<RuntimeBuff>();

    // Property để các script khác đọc máu
    public int CurrentHP => currentHP;

    public void Initialize()
    {
        currentHP = maxHP;
        UpdateUI();
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        // Hiệu ứng tiền nảy lên
        if (goldText) goldText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    }

    // --- LOGIC BUFF MỚI (NÂNG CẤP) ---

    // Hàm này tự động kiểm tra: Nếu có rồi thì nâng cấp, chưa có thì thêm mới
    public void AddOrUpgradeBuff(BuffData buffData)
    {
        // 1. Tìm xem đã có buff này chưa (so sánh bằng ID)
        foreach (var runtimeBuff in activeBuffs)
        {
            if (runtimeBuff.data.id == buffData.id)
            {
                // Nếu có rồi và chưa max cấp 3 -> Nâng cấp
                if (runtimeBuff.level < 3)
                {
                    runtimeBuff.level++;
                    Debug.Log($"Nâng cấp {buffData.buffName} lên cấp {runtimeBuff.level}");
                }
                return; // Thoát hàm
            }
        }

        // 2. Nếu chưa có -> Thêm mới level 1
        activeBuffs.Add(new RuntimeBuff(buffData, 1));
        Debug.Log($"Nhận mới {buffData.buffName} cấp 1");
    }

    // Hàm lấy level hiện tại của một buff (Để UI biết đường hiển thị cấp tiếp theo)
    public int GetBuffLevel(BuffData buffData)
    {
        foreach (var b in activeBuffs)
        {
            if (b.data.id == buffData.id) return b.level;
        }
        return 0; // Chưa có thì level là 0
    }

    // --- LOGIC TÍNH TOÁN SÁT THƯƠNG (GỌI TỪ GAME MANAGER) ---

    // Tính sát thương gây ra (Duyệt qua tất cả buff để cộng dồn hiệu ứng)
    public int CalculateFinalOutgoingDamage(int baseDamage, int myScore, bool isDouble)
    {
        int finalDmg = baseDamage;
        foreach (var b in activeBuffs)
        {
            // Gọi hàm tính toán trong BuffData, truyền kèm Level hiện tại
            finalDmg = b.data.OnCalculateOutgoingDamage(finalDmg, myScore, isDouble, b.level);
        }
        return finalDmg;
    }

    // Tính sát thương nhận vào (Duyệt qua tất cả buff để giảm sát thương)
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
}