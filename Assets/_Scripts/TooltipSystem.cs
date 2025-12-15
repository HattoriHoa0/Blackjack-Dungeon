using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    // Singleton: Để gọi được từ bất cứ đâu
    public static TooltipSystem current;

    public GameObject tooltipPanel;
    public TextMeshProUGUI contentText;
    public RectTransform rectTransform; // Để chỉnh vị trí

    void Awake()
    {
        current = this;
    }

    void Start()
    {
        // Ẩn lúc đầu
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        // Bảng luôn đi theo chuột
        if (tooltipPanel.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            // Dịch lên trên và sang phải một chút để không che chuột
            transform.position = mousePos + new Vector2(15, 15);
        }
    }

    public void Show(string content, string header = "")
    {
        if (tooltipPanel) tooltipPanel.SetActive(true);

        string finalText = content;
        if (!string.IsNullOrEmpty(header))
        {
            finalText = $"<b><size=120%>{header}</size></b>\n{content}";
        }

        if (contentText) contentText.text = finalText;

        // Cập nhật lại layout ngay lập tức để không bị nháy
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void Hide()
    {
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }
}