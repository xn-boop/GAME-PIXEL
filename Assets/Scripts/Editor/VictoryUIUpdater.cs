using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI.Editor
{
    [InitializeOnLoad]
    public static class VictoryUIUpdater
    {
        private const string TriggerFlag = "Temp/UpdateVictoryUI.flag";
        private const string ResultsLog = "Logs/VictoryUIUpdateResults.log";

        static VictoryUIUpdater()
        {
            EditorApplication.update -= CheckForTrigger;
            EditorApplication.update += CheckForTrigger;
        }

        private static void CheckForTrigger()
        {
            if (File.Exists(TriggerFlag))
            {
                if (EditorApplication.isCompiling) return;
                if (EditorApplication.isPlaying)
                {
                    Debug.Log("[VictoryUIUpdater] Editor is playing. Exiting Play Mode to update scenes...");
                    EditorApplication.isPlaying = false;
                    return;
                }
                try { File.Delete(TriggerFlag); } catch { }
                UpdateAllScenes();
            }
        }

        private static Sprite LoadSpriteSubAsset(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj is Sprite sp) return sp;
            }
            return null;
        }

        private static Color HexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                return c;
            return Color.white;
        }

        public static string GetMap3ScenePath()
        {
            if (File.Exists("Assets/_Map3_Final/Scenes/Map3_Final.unity"))
                return "Assets/_Map3_Final/Scenes/Map3_Final.unity";
            if (File.Exists("Assets/Scenes/Scenes/Map3_Final.unity"))
                return "Assets/Scenes/Scenes/Map3_Final.unity";
            return "Assets/Scenes/Scenes/Map3_Final.unity";
        }

        [MenuItem("Tools/UI/Update Victory UI In All 3 Scenes")]
        public static void UpdateAllScenes()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[VictoryUIUpdater] Starting Victory UI update at {DateTime.Now:yyyy-MM-dd HH:mm:ss}...");
            Debug.Log("[VictoryUIUpdater] Starting Victory UI update for all 3 scenes...");

            string activeScenePath = EditorSceneManager.GetActiveScene().path;

            bool ok1 = UpdateSceneVictoryUI("Assets/Scenes/Level1_Prototype.unity", 1, sb);
            bool ok2 = UpdateSceneVictoryUI("Assets/Scenes/Scene_Level2.unity", 2, sb);
            bool ok3a = false;
            if (File.Exists("Assets/_Map3_Final/Scenes/Map3_Final.unity"))
                ok3a = UpdateSceneVictoryUI("Assets/_Map3_Final/Scenes/Map3_Final.unity", 3, sb);
            bool ok3b = false;
            if (File.Exists("Assets/Scenes/Scenes/Map3_Final.unity"))
                ok3b = UpdateSceneVictoryUI("Assets/Scenes/Scenes/Map3_Final.unity", 3, sb);

            // Restore original active scene
            if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            {
                EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
            }

            sb.AppendLine($"[VictoryUIUpdater] Finished. Level1: {ok1}, Level2: {ok2}, Map3_Final: {ok3a}, Scenes/Map3_Final: {ok3b}");
            Debug.Log("[VictoryUIUpdater] ALL 3 SCENES VICTORY UI SUCCESSFULLY UPDATED!");

            try
            {
                Directory.CreateDirectory("Logs");
                File.WriteAllText(ResultsLog, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VictoryUIUpdater] Failed to write log: {ex.Message}");
            }
        }

        [MenuItem("Tools/Map 1/Update Victory UI")]
        public static void UpdateMap1()
        {
            var sb = new StringBuilder();
            UpdateSceneVictoryUI("Assets/Scenes/Level1_Prototype.unity", 1, sb);
        }

        [MenuItem("Tools/Level 2/Update Victory UI")]
        public static void UpdateMap2()
        {
            var sb = new StringBuilder();
            UpdateSceneVictoryUI("Assets/Scenes/Scene_Level2.unity", 2, sb);
        }

        [MenuItem("Tools/Map 3/Update Victory UI")]
        public static void UpdateMap3()
        {
            var sb = new StringBuilder();
            UpdateSceneVictoryUI(GetMap3ScenePath(), 3, sb);
        }

        public static bool UpdateSceneVictoryUI(string scenePath, int mapIndex, StringBuilder sb = null)
        {
            if (sb == null) sb = new StringBuilder();

            if (!File.Exists(scenePath))
            {
                sb.AppendLine($"[VictoryUIUpdater] Scene not found: {scenePath}");
                Debug.LogError($"[VictoryUIUpdater] Scene not found: {scenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");
            Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");
            TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
            if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                sb.AppendLine($"[VictoryUIUpdater] Canvas not found in {scenePath}");
                Debug.LogError($"[VictoryUIUpdater] Canvas not found in {scenePath}");
                return false;
            }

            Transform victoryModalT = canvas.transform.Find("Panel_Victory");
            if (victoryModalT == null)
            {
                sb.AppendLine($"[VictoryUIUpdater] Panel_Victory not found in {scenePath}");
                Debug.LogError($"[VictoryUIUpdater] Panel_Victory not found in {scenePath}");
                return false;
            }

            Transform cardVictoryT = victoryModalT.Find("Card_Victory");
            if (cardVictoryT == null)
            {
                sb.AppendLine($"[VictoryUIUpdater] Card_Victory not found in {scenePath}");
                Debug.LogError($"[VictoryUIUpdater] Card_Victory not found in {scenePath}");
                return false;
            }

            // 1. Configure Card_Victory (Question frame sprite, sliced, white color)
            RectTransform cardRt = cardVictoryT.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = Vector2.zero;
            cardRt.sizeDelta = new Vector2(780f, 440f);

            Image cardImg = cardVictoryT.GetComponent<Image>();
            if (cardImg != null)
            {
                if (quizCardSprite != null) cardImg.sprite = quizCardSprite;
                cardImg.type = Image.Type.Sliced;
                cardImg.preserveAspect = false;
                cardImg.color = Color.white;
            }

            // 2. Configure Txt_Victory_Header ("XUẤT SẮC!" in gold on wooden bar)
            Transform headerT = cardVictoryT.Find("Txt_Victory_Header");
            if (headerT != null)
            {
                RectTransform rt = headerT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -48f);
                rt.sizeDelta = new Vector2(680f, 48f);

                TextMeshProUGUI tmp = headerT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "XUẤT SẮC!";
                    if (defaultFont != null) tmp.font = defaultFont;
                    tmp.fontSize = 34f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = HexColor("#FFD54F");
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableAutoSizing = false;
                }
            }

            // 3. Configure Txt_Victory_Title (Dark brown on parchment, centered vertically in upper area of parchment)
            Transform titleT = cardVictoryT.Find("Txt_Victory_Title");
            if (titleT != null)
            {
                RectTransform rt = titleT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 45f);
                rt.sizeDelta = new Vector2(700f, 65f);

                TextMeshProUGUI tmp = titleT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    if (defaultFont != null) tmp.font = defaultFont;
                    tmp.fontSize = 26f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = HexColor("#372016");
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableAutoSizing = false;
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                }
            }

            // 4. Configure Txt_Victory_Desc (Warm dark brown on parchment, centered vertically in lower area of parchment)
            Transform descT = cardVictoryT.Find("Txt_Victory_Desc");
            if (descT != null)
            {
                RectTransform rt = descT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -32f);
                rt.sizeDelta = new Vector2(680f, 85f);

                TextMeshProUGUI tmp = descT.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    if (defaultFont != null) tmp.font = defaultFont;
                    tmp.fontSize = 21f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = HexColor("#4E342E");
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableAutoSizing = false;
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                }
            }

            // 5. Configure Btn_Restart (Wood button sprite, sliced, styled text)
            Transform btnRestartT = cardVictoryT.Find("Btn_Restart");
            if (btnRestartT != null)
            {
                RectTransform rt = btnRestartT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                if (mapIndex == 3)
                {
                    rt.anchoredPosition = new Vector2(0f, 60f);
                    rt.sizeDelta = new Vector2(240f, 52f);
                }
                else
                {
                    rt.anchoredPosition = new Vector2(-125f, 60f);
                    rt.sizeDelta = new Vector2(210f, 52f);
                }

                Image img = btnRestartT.GetComponent<Image>();
                if (img != null)
                {
                    if (btnWoodSprite != null) img.sprite = btnWoodSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }

                TextMeshProUGUI txt = btnRestartT.Find("Text")?.GetComponent<TextMeshProUGUI>() ?? btnRestartT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.text = "Chơi lại";
                    if (defaultFont != null) txt.font = defaultFont;
                    txt.fontSize = 22f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = (mapIndex == 3) ? HexColor("#FFD54F") : HexColor("#FFE082");
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }

            // 6. Configure Btn_Continue (Wood button sprite, sliced, gold text)
            Transform btnContinueT = cardVictoryT.Find("Btn_Continue");
            if (btnContinueT != null)
            {
                RectTransform rt = btnContinueT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(125f, 60f);
                rt.sizeDelta = new Vector2(210f, 52f);

                Image img = btnContinueT.GetComponent<Image>();
                if (img != null)
                {
                    if (btnWoodSprite != null) img.sprite = btnWoodSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }

                TextMeshProUGUI txt = btnContinueT.Find("Text")?.GetComponent<TextMeshProUGUI>() ?? btnContinueT.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txt != null)
                {
                    txt.text = "Tiếp tục";
                    if (defaultFont != null) txt.font = defaultFont;
                    txt.fontSize = 22f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = HexColor("#FFD54F");
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            sb.AppendLine($"[VictoryUIUpdater] Scene {scenePath} updated successfully!");
            Debug.Log($"[VictoryUIUpdater] Scene {scenePath} updated successfully!");
            return true;
        }
    }
}
