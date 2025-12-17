using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Danh sách nhạc nền")]
    public AudioClip[] playlist; // Kéo thả các file nhạc vào đây

    private AudioSource audioSource;
    private int currentTrackIndex = -1;

    void Awake()
    {
        // --- SINGLETON PATTERN ---
        // Đảm bảo chỉ có 1 AudioManager tồn tại trong game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển Scene

            // Tự thêm AudioSource nếu chưa có
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // Nếu đã có 1 cái rồi thì hủy cái mới này đi
        }
    }

    void Start()
    {
        // Bắt đầu chơi nhạc ngay khi vào game
        PlayRandomMusic();
    }

    void Update()
    {
        // Kiểm tra: Nếu nhạc không còn chạy (hết bài) thì chuyển bài mới
        if (!audioSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    public void PlayRandomMusic()
    {
        if (playlist.Length == 0) return;

        int newIndex;

        // Thuật toán: Random bài mới, nhưng tránh trùng lại bài vừa nghe
        if (playlist.Length == 1)
        {
            newIndex = 0;
        }
        else
        {
            do
            {
                newIndex = Random.Range(0, playlist.Length);
            }
            while (newIndex == currentTrackIndex); // Random lại nếu trùng bài cũ
        }

        currentTrackIndex = newIndex;

        // Cài đặt và chơi nhạc
        audioSource.clip = playlist[newIndex];
        audioSource.Play();
    }

    // Hàm chỉnh âm lượng (để sau này làm Slider trong Setting)
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}