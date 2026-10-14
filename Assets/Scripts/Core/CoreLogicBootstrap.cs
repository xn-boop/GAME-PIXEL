using UnityEngine;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Core
{
    public static class CoreLogicBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCoreLogic()
        {
            Debug.Log("[CoreLogicBootstrap] Khởi tạo hệ sinh thái Core Logic tự động...");

            GameObject coreRoot = new GameObject("[CORE_LOGIC_SYSTEM]");
            Object.DontDestroyOnLoad(coreRoot);

            if (GameStateManager.Instance == null) coreRoot.AddComponent<GameStateManager>();
            if (InventoryManager.Instance == null) coreRoot.AddComponent<InventoryManager>();
            if (QuizManager.Instance == null) coreRoot.AddComponent<QuizManager>();
            if (PlacementManager.Instance == null) coreRoot.AddComponent<PlacementManager>();
            if (CombineManager.Instance == null) coreRoot.AddComponent<CombineManager>();
            if (SaveLoadManager.Instance == null) coreRoot.AddComponent<SaveLoadManager>();
            if (SceneFlowManager.Instance == null) coreRoot.AddComponent<SceneFlowManager>();

            Debug.Log("[CoreLogicBootstrap] Tất cả các Manager lõi đã sẵn sàng!");
        }
    }
}
