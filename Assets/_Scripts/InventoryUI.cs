using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Button backpackButton;
    public Button overlayButton;
    public Transform itemsContainer; // [MỚI] Nơi chứa các ô item (Grid Layout)

    [Header("Data References")]
    public GameManager gameManager;  // [MỚI] Cần cái này để lấy dữ liệu túi đồ
    public GameObject itemSlotPrefab;// [MỚI] Prefab ô đồ để sinh ra

    [Header("Animation Settings")]
    public float animDuration = 0.3f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private bool isOpen = false;

    void Start()
    {
        isOpen = false;
        inventoryPanel.SetActive(false);
        overlayButton.gameObject.SetActive(false);
        inventoryPanel.transform.localScale = Vector3.zero;

        backpackButton.onClick.AddListener(ToggleInventory);
        overlayButton.onClick.AddListener(CloseInventory);
    }

    public void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    void OpenInventory()
    {
        if (isOpen) return;
        isOpen = true;

        // [MỚI] CẬP NHẬT LẠI GIAO DIỆN TRƯỚC KHI MỞ
        RefreshInventoryUI();

        overlayButton.gameObject.SetActive(true);
        inventoryPanel.SetActive(true);

        inventoryPanel.transform.DOKill();
        inventoryPanel.transform.DOScale(1f, animDuration).SetEase(openEase);

        CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, animDuration);
        }
    }

    void CloseInventory()
    {
        if (!isOpen) return;
        isOpen = false;

        overlayButton.gameObject.SetActive(false);

        inventoryPanel.transform.DOKill();
        inventoryPanel.transform.DOScale(0f, animDuration * 0.8f)
            .SetEase(closeEase)
            .OnComplete(() =>
            {
                inventoryPanel.SetActive(false);
            });

        CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.DOFade(0, animDuration * 0.8f);
    }

    // [MỚI] HÀM VẼ LẠI TÚI ĐỒ
    public void RefreshInventoryUI()
    {
        // 1. Xóa hết các ô cũ đi
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Lấy danh sách đồ từ Player
        List<ItemData> inventory = gameManager.player.inventory;

        // 3. Tạo ô mới cho từng món đồ
        foreach (ItemData item in inventory)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
            ItemSlot slotScript = slotObj.GetComponent<ItemSlot>();

            // Setup ở chế độ Balo (isShop = false)
            slotScript.Setup(item, gameManager, false);
        }
    }
}