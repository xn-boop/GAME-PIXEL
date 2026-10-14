using UnityEngine;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Core
{
    public static class CoreLogicBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCoreLogic()
        {
            if (GameStateManager.Instance != null) return;

            Debug.Log("[CoreLogicBootstrap] Khởi tạo hệ sinh thái Core Logic tự động...");

            GameObject coreRoot = new GameObject("[CORE_LOGIC_SYSTEM]");
            Object.DontDestroyOnLoad(coreRoot);

            coreRoot.AddComponent<GameStateManager>();
            coreRoot.AddComponent<InventoryManager>();
            coreRoot.AddComponent<QuizManager>();
            coreRoot.AddComponent<PlacementManager>();
            coreRoot.AddComponent<CombineManager>();
            coreRoot.AddComponent<SaveLoadManager>();

            Debug.Log("[CoreLogicBootstrap] Tất cả các Manager lõi đã sẵn sàng!");
        }
    }
}
