using UnityEngine;

public class GameDataHolder : MonoBehaviour
{
    public static GameDataHolder Instance;

    [Header("Dữ liệu phiên chơi")]
    public HeroData selectedHero; // Hero người chơi vừa chọn

    void Awake()
    {
        // Singleton Pattern: Đảm bảo chỉ có 1 cái tồn tại
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Lệnh quan trọng nhất: Không hủy khi chuyển Scene
        }
        else
        {
            Destroy(gameObject); // Nếu lỡ tạo trùng thì xóa cái mới đi
        }
    }
}