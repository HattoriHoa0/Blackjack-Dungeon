using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public Button button;

    // Tham chiếu đến Tooltip có sẵn của bạn
    public TooltipTrigger tooltipTrigger;

    private ItemData itemData;
    private bool isShopSlot; // True = Hàng trong shop (Mua), False = Hàng trong túi (Dùng)
    private GameManager gm;

    public void Setup(ItemData item, GameManager gameManager, bool isShop)
    {
        itemData = item;
        gm = gameManager;
        isShopSlot = isShop;

        // Cập nhật hình ảnh
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            button.interactable = true;

            // --- CÀI ĐẶT TOOLTIP (Tên & Mô tả) ---
            if (tooltipTrigger)
            {
                tooltipTrigger.header = item.itemName;

                string content = item.description;
                if (isShop) content += $"\n<color=yellow>Giá: {item.price} Gold</color>";

                tooltipTrigger.content = content;
            }
        }
        else
        {
            // Ô trống
            iconImage.enabled = false;
            button.interactable = false;
            if (tooltipTrigger) tooltipTrigger.content = "";
        }
    }

    // Hàm gọi khi người chơi bấm vào nút
    public void OnClick()
    {
        TooltipSystem.current.Hide();

        if (itemData == null) return;

        if (isShopSlot)
        {
            // [QUAN TRỌNG] Gọi hàm này để thực hiện trừ tiền và lấy đồ
            if (gm != null)
            {
                gm.TryBuyItemFromShop(itemData, this);
            }
            else
            {
                Debug.LogError("Chưa kết nối GameManager cho ItemSlot!");
            }
        }
        else
        {
            // Logic Dùng item trong balo
            if (gm != null) gm.TryUseItem(itemData);
        }
    }

    // Tích hợp với TooltipSystem cũ của bạn (nếu dùng chuột PC)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipTrigger) tooltipTrigger.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipTrigger) tooltipTrigger.OnPointerExit(eventData);
    }
}