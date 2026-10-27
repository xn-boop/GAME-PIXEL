public enum L1_ItemType
{
    ThitKho = 0,
    Kimchi = 1,
    Kimbap = 2
}

public static class L1_ItemConstants
{
    public const string Item_ThitKho = "Item_ThitKho";
    public const string Item_Kimchi = "Item_Kimchi";
    public const string Item_Kimbap = "Item_Kimbap";

    public static string ToItemId(this L1_ItemType type)
    {
        switch (type)
        {
            case L1_ItemType.ThitKho: return Item_ThitKho;
            case L1_ItemType.Kimchi: return Item_Kimchi;
            case L1_ItemType.Kimbap: return Item_Kimbap;
            default: return type.ToString();
        }
    }

    public static string GetDisplayName(this L1_ItemType type)
    {
        switch (type)
        {
            case L1_ItemType.ThitKho: return "Thịt kho trứng";
            case L1_ItemType.Kimchi: return "Kim chi";
            case L1_ItemType.Kimbap: return "Kimbap";
            default: return "Món ăn";
        }
    }
}
