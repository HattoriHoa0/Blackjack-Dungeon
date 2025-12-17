using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string id;
    public string itemName;
    public int price;
    public Sprite icon;
    [TextArea] public string description;

    // Hàm dùng Item (Trả về true nếu dùng thành công, false nếu không dùng được)
    public abstract bool OnUse(GameManager gm);
}