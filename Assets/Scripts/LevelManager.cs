using System;
using UnityEngine;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

/// <summary>
/// Adapter tương thích cho UI Map 1. Không sở hữu hoặc quyết định tiến độ.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int totalSlots = 3;
    public int CompletedSlots => CountCompletedSlots();
    public int TotalSlots => totalSlots;
    public bool IsLevelCompleted => GameStateManager.Instance != null &&
                                    GameStateManager.Instance.CurrentMap == MapId.Map1_Kitchen &&
                                    GameStateManager.Instance.CurrentMapState == MapState.Completed;

    public event Action<int, int> OnProgressChanged;
    public event Action OnLevelCompleted;

    private void OnEnable()
    {
        if (PlacementManager.Instance != null) PlacementManager.Instance.OnItemPlaced += OnItemPlaced;
        if (GameStateManager.Instance != null) GameStateManager.Instance.OnMapCompleted += OnMapCompleted;
    }

    private void Start() => OnProgressChanged?.Invoke(CompletedSlots, TotalSlots);

    private void OnDisable()
    {
        if (PlacementManager.Instance != null) PlacementManager.Instance.OnItemPlaced -= OnItemPlaced;
        if (GameStateManager.Instance != null) GameStateManager.Instance.OnMapCompleted -= OnMapCompleted;
    }

    private void OnItemPlaced(string slotId, string itemId)
    {
        if (slotId == GameIds.Slot.ThitKhoTrung || slotId == GameIds.Slot.Kimchi || slotId == GameIds.Slot.ComCuon)
            OnProgressChanged?.Invoke(CompletedSlots, TotalSlots);
    }

    private void OnMapCompleted(MapId mapId)
    {
        if (mapId == MapId.Map1_Kitchen) OnLevelCompleted?.Invoke();
    }

    [Obsolete("Progress is owned by PlacementManager/GameStateManager.")]
    public void RegisterCompletedSlot() => OnProgressChanged?.Invoke(CompletedSlots, TotalSlots);

    private int CountCompletedSlots()
    {
        var placement = PlacementManager.Instance;
        if (placement == null) return 0;
        int count = 0;
        if (placement.IsSlotFilled(GameIds.Slot.ThitKhoTrung)) count++;
        if (placement.IsSlotFilled(GameIds.Slot.Kimchi)) count++;
        if (placement.IsSlotFilled(GameIds.Slot.ComCuon)) count++;
        return count;
    }
}
