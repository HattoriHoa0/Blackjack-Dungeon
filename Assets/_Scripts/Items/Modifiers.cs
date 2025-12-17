using UnityEngine;

[CreateAssetMenu(menuName = "Blackjack/Items/Score Modifier")]
public class Item_ScoreModifier : ItemData
{
    public int scoreModifier; // Điền +3 hoặc -3 trong Inspector

    public override bool OnUse(GameManager gm)
    {
        gm.AddTempScoreBonus(scoreModifier);
        return true;
    }
}