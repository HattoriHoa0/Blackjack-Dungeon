using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Cần DOTween
using System;

public class BattleIntroUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup panelCanvasGroup; // Để Fade in/out toàn bộ panel

    [Header("Player Side (Left)")]
    public RectTransform leftContainer;
    public Image playerImage;
    public TextMeshProUGUI playerName;

    [Header("Enemy Side (Right)")]
    public RectTransform rightContainer;
    public Image enemyImage;
    public TextMeshProUGUI enemyName;

    [Header("Center")]
    public RectTransform vsIcon; // Icon 2 cây kiếm

    [Header("Audio SFX")]
    public AudioSource audioSource; // Kéo AudioSource vào đây
    public AudioClip impactSFX;     // Tiếng va chạm (lúc hiện VS)

    // Vị trí gốc để reset animation
    private float leftStartX = -1500f;
    private float rightStartX = 1500f;

    public void PlayIntroSequence(HeroData hero, EnemyData enemy, Action onComplete)
    {
        // 1. SETUP DỮ LIỆU
        gameObject.SetActive(true);
        panelCanvasGroup.alpha = 1;

        // Set ảnh và tên
        playerImage.sprite = hero.portrait;
        playerName.text = hero.heroName;

        enemyImage.sprite = enemy.portrait; // Giả sử EnemyData có biến portrait
        enemyName.text = enemy.enemyName;

        // Reset vị trí về 2 bên cánh gà
        leftContainer.anchoredPosition = new Vector2(leftStartX, 0);
        rightContainer.anchoredPosition = new Vector2(rightStartX, 0);
        vsIcon.localScale = Vector3.zero; // Ẩn icon VS

        // 2. BẮT ĐẦU SEQUENCE ANIMATION
        Sequence seq = DOTween.Sequence();

        // Điểm dừng của bên trái (Ví dụ: cách lề trái 300 pixel)
        float leftEndX = 500f;
        // Điểm dừng của bên phải (Ví dụ: cách lề phải 300 pixel -> Tọa độ là âm)
        float rightEndX = -500f;

        // Bước A: Hai bên lao vào nhau (0.5 giây)
        seq.Append(leftContainer.DOAnchorPosX(leftEndX, 0.5f).SetEase(Ease.OutBack));
        seq.Join(rightContainer.DOAnchorPosX(rightEndX, 0.5f).SetEase(Ease.OutBack));

        seq.AppendCallback(() => PlaySFX(impactSFX)); // Phát âm thanh va chạm khi 2 bên chạm nhau

        // Bước B: Icon VS đập vào giữa (Ngay sau khi 2 bên lao vào)
        seq.Append(vsIcon.DOScale(1.5f, 0.3f).SetEase(Ease.OutBounce)); // Phóng to quá cỡ chút
        seq.Append(vsIcon.DOScale(1.0f, 0.2f)); // Thu về kích thước chuẩn

        // Bước C: Rung màn hình cho kịch tính
        seq.AppendCallback(() =>
        {
            transform.DOShakePosition(0.5f, 30f); // Rung cả cái panel
            // Ở đây có thể play âm thanh "BÙM" hoặc tiếng kiếm chém
        });

        // Bước D: Dừng lại 2 giây cho người chơi ngắm
        seq.AppendInterval(2.0f);

        // Bước E: Fade Out và kết thúc
        seq.Append(panelCanvasGroup.DOFade(0, 0.5f));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke(); // Báo cho GameManager biết là xong rồi, vào trận thôi!
        });
    }

    // Hàm tiện ích để phát âm thanh
    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip); // Dùng PlayOneShot để âm thanh chồng lên nhau được
        }
    }
}