namespace JourneyOfMemories.Core.Data
{
    /// <summary>
    /// Registry ID dùng chung giữa Core, scene và UI. Không dùng display name làm khóa logic.
    /// </summary>
    public static class GameIds
    {
        public static class Quiz
        {
            public const string Q01 = "Q01";
            public const string Q02 = "Q02";
            public const string Q03 = "Q03";
            public const string Q04 = "Q04";
            public const string Q05 = "Q05";
            public const string Q06 = "Q06";
            public const string Q07 = "Q07";
        }

        public static class Item
        {
            public const string ThitKhoTrung = "Food_ThitKhoTrung";
            public const string Kimchi = "Food_Kimchi";
            public const string ComCuon = "Food_ComCuon";
            public const string AoDai = "Clothing_AoDai";
            public const string Hanbok = "Clothing_Hanbok";
            public const string LotusHalf = "Gem_LotusHalf";
            public const string MugunghwaHalf = "Gem_MugunghwaHalf";
            public const string WholeGem = "Gem_Whole";
        }

        public static class Slot
        {
            public const string ThitKhoTrung = "Slot_ThitKhoTrung";
            public const string Kimchi = "Slot_Kimchi";
            public const string ComCuon = "Slot_ComCuon";
            public const string Mother = "Slot_Mother";
            public const string Father = "Slot_Father";
            public const string Pedestal = "Slot_Pedestal";
        }
    }
}
