using UnityEngine;
using UnityEngine.SceneManagement;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    /// <summary>Điểm duy nhất quyết định map ID tương ứng scene nào.</summary>
    public class SceneFlowManager : MonoBehaviour
    {
        public static SceneFlowManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public string GetSceneName(MapId mapId)
        {
            switch (mapId)
            {
                case MapId.Map1_Kitchen: return "Level1_Prototype";
                case MapId.Map2_DressingRoom: return "Map2_DressingRoom";
                case MapId.Map3_Garden: return "Map3_Garden";
                case MapId.Ending: return "Ending";
                default: return string.Empty;
            }
        }

        public bool LoadCurrentMap()
        {
            if (GameStateManager.Instance == null) return false;
            return LoadMapScene(GameStateManager.Instance.CurrentMap);
        }

        public bool RequestAndLoad(MapId targetMap)
        {
            return GameStateManager.Instance != null &&
                   GameStateManager.Instance.RequestTransition(targetMap) &&
                   LoadMapScene(targetMap);
        }

        private bool LoadMapScene(MapId mapId)
        {
            string sceneName = GetSceneName(mapId);
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneFlowManager] Scene '{sceneName}' chưa có trong Build Settings.");
                return false;
            }
            SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}
