using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc để dùng IPointerEnter

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    [TextArea] public string content;

    // Hàm gọi khi chuột bay vào
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Gọi hệ thống để hiện bảng
        TooltipSystem.current.Show(content, header);
    }

    // Hàm gọi khi chuột bay ra
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.current.Hide();
    }
}