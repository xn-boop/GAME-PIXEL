public enum L2_ItemType
{
    AoDai = 0,
    Hanbok = 1,
    NonLa = 2,
    MuGat = 3,

    // Aliases with exact ID strings
    Item_AoDai = 0,
    Item_Hanbok = 1,
    Item_NonLa = 2,
    Item_MuGat = 3
}

public static class L2_ItemConstants
{
    public const string Item_AoDai = "Item_AoDai";
    public const string Item_Hanbok = "Item_Hanbok";
    public const string Item_NonLa = "Item_NonLa";
    public const string Item_MuGat = "Item_MuGat";

    public static string ToItemId(this L2_ItemType type)
    {
        switch (type)
        {
            case L2_ItemType.AoDai: return Item_AoDai;
            case L2_ItemType.Hanbok: return Item_Hanbok;
            case L2_ItemType.NonLa: return Item_NonLa;
            case L2_ItemType.MuGat: return Item_MuGat;
            default: return type.ToString();
        }
    }
}
