using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Cần cái này để chuyển cảnh

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel; // Kéo cái Panel vào đây
    public Slider volumeSlider;      // Kéo cái Slider vào đây

    [Header("Scene Config")]
    public string mainMenuSceneName = "MainMenu"; // Tên màn hình Menu chính của bạn

    private bool isPaused = false;

    void Start()
    {
        // 1. Ẩn menu lúc đầu
        if (settingsPanel) settingsPanel.SetActive(false);

        // 2. Cài đặt Slider theo âm lượng hiện tại
        if (volumeSlider)
        {
            volumeSlider.value = AudioListener.volume;
            // Gán sự kiện khi kéo thanh slider
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        // 3. Bắt sự kiện ấn ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (settingsPanel) settingsPanel.SetActive(isPaused);

        // [TÙY CHỌN] Ngưng đọng thời gian khi mở menu
        // Nếu muốn game dừng hẳn:
        Time.timeScale = isPaused ? 0f : 1f;

        // Hiện/Ẩn con trỏ chuột (nếu game FPS/TPS đang ẩn chuột)
        // Cursor.visible = isPaused;
        // Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Hàm chỉnh âm lượng (Gán vào Slider)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // Chỉnh âm lượng toàn cục (0.0 đến 1.0)
    }

    // Hàm về Menu chính (Gán vào Nút)
    public void ReturnToMainMenu()
    {
        // Nhớ trả lại thời gian chạy bình thường trước khi chuyển cảnh
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}