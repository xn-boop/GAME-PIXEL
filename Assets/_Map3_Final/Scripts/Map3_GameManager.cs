using UnityEngine;

namespace Game.Map3
{
    public class Map3_GameManager : MonoBehaviour
    {
        private static Map3_GameManager instance;

        public static Map3_GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindFirstObjectByType<Map3_GameManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("Map3_GameManager");
                        instance = go.AddComponent<Map3_GameManager>();
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        [Header("Progression Flags")]
        public bool hasTalkedToCat = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
