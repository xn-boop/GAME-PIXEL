using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class PuzzelMenuSetup
    {
        private const string PREF_KEY = "PuzzelMenu_Scene_Created_v1";

        static PuzzelMenuSetup()
        {
            EditorApplication.delayCall += InitializeIfNeeded;
        }

        private static void InitializeIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string scenePath = "Assets/Scenes/Puzzel.unity";
            if (!File.Exists(scenePath))
            {
                Debug.Log("[PuzzelMenuSetup] Scene 'Puzzel' chưa tồn tại. Tự động khởi tạo...");
                BuildScene();
            }
        }

        [MenuItem("Tools/Build 'Puzzel' Scene")]
        public static void BuildScene()
        {
            Debug.Log("[PuzzelMenuSetup] Bắt đầu khởi tạo Scene 'Puzzel' tinh gọn (đã bỏ phần khoanh đỏ)...");

            // 1. Đảm bảo toàn bộ 30 frames và 9 tiles được import đúng chuẩn Sprite 2D
            EnsureSpritesImported();

            // 2. Tạo Scene mới
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Tạo Main Camera (chuẩn 2D Pixel Art)
            GameObject cameraObj = new GameObject("Main Camera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";

            // 4. Tạo EventSystem duy nhất (tránh duplicate)
            GameObject eventObj = new GameObject("EventSystem");
            EventSystem es = eventObj.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventObj.AddComponent<InputSystemUIInputModule>();

            // 5. Tạo Canvas (Reference Resolution 1896 x 830 pixel art)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1896, 830);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 6. Background Image & Frame Animator (30 frames hoa bay)
            GameObject bgObj = new GameObject("BackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.raycastTarget = false;

            // Tải 30 frames từ Assets/UI/PuzzelFrames
            List<Sprite> framesList = new List<Sprite>();
            for (int i = 0; i < 30; i++)
            {
                string framePath = $"Assets/UI/PuzzelFrames/frame_{i:D2}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
                if (sp == null)
                {
                    AssetDatabase.ImportAsset(framePath, ImportAssetOptions.ForceUpdate);
                    sp = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
                    if (sp == null)
                    {
                        var allAssets = AssetDatabase.LoadAllAssetsAtPath(framePath);
                        foreach (var a in allAssets)
                        {
                            if (a is Sprite spr) { sp = spr; break; }
                        }
                    }
                }

                if (sp != null)
                {
                    framesList.Add(sp);
                }
            }

            if (framesList.Count > 0)
            {
                bgImage.sprite = framesList[0];
            }

            UIFrameAnimator animator = bgObj.AddComponent<UIFrameAnimator>();
            animator.frames = framesList.ToArray();
            animator.framesPerSecond = 10f;
            animator.loop = true;
            animator.unscaledTime = true;

            // Font chữ hệ thống chuẩn Pixel
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                try { defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }

            Color goldTitleColor = new Color(0.96f, 0.85f, 0.60f, 1f);    // #F5D999
            Color lightGoldColor = new Color(1.0f, 0.90f, 0.66f, 1f);     // #FFE6A8
            Color cardBgColor = new Color(0.18f, 0.11f, 0.08f, 0.95f);    // #2E1C14
            Color btnNormalColor = new Color(0.28f, 0.16f, 0.11f, 1f);   // #47291C
            Color accentBtnColor = new Color(0.38f, 0.20f, 0.14f, 1f);   // #613324
            Color subtitleColor = new Color(0.85f, 0.78f, 0.72f, 0.95f);

            // Container chính của toàn bộ UI
            GameObject rootUIObj = new GameObject("PuzzelMenuUI", typeof(RectTransform));
            rootUIObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rootRect = rootUIObj.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // 7. Helper tạo nút UI với hiệu ứng RegisterButtonEffect
            Button CreateMenuButton(Transform parent, string name, Vector2 pos, Vector2 size, string text, Color bgColor, float chime)
            {
                GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
                btnObj.transform.SetParent(parent, false);
                RectTransform bRect = btnObj.GetComponent<RectTransform>();
                bRect.anchorMin = new Vector2(0.5f, 0.5f);
                bRect.anchorMax = new Vector2(0.5f, 0.5f);
                bRect.anchoredPosition = pos;
                bRect.sizeDelta = size;

                Image img = btnObj.GetComponent<Image>();
                img.color = bgColor;

                Button btn = btnObj.GetComponent<Button>();
                RegisterButtonEffect effect = btnObj.GetComponent<RegisterButtonEffect>();
                effect.chimeFrequency = chime;
                effect.punchDuration = 0.2f;
                effect.pressedScale = 0.94f;

                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform tr = txtObj.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.sizeDelta = Vector2.zero;

                Text txt = txtObj.GetComponent<Text>();
                txt.font = defaultFont;
                txt.fontSize = 19;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = lightGoldColor;
                txt.text = text;

                return btn;
            }

            // 8. Tương tác trực tiếp trên khung tranh (PortraitButton)
            // Nhấn vào bức tranh ở giữa để kích hoạt minigame ghép hình
            GameObject portraitObj = new GameObject("PortraitButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
            portraitObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform portRect = portraitObj.GetComponent<RectTransform>();
            portRect.anchorMin = new Vector2(0.5f, 0.5f);
            portRect.anchorMax = new Vector2(0.5f, 0.5f);
            portRect.anchoredPosition = new Vector2(-38f, 25f);
            portRect.sizeDelta = new Vector2(630f, 630f);

            Image portImg = portraitObj.GetComponent<Image>();
            portImg.color = new Color(1f, 1f, 1f, 0.001f); // Vùng tương tác trong suốt
            portImg.raycastTarget = true;

            Button portraitBtn = portraitObj.GetComponent<Button>();
            RegisterButtonEffect portEff = portraitObj.GetComponent<RegisterButtonEffect>();
            portEff.chimeFrequency = 1050f;
            portEff.punchDuration = 0.15f;

            // (Lưu ý: Bảng MainControlPanel bên trái đã được gỡ bỏ hoàn toàn theo yêu cầu khoanh đỏ của người dùng)

            // 9. Right Info Panel (Thẻ thông tin ký ức bên phải - Giữ nguyên không thay đổi)
            GameObject rightPanelObj = new GameObject("RightInfoPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rightPanelObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform rpRect = rightPanelObj.GetComponent<RectTransform>();
            rpRect.anchorMin = new Vector2(0.5f, 0.5f);
            rpRect.anchorMax = new Vector2(0.5f, 0.5f);
            rpRect.anchoredPosition = new Vector2(650f, 10f);
            rpRect.sizeDelta = new Vector2(360f, 520f);

            Image rpImg = rightPanelObj.GetComponent<Image>();
            rpImg.color = cardBgColor;

            GameObject rpTitleObj = new GameObject("PanelTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            rpTitleObj.transform.SetParent(rightPanelObj.transform, false);
            RectTransform rptRect = rpTitleObj.GetComponent<RectTransform>();
            rptRect.anchorMin = new Vector2(0.5f, 0.5f);
            rptRect.anchorMax = new Vector2(0.5f, 0.5f);
            rptRect.anchoredPosition = new Vector2(0f, 215f);
            rptRect.sizeDelta = new Vector2(320f, 40f);

            Text rptText = rpTitleObj.GetComponent<Text>();
            rptText.font = defaultFont;
            rptText.fontSize = 22;
            rptText.fontStyle = FontStyle.Bold;
            rptText.alignment = TextAnchor.MiddleCenter;
            rptText.color = goldTitleColor;
            rptText.text = "THÔNG ĐIỆP KÝ ỨC";

            GameObject quoteObj = new GameObject("QuoteText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            quoteObj.transform.SetParent(rightPanelObj.transform, false);
            RectTransform qRect = quoteObj.GetComponent<RectTransform>();
            qRect.anchorMin = new Vector2(0.5f, 0.5f);
            qRect.anchorMax = new Vector2(0.5f, 0.5f);
            qRect.anchoredPosition = new Vector2(0f, 65f);
            qRect.sizeDelta = new Vector2(310f, 230f);

            Text qText = quoteObj.GetComponent<Text>();
            qText.font = defaultFont;
            qText.fontSize = 15;
            qText.fontStyle = FontStyle.Italic;
            qText.alignment = TextAnchor.UpperLeft;
            qText.color = new Color(0.92f, 0.86f, 0.80f, 0.95f);
            qText.text = "“Nhiều năm qua, con đã luôn hoang mang vì nghĩ mình là một mảnh ghép lỗi, không vừa vặn với bức tranh của bất kỳ ai.\n\nNhưng khi đi lại những bước chân này, con đã hiểu. Một trái tim không cần chia đôi để thuộc về hai quê hương.\n\nCon là điểm giao thoa. Con là một nhịp cầu.”";

            GameObject infoBoxObj = new GameObject("DetailsBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            infoBoxObj.transform.SetParent(rightPanelObj.transform, false);
            RectTransform ibRect = infoBoxObj.GetComponent<RectTransform>();
            ibRect.anchorMin = new Vector2(0.5f, 0.5f);
            ibRect.anchorMax = new Vector2(0.5f, 0.5f);
            ibRect.anchoredPosition = new Vector2(0f, -125f);
            ibRect.sizeDelta = new Vector2(310f, 110f);

            Image ibImg = infoBoxObj.GetComponent<Image>();
            ibImg.color = new Color(0.12f, 0.07f, 0.05f, 0.90f);

            GameObject infoTxtObj = new GameObject("InfoText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            infoTxtObj.transform.SetParent(infoBoxObj.transform, false);
            RectTransform itRect = infoTxtObj.GetComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            itRect.offsetMin = new Vector2(12f, 10f);
            itRect.offsetMax = new Vector2(-12f, -10f);

            Text itText = infoTxtObj.GetComponent<Text>();
            itText.font = defaultFont;
            itText.fontSize = 14;
            itText.fontStyle = FontStyle.Normal;
            itText.alignment = TextAnchor.MiddleLeft;
            itText.color = subtitleColor;
            itText.text = "• Tác phẩm: Bức tranh sum vầy gia đình\n• Biểu tượng: Hoa Sen & Mugunghwa\n• Danh thắng: Tháp Rùa & Cung Cảnh Phúc\n• Hoạt ảnh: 30 Khung hình cánh hoa bay";

            // 10. Interactive Puzzle Board Minigame (Bảng ghép hình 3x3 ngay vị trí khung tranh trung tâm)
            GameObject puzzleBoardObj = new GameObject("PuzzleBoardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            puzzleBoardObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform pbRect = puzzleBoardObj.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.5f, 0.5f);
            pbRect.anchorMax = new Vector2(0.5f, 0.5f);
            pbRect.anchoredPosition = new Vector2(-38f, 25f);
            pbRect.sizeDelta = new Vector2(650f, 650f);

            Image pbImg = puzzleBoardObj.GetComponent<Image>();
            pbImg.color = new Color(0.10f, 0.05f, 0.04f, 0.96f);

            List<Sprite> tileSpriteList = new List<Sprite>();
            for (int i = 0; i < 9; i++)
            {
                string tPath = $"Assets/UI/PuzzelFrames/Tiles/tile_{i}.png";
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(tPath);
                if (sp == null)
                {
                    AssetDatabase.ImportAsset(tPath, ImportAssetOptions.ForceUpdate);
                    sp = AssetDatabase.LoadAssetAtPath<Sprite>(tPath);
                }
                tileSpriteList.Add(sp);
            }

            GameObject gridObj = new GameObject("TilesGrid", typeof(RectTransform));
            gridObj.transform.SetParent(puzzleBoardObj.transform, false);
            RectTransform gRect = gridObj.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0.5f, 0.5f);
            gRect.anchorMax = new Vector2(0.5f, 0.5f);
            gRect.anchoredPosition = new Vector2(0f, 20f);
            gRect.sizeDelta = new Vector2(612f, 510f);

            Button[] tileButtons = new Button[9];
            Image[] tileImages = new Image[9];

            float tileW = 200f;
            float tileH = 166f;
            float spacing = 6f;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    int idx = r * 3 + c;
                    float posX = (c - 1) * (tileW + spacing);
                    float posY = (1 - r) * (tileH + spacing);

                    GameObject tileObj = new GameObject($"Tile_{idx}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
                    tileObj.transform.SetParent(gridObj.transform, false);
                    RectTransform tr = tileObj.GetComponent<RectTransform>();
                    tr.anchorMin = new Vector2(0.5f, 0.5f);
                    tr.anchorMax = new Vector2(0.5f, 0.5f);
                    tr.anchoredPosition = new Vector2(posX, posY);
                    tr.sizeDelta = new Vector2(tileW, tileH);

                    Image img = tileObj.GetComponent<Image>();
                    img.color = Color.white;
                    if (idx < tileSpriteList.Count && tileSpriteList[idx] != null)
                    {
                        img.sprite = tileSpriteList[idx];
                    }

                    Button btn = tileObj.GetComponent<Button>();
                    RegisterButtonEffect eff = tileObj.GetComponent<RegisterButtonEffect>();
                    eff.chimeFrequency = 640f + idx * 40f;
                    eff.punchDuration = 0.15f;
                    eff.pressedScale = 0.95f;

                    tileButtons[idx] = btn;
                    tileImages[idx] = img;
                }
            }

            GameObject pbTopObj = new GameObject("TopStatsBar", typeof(RectTransform));
            pbTopObj.transform.SetParent(puzzleBoardObj.transform, false);
            RectTransform pbtRect = pbTopObj.GetComponent<RectTransform>();
            pbtRect.anchorMin = new Vector2(0.5f, 0.5f);
            pbtRect.anchorMax = new Vector2(0.5f, 0.5f);
            pbtRect.anchoredPosition = new Vector2(0f, 290f);
            pbtRect.sizeDelta = new Vector2(620f, 36f);

            GameObject movesObj = new GameObject("MoveCountText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            movesObj.transform.SetParent(pbTopObj.transform, false);
            RectTransform mRect = movesObj.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0f, 0.5f);
            mRect.anchorMax = new Vector2(0f, 0.5f);
            mRect.anchoredPosition = new Vector2(100f, 0f);
            mRect.sizeDelta = new Vector2(200f, 30f);

            Text movesTxt = movesObj.GetComponent<Text>();
            movesTxt.font = defaultFont;
            movesTxt.fontSize = 18;
            movesTxt.fontStyle = FontStyle.Bold;
            movesTxt.alignment = TextAnchor.MiddleLeft;
            movesTxt.color = goldTitleColor;
            movesTxt.text = "Số bước: 0";

            GameObject timerObj = new GameObject("TimerText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            timerObj.transform.SetParent(pbTopObj.transform, false);
            RectTransform tmRect = timerObj.GetComponent<RectTransform>();
            tmRect.anchorMin = new Vector2(1f, 0.5f);
            tmRect.anchorMax = new Vector2(1f, 0.5f);
            tmRect.anchoredPosition = new Vector2(-100f, 0f);
            tmRect.sizeDelta = new Vector2(200f, 30f);

            Text timerTxt = timerObj.GetComponent<Text>();
            timerTxt.font = defaultFont;
            timerTxt.fontSize = 18;
            timerTxt.fontStyle = FontStyle.Bold;
            timerTxt.alignment = TextAnchor.MiddleRight;
            timerTxt.color = goldTitleColor;
            timerTxt.text = "Thời gian: 00:00";

            GameObject pbBotObj = new GameObject("BottomControls", typeof(RectTransform));
            pbBotObj.transform.SetParent(puzzleBoardObj.transform, false);
            RectTransform pbbRect = pbBotObj.GetComponent<RectTransform>();
            pbbRect.anchorMin = new Vector2(0.5f, 0.5f);
            pbbRect.anchorMax = new Vector2(0.5f, 0.5f);
            pbbRect.anchoredPosition = new Vector2(0f, -275f);
            pbbRect.sizeDelta = new Vector2(620f, 50f);

            Button shuffleBtn = CreateMenuButton(pbBotObj.transform, "ShuffleButton", new Vector2(-225f, 0f), new Vector2(140f, 44f), "XÁO TRỘN", accentBtnColor, 520f);
            Button hintBtn = CreateMenuButton(pbBotObj.transform, "HintButton", new Vector2(-75f, 0f), new Vector2(130f, 44f), "GỢI Ý", btnNormalColor, 880f);
            Button solveBtn = CreateMenuButton(pbBotObj.transform, "AutoSolveButton", new Vector2(75f, 0f), new Vector2(140f, 44f), "TỰ ĐỘNG GIẢI", btnNormalColor, 950f);
            Button closePbBtn = CreateMenuButton(pbBotObj.transform, "CloseBoardButton", new Vector2(225f, 0f), new Vector2(130f, 44f), "QUAY LẠI", new Color(0.35f, 0.16f, 0.12f, 1f), 650f);

            GameObject victoryObj = new GameObject("VictoryBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            victoryObj.transform.SetParent(puzzleBoardObj.transform, false);
            RectTransform vRect = victoryObj.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.5f, 0.5f);
            vRect.anchorMax = new Vector2(0.5f, 0.5f);
            vRect.anchoredPosition = new Vector2(0f, 20f);
            vRect.sizeDelta = new Vector2(580f, 100f);

            Image vImg = victoryObj.GetComponent<Image>();
            vImg.color = new Color(0.12f, 0.08f, 0.04f, 0.94f);

            GameObject vTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            vTextObj.transform.SetParent(victoryObj.transform, false);
            RectTransform vtRect = vTextObj.GetComponent<RectTransform>();
            vtRect.anchorMin = Vector2.zero;
            vtRect.anchorMax = Vector2.one;
            vtRect.sizeDelta = Vector2.zero;

            Text vTxt = vTextObj.GetComponent<Text>();
            vTxt.font = defaultFont;
            vTxt.fontSize = 24;
            vTxt.fontStyle = FontStyle.Bold;
            vTxt.alignment = TextAnchor.MiddleCenter;
            vTxt.color = goldTitleColor;
            vTxt.text = "XUẤT SẮC!\nBỨC TRANH KÝ ỨC ĐÃ HOÀN TẤT TRỌN VẸN!";

            victoryObj.SetActive(false);
            puzzleBoardObj.SetActive(false);

            // 11. Settings Panel (Bảng Cài Đặt)
            GameObject settingsPanelObj = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            settingsPanelObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform spRect = settingsPanelObj.GetComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0.5f, 0.5f);
            spRect.anchorMax = new Vector2(0.5f, 0.5f);
            spRect.anchoredPosition = Vector2.zero;
            spRect.sizeDelta = new Vector2(640f, 440f);

            Image spImg = settingsPanelObj.GetComponent<Image>();
            spImg.color = cardBgColor;

            GameObject stTitleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            stTitleObj.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform sttRect = stTitleObj.GetComponent<RectTransform>();
            sttRect.anchorMin = new Vector2(0.5f, 0.5f);
            sttRect.anchorMax = new Vector2(0.5f, 0.5f);
            sttRect.anchoredPosition = new Vector2(0f, 165f);
            sttRect.sizeDelta = new Vector2(500f, 50f);

            Text sttText = stTitleObj.GetComponent<Text>();
            sttText.font = defaultFont;
            sttText.fontSize = 30;
            sttText.fontStyle = FontStyle.Bold;
            sttText.alignment = TextAnchor.MiddleCenter;
            sttText.color = goldTitleColor;
            sttText.text = "CÀI ĐẶT";

            Slider CreateVolumeSlider(string name, Vector2 pos, string labelText, float defaultValue)
            {
                GameObject lblObj = new GameObject(name + "_Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                lblObj.transform.SetParent(settingsPanelObj.transform, false);
                RectTransform lblRect = lblObj.GetComponent<RectTransform>();
                lblRect.anchorMin = new Vector2(0.5f, 0.5f);
                lblRect.anchorMax = new Vector2(0.5f, 0.5f);
                lblRect.anchoredPosition = new Vector2(pos.x - 70f, pos.y + 30f);
                lblRect.sizeDelta = new Vector2(380f, 30f);

                Text lbl = lblObj.GetComponent<Text>();
                lbl.font = defaultFont;
                lbl.fontSize = 20;
                lbl.fontStyle = FontStyle.Bold;
                lbl.color = lightGoldColor;
                lbl.alignment = TextAnchor.MiddleLeft;
                lbl.text = labelText;

                GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(settingsPanelObj.transform, false);
                RectTransform sRect = sliderObj.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.5f, 0.5f);
                sRect.anchorMax = new Vector2(0.5f, 0.5f);
                sRect.anchoredPosition = pos;
                sRect.sizeDelta = new Vector2(460f, 24f);

                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bg.transform.SetParent(sliderObj.transform, false);
                RectTransform bgR = bg.GetComponent<RectTransform>();
                bgR.anchorMin = new Vector2(0f, 0.25f);
                bgR.anchorMax = new Vector2(1f, 0.75f);
                bgR.sizeDelta = Vector2.zero;
                Image bgI = bg.GetComponent<Image>();
                bgI.color = new Color(0.10f, 0.06f, 0.04f, 0.9f);

                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(sliderObj.transform, false);
                RectTransform faR = fillArea.GetComponent<RectTransform>();
                faR.anchorMin = new Vector2(0f, 0.25f);
                faR.anchorMax = new Vector2(1f, 0.75f);
                faR.sizeDelta = new Vector2(-20f, 0f);

                GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fill.transform.SetParent(fillArea.transform, false);
                RectTransform fillR = fill.GetComponent<RectTransform>();
                fillR.sizeDelta = Vector2.zero;
                Image fillI = fill.GetComponent<Image>();
                fillI.color = new Color(0.85f, 0.55f, 0.22f, 1f);

                GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
                handleArea.transform.SetParent(sliderObj.transform, false);
                RectTransform haR = handleArea.GetComponent<RectTransform>();
                haR.anchorMin = Vector2.zero;
                haR.anchorMax = Vector2.one;
                haR.sizeDelta = new Vector2(-20f, 0f);

                GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                handle.transform.SetParent(handleArea.transform, false);
                RectTransform hR = handle.GetComponent<RectTransform>();
                hR.sizeDelta = new Vector2(24f, 28f);
                Image handleI = handle.GetComponent<Image>();
                handleI.color = goldTitleColor;

                Slider s = sliderObj.GetComponent<Slider>();
                s.fillRect = fillR;
                s.handleRect = hR;
                s.targetGraphic = handleI;
                s.direction = Slider.Direction.LeftToRight;
                s.minValue = 0f;
                s.maxValue = 1f;
                s.value = defaultValue;

                return s;
            }

            Slider musicSlider = CreateVolumeSlider("VolumeSlider", new Vector2(0f, 60f), "Âm lượng Nhạc nền (BGM)", 0.75f);
            Slider sfxSlider = CreateVolumeSlider("SfxSlider", new Vector2(0f, -30f), "Âm thanh Hiệu ứng (SFX)", 0.85f);
            Button closeSetBtn = CreateMenuButton(settingsPanelObj.transform, "CloseSettingsButton", new Vector2(0f, -145f), new Vector2(240f, 52f), "QUAY LẠI", btnNormalColor, 750f);

            settingsPanelObj.SetActive(false);

            // 12. Puzzle Collection Panel (Bảng Bộ Sưu Tập Tranh Ghép)
            GameObject collectionPanelObj = new GameObject("PuzzleCollectionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            collectionPanelObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform cpRect = collectionPanelObj.GetComponent<RectTransform>();
            cpRect.anchorMin = new Vector2(0.5f, 0.5f);
            cpRect.anchorMax = new Vector2(0.5f, 0.5f);
            cpRect.anchoredPosition = Vector2.zero;
            cpRect.sizeDelta = new Vector2(780f, 540f);

            Image cpImg = collectionPanelObj.GetComponent<Image>();
            cpImg.color = cardBgColor;

            GameObject cpTitleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            cpTitleObj.transform.SetParent(collectionPanelObj.transform, false);
            RectTransform cptRect = cpTitleObj.GetComponent<RectTransform>();
            cptRect.anchorMin = new Vector2(0.5f, 0.5f);
            cptRect.anchorMax = new Vector2(0.5f, 0.5f);
            cptRect.anchoredPosition = new Vector2(0f, 215f);
            cptRect.sizeDelta = new Vector2(600f, 45f);

            Text cptText = cpTitleObj.GetComponent<Text>();
            cptText.font = defaultFont;
            cptText.fontSize = 28;
            cptText.fontStyle = FontStyle.Bold;
            cptText.alignment = TextAnchor.MiddleCenter;
            cptText.color = goldTitleColor;
            cptText.text = "BỘ SƯU TẬP TRANH GHÉP";

            GameObject cpSubObj = new GameObject("Subtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            cpSubObj.transform.SetParent(collectionPanelObj.transform, false);
            RectTransform cpsRect = cpSubObj.GetComponent<RectTransform>();
            cpsRect.anchorMin = new Vector2(0.5f, 0.5f);
            cpsRect.anchorMax = new Vector2(0.5f, 0.5f);
            cpsRect.anchoredPosition = new Vector2(0f, 175f);
            cpsRect.sizeDelta = new Vector2(600f, 30f);

            Text cpsText = cpSubObj.GetComponent<Text>();
            cpsText.font = defaultFont;
            cpsText.fontSize = 17;
            cpsText.fontStyle = FontStyle.Normal;
            cpsText.alignment = TextAnchor.MiddleCenter;
            cpsText.color = subtitleColor;
            cpsText.text = "Khám phá các bức tranh ký ức của Hạ Ân";

            Button[] colButtons = new Button[4];
            colButtons[0] = CreateMenuButton(collectionPanelObj.transform, "Item0", new Vector2(0f, 105f), new Vector2(660f, 54f), "1. BỨC TRANH SUM VẦY (ĐANG CHỌN - 30 FRAMES)", accentBtnColor, 900f);
            colButtons[1] = CreateMenuButton(collectionPanelObj.transform, "Item1", new Vector2(0f, 40f), new Vector2(660f, 54f), "2. NỬA NGỌC BỘI HOA SEN (VIỆT NAM)", btnNormalColor, 1020f);
            colButtons[2] = CreateMenuButton(collectionPanelObj.transform, "Item2", new Vector2(0f, -25f), new Vector2(660f, 54f), "3. NỬA NGỌC BỘI MUGUNGHWA (HÀN QUỐC)", btnNormalColor, 1140f);
            colButtons[3] = CreateMenuButton(collectionPanelObj.transform, "Item3", new Vector2(0f, -90f), new Vector2(660f, 54f), "4. CÂY CẦU KÝ ỨC (GIAO THOA HAI CỘI NGUỒN)", btnNormalColor, 1260f);

            Button closeColBtn = CreateMenuButton(collectionPanelObj.transform, "CloseCollectionButton", new Vector2(0f, -185f), new Vector2(240f, 52f), "QUAY LẠI", btnNormalColor, 750f);
            collectionPanelObj.SetActive(false);

            // 13. Exit View Art Mode Button (Nút thoát xem tranh toàn cảnh)
            GameObject exitArtObj = new GameObject("ExitViewArtButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
            exitArtObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform eaRect = exitArtObj.GetComponent<RectTransform>();
            eaRect.anchorMin = new Vector2(0.5f, 0.5f);
            eaRect.anchorMax = new Vector2(0.5f, 0.5f);
            eaRect.anchoredPosition = new Vector2(780f, 360f);
            eaRect.sizeDelta = new Vector2(200f, 48f);

            Image eaImg = exitArtObj.GetComponent<Image>();
            eaImg.color = cardBgColor;

            GameObject eaTxtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            eaTxtObj.transform.SetParent(exitArtObj.transform, false);
            RectTransform eatRect = eaTxtObj.GetComponent<RectTransform>();
            eatRect.anchorMin = Vector2.zero;
            eatRect.anchorMax = Vector2.one;
            eatRect.sizeDelta = Vector2.zero;

            Text eatText = eaTxtObj.GetComponent<Text>();
            eatText.font = defaultFont;
            eatText.fontSize = 17;
            eatText.fontStyle = FontStyle.Bold;
            eatText.alignment = TextAnchor.MiddleCenter;
            eatText.color = goldTitleColor;
            eatText.text = "TRỞ LẠI MENU";

            exitArtObj.SetActive(false);

            // 14. Status Text (Thông báo dưới cùng)
            GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            statusObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform sttRect2 = statusObj.GetComponent<RectTransform>();
            sttRect2.anchorMin = new Vector2(0.5f, 0.5f);
            sttRect2.anchorMax = new Vector2(0.5f, 0.5f);
            sttRect2.anchoredPosition = new Vector2(0f, -380f);
            sttRect2.sizeDelta = new Vector2(1000f, 40f);

            Text stText = statusObj.GetComponent<Text>();
            stText.font = defaultFont;
            stText.fontSize = 22;
            stText.fontStyle = FontStyle.Bold;
            stText.alignment = TextAnchor.MiddleCenter;
            stText.color = lightGoldColor;
            stText.text = "";
            stText.raycastTarget = false;

            // 15. ScreenFadeOverlay
            GameObject fadeObj = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            fadeObj.transform.SetParent(canvasObj.transform, false);
            fadeObj.transform.SetAsLastSibling();

            RectTransform fRect = fadeObj.GetComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.sizeDelta = Vector2.zero;

            Image fImg = fadeObj.GetComponent<Image>();
            fImg.color = Color.black;
            fImg.raycastTarget = true;

            CanvasGroup fadeGroup = fadeObj.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = true;

            // 15b. Puzzle Progression Slices (Map 1 -> Map 2 -> Map 3)
            GameObject slicesContainer = new GameObject("PuzzleSlicesContainer", typeof(RectTransform));
            slicesContainer.transform.SetParent(rootUIObj.transform, false);
            RectTransform scRect = slicesContainer.GetComponent<RectTransform>();
            scRect.anchorMin = new Vector2(0.5f, 0.5f);
            scRect.anchorMax = new Vector2(0.5f, 0.5f);
            scRect.anchoredPosition = new Vector2(-14.5f, 30f);
            scRect.sizeDelta = new Vector2(591f, 614f);

            CanvasGroup CreateSlice(string name, Vector2 pos, Vector2 size)
            {
                GameObject slice = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                slice.transform.SetParent(slicesContainer.transform, false);
                RectTransform sRect = slice.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.5f, 0.5f);
                sRect.anchorMax = new Vector2(0.5f, 0.5f);
                sRect.anchoredPosition = pos;
                sRect.sizeDelta = size;

                Image sImg = slice.GetComponent<Image>();
                sImg.color = Color.black;
                sImg.raycastTarget = false;

                CanvasGroup cg = slice.GetComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                return cg;
            }

            CanvasGroup slice1 = CreateSlice("SliceOverlay_1", new Vector2(-197f, 0f), new Vector2(198f, 614f));
            CanvasGroup slice2 = CreateSlice("SliceOverlay_2", new Vector2(0f, 0f), new Vector2(198f, 614f));
            CanvasGroup slice3 = CreateSlice("SliceOverlay_3", new Vector2(197f, 0f), new Vector2(198f, 614f));

            // SliceUnlockBanner
            GameObject bannerObj = new GameObject("SliceUnlockBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bannerObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform bnRect = bannerObj.GetComponent<RectTransform>();
            bnRect.anchorMin = new Vector2(0.5f, 0.5f);
            bnRect.anchorMax = new Vector2(0.5f, 0.5f);
            bnRect.anchoredPosition = new Vector2(-14.5f, 345f);
            bnRect.sizeDelta = new Vector2(680f, 50f);
            Image bnImg = bannerObj.GetComponent<Image>();
            bnImg.color = new Color(0.12f, 0.08f, 0.05f, 0.92f);
            bnImg.raycastTarget = false;

            GameObject bnTxtObj = new GameObject("SliceUnlockText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            bnTxtObj.transform.SetParent(bannerObj.transform, false);
            RectTransform bntRect = bnTxtObj.GetComponent<RectTransform>();
            bntRect.anchorMin = Vector2.zero;
            bntRect.anchorMax = Vector2.one;
            bntRect.sizeDelta = Vector2.zero;
            Text bnText = bnTxtObj.GetComponent<Text>();
            bnText.font = defaultFont;
            bnText.fontSize = 20;
            bnText.fontStyle = FontStyle.Bold;
            bnText.alignment = TextAnchor.MiddleCenter;
            bnText.color = goldTitleColor;
            bnText.text = "KÝ ỨC THỨC TỈNH: MẢNH 1...";
            bannerObj.SetActive(false);

            // NextStageButton
            Button nextStageBtn = CreateMenuButton(rootUIObj.transform, "NextStageButton", new Vector2(-14.5f, -315f), new Vector2(400f, 54f), "TIẾP TỤC: MÀN 2 (SẮC MÀU QUÊ HƯƠNG) →", accentBtnColor, 1050f);
            Text nextStageBtnTxt = nextStageBtn.GetComponentInChildren<Text>();
            nextStageBtn.gameObject.SetActive(false);

            // Memory Chest & Diary Animation Elements
            // Load 12 chest opening frames and 12 book opening frames
            List<Sprite> chestFrameList = new List<Sprite>();
            for (int i = 0; i < 12; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/PuzzelFrames/ChestAnimation/chest_open_{i:02d}.png");
                if (sp != null) chestFrameList.Add(sp);
            }
            List<Sprite> bookFrameList = new List<Sprite>();
            for (int i = 0; i < 12; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/PuzzelFrames/BookAnimation/book_open_{i:02d}.png");
                if (sp != null) bookFrameList.Add(sp);
            }

            // Load 12 page flip frames and Tet photo sprite
            List<Sprite> pageFlipList = new List<Sprite>();
            for (int i = 0; i < 12; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/PuzzelFrames/PageFlipAnimation/page_flip_{i:02d}.png");
                if (sp != null) pageFlipList.Add(sp);
            }
            Sprite tetPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Tet.png");
            Sprite hoiAnPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_HoiAn.png");
            Sprite seoulPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Seoul.png");
            Sprite palacePhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Gyeongbokgung.png");
            Sprite seollalPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Seollal.png");
            Sprite haLongPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_HaLong.png");
            Sprite busanPhotoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Busan.png");

            // 2. ChestAnimation (12 Frame mở nắp rương)
            GameObject chestAnimObj = new GameObject("ChestAnimation");
            chestAnimObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform chestAnimRt = chestAnimObj.AddComponent<RectTransform>();
            chestAnimRt.anchoredPosition = new Vector2(-683f, -130f);
            chestAnimRt.sizeDelta = new Vector2(310f, 230f);
            Image chestAnimImg = chestAnimObj.AddComponent<Image>();
            if (chestFrameList.Count > 0) chestAnimImg.sprite = chestFrameList[0];
            chestAnimImg.color = Color.white;
            chestAnimImg.raycastTarget = false;
            chestAnimObj.SetActive(false);

            // 3. MemoryChest Button (Vùng chạm mở rương)
            GameObject chestBtnObj = new GameObject("MemoryChest");
            chestBtnObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform chestBtnRt = chestBtnObj.AddComponent<RectTransform>();
            chestBtnRt.anchoredPosition = new Vector2(-683f, -130f);
            chestBtnRt.sizeDelta = new Vector2(310f, 230f);
            Image chestBtnImg = chestBtnObj.AddComponent<Image>();
            chestBtnImg.color = new Color(1f, 1f, 1f, 0.001f);
            chestBtnImg.raycastTarget = true;
            Button chestBtn = chestBtnObj.AddComponent<Button>();
            RegisterButtonEffect chestBtnEff = chestBtnObj.AddComponent<RegisterButtonEffect>();
            chestBtnEff.chimeFrequency = 1100f;

            // 4. ChestOpenOverlay Image (Dự phòng)
            GameObject chestOpenObj = new GameObject("ChestOpenOverlay");
            chestOpenObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform chestOpenRt = chestOpenObj.AddComponent<RectTransform>();
            chestOpenRt.anchoredPosition = new Vector2(-683f, -130f);
            chestOpenRt.sizeDelta = new Vector2(310f, 230f);
            Image chestOpenImg = chestOpenObj.AddComponent<Image>();
            if (chestFrameList.Count > 11) chestOpenImg.sprite = chestFrameList[11];
            chestOpenImg.color = Color.white;
            chestOpenImg.raycastTarget = false;
            chestOpenObj.SetActive(false);

            // 5. ChestPromptBadge (Huy hiệu chỉ dẫn chạm vào rương)
            GameObject chestPromptObj = new GameObject("ChestPromptBadge");
            chestPromptObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform chestPromptRt = chestPromptObj.AddComponent<RectTransform>();
            chestPromptRt.anchoredPosition = new Vector2(-683f, 20f);
            chestPromptRt.sizeDelta = new Vector2(180f, 42f);
            Image chestPromptImg = chestPromptObj.AddComponent<Image>();
            chestPromptImg.color = new Color(0.12f, 0.08f, 0.05f, 0.94f);
            GameObject chestPromptTxtObj = new GameObject("PromptText");
            chestPromptTxtObj.transform.SetParent(chestPromptObj.transform, false);
            RectTransform chestPromptTxtRt = chestPromptTxtObj.AddComponent<RectTransform>();
            chestPromptTxtRt.anchorMin = Vector2.zero;
            chestPromptTxtRt.anchorMax = Vector2.one;
            chestPromptTxtRt.sizeDelta = Vector2.zero;
            Text chestPromptTxt = chestPromptTxtObj.AddComponent<Text>();
            chestPromptTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            chestPromptTxt.fontSize = 18;
            chestPromptTxt.fontStyle = FontStyle.Bold;
            chestPromptTxt.alignment = TextAnchor.MiddleCenter;
            chestPromptTxt.color = new Color(1.0f, 0.90f, 0.40f, 1.0f);
            chestPromptTxt.text = "▼ CHẠM ĐỂ MỞ";
            chestPromptObj.SetActive(false);

            // 6. DiaryInChestButton (Vùng chạm vào cuốn nhật ký trong rương)
            GameObject diaryBtnObj = new GameObject("DiaryInChestButton");
            diaryBtnObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform diaryBtnRt = diaryBtnObj.AddComponent<RectTransform>();
            diaryBtnRt.anchoredPosition = new Vector2(-683f, -120f);
            diaryBtnRt.sizeDelta = new Vector2(160f, 130f);
            Image diaryBtnImg = diaryBtnObj.AddComponent<Image>();
            diaryBtnImg.color = new Color(1f, 1f, 1f, 0.001f);
            diaryBtnImg.raycastTarget = true;
            Button diaryBtn = diaryBtnObj.AddComponent<Button>();
            diaryBtnObj.SetActive(false);

            // 7. DiaryPromptBadge (Huy hiệu chỉ dẫn chạm vào nhật ký)
            GameObject diaryPromptObj = new GameObject("DiaryPromptBadge");
            diaryPromptObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform diaryPromptRt = diaryPromptObj.AddComponent<RectTransform>();
            diaryPromptRt.anchoredPosition = new Vector2(-683f, 20f);
            diaryPromptRt.sizeDelta = new Vector2(200f, 42f);
            Image diaryPromptImg = diaryPromptObj.AddComponent<Image>();
            diaryPromptImg.color = new Color(0.12f, 0.08f, 0.05f, 0.94f);
            GameObject diaryPromptTxtObj = new GameObject("DiaryPromptText");
            diaryPromptTxtObj.transform.SetParent(diaryPromptObj.transform, false);
            RectTransform diaryPromptTxtRt = diaryPromptTxtObj.AddComponent<RectTransform>();
            diaryPromptTxtRt.anchorMin = Vector2.zero;
            diaryPromptTxtRt.anchorMax = Vector2.one;
            diaryPromptTxtRt.sizeDelta = Vector2.zero;
            Text diaryPromptTxt = diaryPromptTxtObj.AddComponent<Text>();
            diaryPromptTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            diaryPromptTxt.fontSize = 18;
            diaryPromptTxt.fontStyle = FontStyle.Bold;
            diaryPromptTxt.alignment = TextAnchor.MiddleCenter;
            diaryPromptTxt.color = new Color(1.0f, 0.90f, 0.40f, 1.0f);
            diaryPromptTxt.text = "▼ MỞ NHẬT KÝ";
            diaryPromptObj.SetActive(false);

            // 9. FloatingDiary (Cuốn nhật ký bay lên và mở trang)
            GameObject floatingDiaryObj = new GameObject("FloatingDiary");
            floatingDiaryObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform floatingDiaryRt = floatingDiaryObj.AddComponent<RectTransform>();
            floatingDiaryRt.anchoredPosition = new Vector2(-683f, -120f);
            floatingDiaryRt.sizeDelta = new Vector2(230f, 280f);
            Image floatingDiaryImg = floatingDiaryObj.AddComponent<Image>();
            if (bookFrameList.Count > 0) floatingDiaryImg.sprite = bookFrameList[0];
            floatingDiaryImg.color = Color.white;
            floatingDiaryImg.raycastTarget = false;
            floatingDiaryObj.SetActive(false);

            // 9b. Diary Content (Văn bản nội dung nhật ký nét bút viết pixel trên trang bên phải)
            GameObject diaryContentObj = new GameObject("DiaryContentContainer", typeof(RectTransform), typeof(CanvasGroup));
            diaryContentObj.transform.SetParent(floatingDiaryObj.transform, false);
            RectTransform diaryContentRt = diaryContentObj.GetComponent<RectTransform>();
            diaryContentRt.anchorMin = Vector2.zero;
            diaryContentRt.anchorMax = Vector2.one;
            diaryContentRt.sizeDelta = Vector2.zero;
            diaryContentRt.anchoredPosition = Vector2.zero;

            Font diaryPixelFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.otf");
            if (diaryPixelFont == null) diaryPixelFont = defaultFont;

            Color inkColor = new Color(0.18f, 0.10f, 0.06f, 1f);

            // Trang bên trái: để trống giữ nguyên hoa văn mây và hoa sen theo yêu cầu
            GameObject leftPageObj = new GameObject("LeftPageText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            leftPageObj.transform.SetParent(diaryContentObj.transform, false);
            RectTransform leftPageRt = leftPageObj.GetComponent<RectTransform>();
            leftPageRt.anchorMin = new Vector2(0.5f, 0.5f);
            leftPageRt.anchorMax = new Vector2(0.5f, 0.5f);
            leftPageRt.anchoredPosition = new Vector2(-240f, 10f);
            leftPageRt.sizeDelta = new Vector2(380f, 440f);

            Text leftPageTxt = leftPageObj.GetComponent<Text>();
            leftPageTxt.font = diaryPixelFont;
            leftPageTxt.fontSize = 15;
            leftPageTxt.fontStyle = FontStyle.Italic;
            leftPageTxt.lineSpacing = 1.25f;
            leftPageTxt.alignment = TextAnchor.UpperLeft;
            leftPageTxt.color = inkColor;
            leftPageTxt.raycastTarget = false;
            leftPageTxt.text = string.Empty;
            leftPageObj.SetActive(false);

            // Trang bên phải: Toàn bộ nội dung nhật ký nằm trọn vẹn trong vùng giấy trang phải với nét bút viết pixel
            GameObject rightPageObj = new GameObject("RightPageText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            rightPageObj.transform.SetParent(diaryContentObj.transform, false);
            RectTransform rightPageRt = rightPageObj.GetComponent<RectTransform>();
            rightPageRt.anchorMin = new Vector2(0.5f, 0.5f);
            rightPageRt.anchorMax = new Vector2(0.5f, 0.5f);
            rightPageRt.anchoredPosition = new Vector2(192f, 15f);
            rightPageRt.sizeDelta = new Vector2(310f, 365f);

            Text rightPageTxt = rightPageObj.GetComponent<Text>();
            rightPageTxt.font = diaryPixelFont;
            rightPageTxt.fontSize = 14;
            rightPageTxt.fontStyle = FontStyle.Italic; // Nét nghiêng bút viết tay pixel
            rightPageTxt.lineSpacing = 1.3f;
            rightPageTxt.alignment = TextAnchor.UpperLeft;
            rightPageTxt.color = inkColor;
            rightPageTxt.supportRichText = true;
            rightPageTxt.raycastTarget = false;
            rightPageTxt.text = PuzzelMenuController.DIARY_RIGHT_PAGE_CONTENT;

            diaryContentObj.SetActive(false);

            // 5. EndingLetterModal
            GameObject endingModalObj = new GameObject("EndingLetterModal");
            endingModalObj.transform.SetParent(rootUIObj.transform, false);
            RectTransform endingModalRt = endingModalObj.AddComponent<RectTransform>();
            endingModalRt.anchorMin = Vector2.zero;
            endingModalRt.anchorMax = Vector2.one;
            endingModalRt.sizeDelta = Vector2.zero;
            Image endingModalImg = endingModalObj.AddComponent<Image>();
            endingModalImg.color = new Color(0f, 0f, 0f, 0.78f);
            CanvasGroup endingModalCg = endingModalObj.AddComponent<CanvasGroup>();
            endingModalObj.SetActive(false);

            GameObject cardObj = new GameObject("LetterCard");
            cardObj.transform.SetParent(endingModalObj.transform, false);
            RectTransform cardRt = cardObj.AddComponent<RectTransform>();
            cardRt.anchoredPosition = Vector2.zero;
            cardRt.sizeDelta = new Vector2(840f, 560f);
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = new Color(0.14f, 0.09f, 0.07f, 0.98f);

            // Card Title
            GameObject titleObj = new GameObject("ModalTitle");
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 225f);
            titleRt.sizeDelta = new Vector2(800f, 44f);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 22;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(0.98f, 0.88f, 0.55f, 1f);
            titleTxt.text = "★ CUỐN NHẬT KÝ KÝ ỨC ★";

            // Card Subtitle
            GameObject subObj = new GameObject("ModalSubtitle");
            subObj.transform.SetParent(cardObj.transform, false);
            RectTransform subRt = subObj.AddComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, 190f);
            subRt.sizeDelta = new Vector2(760f, 30f);
            Text subTxt = subObj.AddComponent<Text>();
            subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subTxt.fontSize = 15;
            subTxt.fontStyle = FontStyle.Italic;
            subTxt.alignment = TextAnchor.MiddleCenter;
            subTxt.color = new Color(0.85f, 0.78f, 0.68f, 1f);
            subTxt.text = "31/01/2014 — Mùng Một Tết ở Việt Nam";

            // Content box
            GameObject contentBoxObj = new GameObject("LetterContentBox");
            contentBoxObj.transform.SetParent(cardObj.transform, false);
            RectTransform contentBoxRt = contentBoxObj.AddComponent<RectTransform>();
            contentBoxRt.anchoredPosition = new Vector2(0f, 25f);
            contentBoxRt.sizeDelta = new Vector2(760f, 260f);
            Image contentBoxImg = contentBoxObj.AddComponent<Image>();
            contentBoxImg.color = new Color(0.09f, 0.06f, 0.04f, 0.90f);

            GameObject letterTxtObj = new GameObject("LetterText");
            letterTxtObj.transform.SetParent(contentBoxObj.transform, false);
            RectTransform letterTxtRt = letterTxtObj.AddComponent<RectTransform>();
            letterTxtRt.anchoredPosition = Vector2.zero;
            letterTxtRt.sizeDelta = new Vector2(710f, 230f);
            Text letterTxt = letterTxtObj.AddComponent<Text>();
            letterTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            letterTxt.fontSize = 16;
            letterTxt.alignment = TextAnchor.UpperLeft;
            letterTxt.color = new Color(0.95f, 0.92f, 0.86f, 1f);
            letterTxt.text = PuzzelMenuController.FULL_DIARY_CONTENT;

            // Finish Game Button (gán cha trực tiếp là rootUIObj để luôn hiển thị đúng bên dưới cuốn nhật ký; chứa 2 mũi tên điều hướng)
            Button finishBtn = CreateMenuButton(rootUIObj.transform, "FinishGameButton", new Vector2(0f, -325f), new Vector2(460f, 54f), "", new Color(0.42f, 0.22f, 0.12f, 1f), 1050f);
            finishBtn.gameObject.SetActive(false);

            // 16. Controller Manager
            GameObject managerObj = new GameObject("PuzzelMenuManager");
            PuzzelMenuController controller = managerObj.AddComponent<PuzzelMenuController>();

            // Nhạc nền BGM cho màn nhật ký
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03_Bridge.mp3");
            if (bgmClip == null) bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_MainMenu.mp3");
            AudioSource bgmSource = managerObj.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
            bgmSource.volume = 0.60f;
            controller.bgmAudioSource = bgmSource;
            controller.backgroundMusicClip = bgmClip;

            controller.isStandaloneMode = true;
            controller.fadeDuration = 0.35f;
            controller.headerPanel = null;
            controller.mainControlPanel = null;
            controller.rightInfoPanel = rightPanelObj;
            controller.settingsPanel = settingsPanelObj;
            controller.puzzleCollectionPanel = collectionPanelObj;
            controller.puzzleBoardPanel = puzzleBoardObj;
            controller.exitViewArtButton = exitArtObj;

            controller.portraitButton = portraitBtn;
            controller.slicesContainer = slicesContainer;
            controller.sliceOverlay1 = slice1;
            controller.sliceOverlay2 = slice2;
            controller.sliceOverlay3 = slice3;
            controller.sliceRevealDuration = 4.0f;
            controller.nextStageButton = nextStageBtn;
            controller.nextStageButtonText = nextStageBtnTxt;
            controller.sliceUnlockBanner = bannerObj;
            controller.sliceUnlockText = bnText;
            controller.chestOpeningFrames = chestFrameList.ToArray();
            controller.chestAnimationImage = chestAnimImg;
            controller.chestButton = chestBtn;
            controller.diaryInChestButton = diaryBtn;
            controller.diaryPromptBadge = diaryPromptObj;
            controller.chestOpenOverlay = chestOpenObj;
            controller.chestGlowEffect = null;
            controller.chestRaysEffect = null;
            controller.chestPromptBadge = chestPromptObj;
            controller.bookOpeningFrames = bookFrameList.ToArray();
            controller.pageFlipFrames = pageFlipList.ToArray();
            controller.diaryTetPhotoSprite = tetPhotoSp;
            controller.diaryHoiAnPhotoSprite = hoiAnPhotoSp;
            controller.diarySeoulPhotoSprite = seoulPhotoSp;
            controller.diaryGyeongbokgungPhotoSprite = palacePhotoSp;
            controller.diarySeollalPhotoSprite = seollalPhotoSp;
            controller.diaryHaLongPhotoSprite = haLongPhotoSp;
            controller.diaryBusanPhotoSprite = busanPhotoSp;
            controller.floatingDiaryImage = floatingDiaryImg;
            controller.diaryContentContainer = diaryContentObj;
            controller.diaryLeftPageText = leftPageTxt;
            controller.diaryRightPageText = rightPageTxt;
            controller.diaryGlowEffect = null;
            controller.diaryRaysEffect = null;
            controller.diarySparklesEffect = null;
            controller.endingLetterModal = endingModalObj;
            controller.finishGameButton = finishBtn;
            controller.chestPromptBadgeText = chestPromptTxt;
            controller.playPuzzleButton = null;
            controller.puzzleListButton = null;
            controller.viewArtButton = null;
            controller.settingsButton = null;
            controller.quitButton = null;

            controller.volumeSlider = musicSlider;
            controller.sfxSlider = sfxSlider;
            controller.closeSettingsButton = closeSetBtn;

            controller.closeCollectionButton = closeColBtn;
            controller.collectionItemButtons = colButtons;

            controller.puzzleTileButtons = tileButtons;
            controller.puzzleTileImages = tileImages;
            controller.tileSprites = tileSpriteList.ToArray();
            controller.shuffleButton = shuffleBtn;
            controller.autoSolveButton = solveBtn;
            controller.hintButton = hintBtn;
            controller.closePuzzleBoardButton = closePbBtn;
            controller.moveCountText = movesTxt;
            controller.timerText = timerTxt;
            controller.victoryBanner = victoryObj;

            controller.bgAnimator = animator;
            controller.statusText = stText;
            controller.screenFadeOverlay = fadeGroup;

            // 17. Lưu Scene độc lập tại: Assets/Scenes/Puzzle.unity và Assets/Scenes/Puzzel.unity
            string scenePathPuzzle = "Assets/Scenes/Puzzle.unity";
            string scenePathPuzzel = "Assets/Scenes/Puzzel.unity";
            EditorSceneManager.SaveScene(newScene, scenePathPuzzle);
            EditorSceneManager.SaveScene(newScene, scenePathPuzzel);
            Debug.Log($"[PuzzelMenuSetup] Đã lưu thành công scene tại: {scenePathPuzzle} và {scenePathPuzzel}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PuzzelMenuSetup] Hoàn tất toàn bộ quy trình thiết lập Scene 'Puzzel' tinh gọn độc lập!");
        }

        private static void EnsureSpritesImported()
        {
            for (int i = 0; i < 30; i++)
            {
                string path = $"Assets/UI/PuzzelFrames/frame_{i:D2}.png";
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    bool changed = false;
                    if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
                    if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
                    if (ti.maxTextureSize != 2048) { ti.maxTextureSize = 2048; changed = true; }
                    if (changed) ti.SaveAndReimport();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                string path = $"Assets/UI/PuzzelFrames/Tiles/tile_{i}.png";
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    bool changed = false;
                    if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
                    if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
                    if (ti.maxTextureSize != 512) { ti.maxTextureSize = 512; changed = true; }
                    if (changed) ti.SaveAndReimport();
                }
            }
        }
    }
}
