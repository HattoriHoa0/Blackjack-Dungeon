using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject shopPanel;
    public Transform itemsContainer; // Nơi chứa các nút item
    public GameObject itemSlotPrefab; // Prefab ItemSlot chúng ta đã làm
    public TextMeshProUGUI playerGoldText;

    [Header("Data")]
    public List<ItemData> allPossibleItems; // Kéo tất cả item vào đây (trừ bình máu)
    public ItemData healthPotion; // Kéo riêng bình máu vào đây để đảm bảo luôn xuất hiện

    void Start()
    {
        // Ẩn shop lúc đầu
        if (shopPanel) shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateGoldUI();
        GenerateShopItems();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        // Gọi sang GameManager để sang màn chơi tiếp theo
        gameManager.ProceedToNextLevel();
    }

    void GenerateShopItems()
    {
        // 1. Xóa đồ cũ trong shop (nếu có)
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);

        // 2. Tạo danh sách hàng hóa đợt này
        List<ItemData> shopStock = new List<ItemData>();

        // Slot 1: Luôn là Bình Máu
        shopStock.Add(healthPotion);

        // Slot 2-6: Random item (cho phép trùng lặp)
        for (int i = 0; i < 5; i++)
        {
            if (allPossibleItems.Count > 0)
            {
                ItemData randomItem = allPossibleItems[Random.Range(0, allPossibleItems.Count)];
                shopStock.Add(randomItem);
            }
        }

        // 3. Sinh ra UI cho từng món
        foreach (ItemData item in shopStock)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
            ItemSlot slotScript = slotObj.GetComponent<ItemSlot>();

            // Setup slot ở chế độ Shop (true)
            slotScript.Setup(item, gameManager, true);

            //XỬ LÝ GIÁ TIỀN CHO PAIN
            ItemData itemDisplay = item; // Biến tạm

            // Kiểm tra logic
            if (gameManager.GetCurrentHero() != null &&
                gameManager.GetCurrentHero().passiveType == HeroPassiveType.DiscountSpyglass &&
                item.id == "spyglass") // Đảm bảo Item Kính soi của bạn có id là "spyglass"
            {
                // Clone ra item ảo để hiển thị giá giảm (tránh sửa vào file gốc)
                itemDisplay = Instantiate(item);
                itemDisplay.price = Mathf.Max(0, item.price - 10); // Giảm 10 vàng
            }

            // Setup slot với itemDisplay (giá đã giảm)
            slotScript.Setup(itemDisplay, gameManager, true);
        }
    }

    // Hàm mua đồ (Được gọi từ ItemSlot khi click)
    public void TryBuyItem(ItemData item, ItemSlot slotUI)
    {
        if (gameManager.player.currentGold >= item.price)
        {
            if (!gameManager.player.IsInventoryFull())
            {
                // Trừ tiền
                gameManager.player.AddGold(-item.price);

                // Thêm vào túi
                gameManager.player.AddItem(item);

                // Cập nhật UI
                UpdateGoldUI();

                Destroy(slotUI.gameObject); //xóa luôn cái slot đó khỏi màn hình shop

                Debug.Log("Mua thành công!");
            }
            else
            {
                gameManager.UpdateResultText("<color=red>TÚI ĐỒ ĐÃ ĐẦY (6/6)!</color>");
            }
        }
        else
        {
            gameManager.UpdateResultText("<color=red>KHÔNG ĐỦ TIỀN!</color>");
        }
    }

    void UpdateGoldUI()
    {
        if (playerGoldText) playerGoldText.text = "Vàng: " + gameManager.player.currentGold;
    }
}