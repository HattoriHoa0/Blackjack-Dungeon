using UnityEngine;
using TMPro;
using DG.Tweening; // Nếu bạn muốn dùng hiệu ứng rung

public class CharacterBase : MonoBehaviour
{
    [Header("Cấu hình")]
    public int maxHP = 50;
    public TextMeshProUGUI hpText; // Kéo text máu của nhân vật này vào đây

    private int currentHP;

    public int CurrentHP => currentHP; // Cho phép script khác đọc máu (nhưng không được sửa trực tiếp)

    public void Initialize()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        // Hiệu ứng Visual (Game Juice)
        UpdateUI();

        // Rung chữ HP khi bị đánh (Nếu đã cài DOTween)
        if (hpText) hpText.transform.DOShakePosition(0.5f, 10f);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";

            // Đổi màu chữ: Ít máu thì đỏ, nhiều máu thì xanh/trắng
            hpText.color = (currentHP < maxHP * 0.3f) ? Color.red : Color.white;
        }
    }
}