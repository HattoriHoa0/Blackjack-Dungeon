using UnityEngine;
using UnityEngine.SceneManagement; // Cần cái này để chuyển cảnh

public class MainMenuController : MonoBehaviour
{
    // Gắn hàm này vào nút "CHƠI NGAY"
    public void PlayGame()
    {
        // "SampleScene" là tên scene chơi game của bạn (nhớ kiểm tra đúng tên)
        SceneManager.LoadScene("SampleScene");
    }

    // Gắn hàm này vào nút "THOÁT" (nếu có)
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit();
    }
}