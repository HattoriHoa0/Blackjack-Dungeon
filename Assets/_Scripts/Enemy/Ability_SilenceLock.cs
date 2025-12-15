using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Silence Lock")]
public class Ability_SilenceLock : EnemyAbilityData
{
    public override void OnRoundStart(GameManager gm)
    {
        // Khóa nút Hit
        gm.hitButton.interactable = false;

        // Mở nút Double nếu đủ tiền (Logic riêng của skill này)
        // Lưu ý: Cần truy cập biến currentBet và player của GM
        bool canDouble = gm.player.CurrentHP >= gm.GetCurrentBet() * 2;
        gm.doubleButton.interactable = canDouble;

        gm.resultText.text = "CẤM HIT";
    }

    public override bool OnDealerTurnStop(GameManager gm)
    {
        return true; // Bắt Dealer dừng ngay, không rút thêm
    }
}