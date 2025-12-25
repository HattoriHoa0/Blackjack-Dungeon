using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [Header("Thông tin chung")]
    public string id;
    public string buffName;
    public Sprite icon;

    public abstract string GetDescription(int level);

    // --- NHÓM 1: TÍNH TOÁN DAMAGE (Giữ nguyên) ---
    public virtual int OnCalculateOutgoingDamage(int baseDamage, int myScore, bool isDouble, int level) { return baseDamage; }
    public virtual int OnCalculateIncomingDamage(int incomingDamage, int enemyScore, int level) { return incomingDamage; }

    // --- NHÓM 2: SỰ KIỆN GAME (Hooks) ---
    public virtual void OnLevelStart(GameManager gm, int level) { }

    // Dùng cho buff thay đổi chỉ số vĩnh viễn (HP, Slot Balo)
    // Hàm này sẽ được CharacterBase gọi khi RecalculateStats
    public virtual void ApplyStatModifiers(CharacterBase character, int level) { }

    // Dùng cho buff "Ngũ Linh"
    // Hàm này sẽ được GameManager gọi ngay sau khi rút bài (OnHitPressed)
    public virtual void OnPostPlayerHit(GameManager gm, int level) { }
    public virtual int ModifyPlayerDamage(int finalDamage, GameManager gm, int level) { return finalDamage; }
    // Tham số: gm (để truy cập player/enemy), enemyData (để biết quái nào vừa chết)
    public virtual void OnEnemyKilled(GameManager gm, EnemyData enemyData, int level) { }
}