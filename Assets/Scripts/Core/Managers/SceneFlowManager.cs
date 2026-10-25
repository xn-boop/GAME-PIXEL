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
#if UNITY_EDITOR
            Instance = this;
#else
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#endif
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public string GetSceneName(MapId mapId)
        {
            switch (mapId)
            {
                case MapId.Map1_Kitchen:
                    return "Map1";
                case MapId.Map2_DressingRoom: return "Scene_Level2";
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
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[SceneFlowManager] Không tìm thấy tên scene tương ứng cho {mapId}.");
                return false;
            }

            bool canLoad = Application.CanStreamedLevelBeLoaded(sceneName);
            if (!canLoad && !sceneName.EndsWith(".unity"))
            {
                canLoad = Application.CanStreamedLevelBeLoaded($"Assets/Scenes/{sceneName}.unity");
            }

#if UNITY_EDITOR
            if (!canLoad)
            {
                foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
                {
                    if (scene.enabled && (scene.path.EndsWith($"{sceneName}.unity") || System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName))
                    {
                        canLoad = true;
                        break;
                    }
                }
            }
#endif

            if (!canLoad)
            {
                Debug.LogError($"[SceneFlowManager] Scene '{sceneName}' chưa có trong Build Settings.");
                return false;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return true;
            }
#endif
            SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}
