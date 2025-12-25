using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Enemy Abilities/Slippery Hands")]
public class Ability_SlipperyHands : EnemyAbilityData
{
    public override void OnPlayerHit(GameManager gm)
    {
        // Tỉ lệ 36% rút thêm bài
        if (Random.value < 0.36f)
        {
            Debug.Log("Trượt tay! Rút thêm bài!");
            // Gọi hàm SpawnCard của GameManager (Cần public hàm này hoặc dùng phương thức khác)
            // Ở đây ta giả định GameManager có hàm public SpawnExtraCardForPlayer()
            gm.SpawnExtraCardForPlayer();
        }
    }
}