using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelector : MonoBehaviour
{
    public HeroData heroData; // Kéo file HeroData vào đây

    public void OnClickSelectHero()
    {
        if (GameDataHolder.Instance != null)
        {
            GameDataHolder.Instance.selectedHero = heroData;
            Debug.Log($"Đã chọn: {heroData.heroName}");

            // Chuyển sang Scene Game ngay lập tức
            SceneManager.LoadScene("GameplayScene");
        }
        else
        {
            Debug.LogError("Thiếu GameDataHolder trong Scene!");
        }
    }
}