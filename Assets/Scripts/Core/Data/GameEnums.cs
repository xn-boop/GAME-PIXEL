namespace JourneyOfMemories.Core.Data
{
    public enum MapId
    {
        Map1_Kitchen,
        Map2_DressingRoom,
        Map3_Garden,
        Ending
    }

    public enum MapState
    {
        NotStarted,
        Active,
        MemoryPending,
        JournalPending,
        Completed
    }

    public enum PlacementResult
    {
        Success,
        ItemMismatch,
        SlotAlreadyFilled,
        ItemNotInInventory,
        InvalidSlot
    }

    public enum QuizOption
    {
        None = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4
    }
}
