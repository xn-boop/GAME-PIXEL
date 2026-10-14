using System;
using System.Collections.Generic;
using UnityEngine;

namespace JourneyOfMemories.Core.Data
{
    [Serializable]
    public class ItemInfo
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public Sprite Icon;

        public ItemInfo(string itemId, string displayName, string description, Sprite icon = null)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Description = description;
            Icon = icon;
        }
    }

    public static class DefaultItemDatabase
    {
        private static readonly Dictionary<string, ItemInfo> Items = new Dictionary<string, ItemInfo>
        {
            { "Food_ThitKhoTrung", new ItemInfo("Food_ThitKhoTrung", "Thịt kho trứng", "Món ăn ấm cúng mẹ nấu cho cả nhà.") },
            { "Food_Kimchi", new ItemInfo("Food_Kimchi", "Hũ Kim chi", "Hũ thức ăn lên men quen thuộc của ba.") },
            { "Food_ComCuon", new ItemInfo("Food_ComCuon", "Đĩa Kimbap", "Đĩa kimbap thơm ngon xếp cẩn thận.") },
            { "Clothing_AoDai", new ItemInfo("Clothing_AoDai", "Áo dài truyền thống", "Tà áo dài duyên dáng của mẹ trong ảnh.") },
            { "Clothing_Hanbok", new ItemInfo("Clothing_Hanbok", "Hanbok nam", "Bộ Hanbok trang trọng của ba.") },
            { "Gem_LotusHalf", new ItemInfo("Gem_LotusHalf", "Nửa ngọc Hoa Sen", "Nửa ngọc chạm khắc họa tiết hoa sen thanh khiết.") },
            { "Gem_MugunghwaHalf", new ItemInfo("Gem_MugunghwaHalf", "Nửa ngọc Mugunghwa", "Nửa ngọc chạm khắc quốc hoa Hàn Quốc.") },
            { "Gem_Whole", new ItemInfo("Gem_Whole", "Viên ngọc hoàn chỉnh", "Viên ngọc hòa hợp biểu tượng cho hai cội nguồn.") }
        };

        public static ItemInfo GetItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            if (Items.TryGetValue(itemId, out var info))
            {
                return info;
            }
            return new ItemInfo(itemId, itemId, string.Empty);
        }

        public static string GetDisplayName(string itemId)
        {
            var info = GetItem(itemId);
            return info != null ? info.DisplayName : itemId;
        }
    }
}
