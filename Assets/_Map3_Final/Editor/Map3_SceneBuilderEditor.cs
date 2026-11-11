using System.Collections.Generic;

using System.IO;

using System.Linq;

using TMPro;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem.UI;

using UnityEngine.UI;



namespace Game.Map3.Editor

{

    [InitializeOnLoad]

    public static class Map3_SceneBuilderEditor

    {

        private const string SceneDirectory = "Assets/_Map3_Final/Scenes";

        private const string ScenePath = "Assets/_Map3_Final/Scenes/Map3_Final.unity";
        public static readonly string[] AllMap3ScenePaths = new string[]
        {
            "Assets/Scenes/Scenes/Map3_Final.unity",
            "Assets/_Map3_Final/Scenes/Map3_Final.unity"
        };

        private const string TriggerHudFlagFile = "Temp/UpdateL3HUD.flag";



        static Map3_SceneBuilderEditor()

        {

            EditorApplication.update -= CheckForTrigger;

            EditorApplication.update += CheckForTrigger;

        }



        private static void CheckForTrigger()

        {

            if (File.Exists(TriggerHudFlagFile))

            {

                if (EditorApplication.isPlaying || EditorApplication.isCompiling) return;

                try { File.Delete(TriggerHudFlagFile); } catch { }

                SetupTopRightHUDInScene();

            }

        }



        [MenuItem("Tools/Map 3/Setup Hint Frame & Dialog")]
        public static void SetupHintFrameInScene()
        {
            SetupTopRightHUDInScene();
        }

        [MenuItem("Tools/Map 3/Configure Quiz Illustration Sprites")]
        public static void ConfigureQuizIllustrationSprites()
        {
            string dir = "Assets/Sprites/Level3/QuizIllustrations";
            if (!Directory.Exists(dir)) return;
            string[] files = Directory.GetFiles(dir, "*.png");
            foreach (string f in files)
            {
                string assetPath = f.Replace("\\", "/");
                TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.mipmapEnabled = false;
                    ti.filterMode = FilterMode.Point;
                    ti.alphaIsTransparency = true;
                    ti.SaveAndReimport();
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("[Map3] Configured all QuizIllustration sprites!");
        }

        [MenuItem("Tools/Map 3/Update Quiz UI in Map3_Final")]
        public static void UpdateQuizUIInScene()
        {
            ConfigureQuizIllustrationSprites();

            var initialScene = EditorSceneManager.GetActiveScene();
            string returnPath = initialScene.path;

            foreach (string scenePath in AllMap3ScenePaths)
            {
                if (!File.Exists(scenePath)) continue;

                Debug.Log($"[Map3_SceneBuilder] Updating Quiz UI layout in scene: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var qm = Object.FindFirstObjectByType<Map3_QuizManager>();
                if (qm != null)
                {
                    qm.LoadIllustrationSpritesIfNull();
                    qm.EnforceQuizUILayout();
                    EditorUtility.SetDirty(qm);
                    if (qm.gameObject != null) EditorUtility.SetDirty(qm.gameObject);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(returnPath) && File.Exists(returnPath))
            {
                EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
            }
            Debug.Log("[Map3_SceneBuilder] Finished updating Quiz UI in Map 3 scenes.");
        }



        [MenuItem("Tools/Map 3/Setup Top-Right HUD & Modals")]

        public static void SetupTopRightHUDInScene()
        {
            var initialScene = EditorSceneManager.GetActiveScene();
            string returnPath = initialScene.path;

            foreach (string scenePath in AllMap3ScenePaths)
            {
                if (!File.Exists(scenePath)) continue;

                Debug.Log($"[Map3_SceneBuilder] Configuring Top-Right HUD & Modals in scene: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
                if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;

                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError($"[Map3_SceneBuilder] Could not find Canvas in {scenePath}!");
                    continue;
                }

                Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");
                if (quizCardSprite == null) quizCardSprite = uiSprite;

                Sprite toastFrameSprite = LoadSprite("Assets/UI/Toast_Frame.png");
                if (toastFrameSprite == null) toastFrameSprite = quizCardSprite;

                Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");
                if (btnWoodSprite == null) btnWoodSprite = uiSprite;

                Sprite catSprite = LoadSpriteSubAsset("Assets/Sprites/Meo_1.png");
                if (catSprite == null) catSprite = LoadSprite("Assets/Sprites/Meo_1.png");

                SetupTopRightHUDAndModals(canvas, defaultFont, uiSprite);
                SetupHintFrameAndDialog(canvas, defaultFont, uiSprite, quizCardSprite, btnWoodSprite, catSprite, toastFrameSprite);

                Transform fadeT = canvas.transform.Find("Fade_Overlay");
                if (fadeT != null) fadeT.SetAsLastSibling();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Map3_SceneBuilder] Top-Right HUD Buttons and Modals successfully configured and saved in {scenePath}!");
            }

            if (!string.IsNullOrEmpty(returnPath) && File.Exists(returnPath))
            {
                var cur = EditorSceneManager.GetActiveScene();
                if (cur.path != returnPath)
                {
                    EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
                }
            }
        }

        [MenuItem("Tools/Map 3/Build Playable Map 3 Scene (Safe Mode)")]

        public static void BuildMap3Scene()

        {

            Debug.Log("[Map3_SceneBuilder] Starting Map 3 Scene Generation (Safe Mode)...");



            if (!Directory.Exists(SceneDirectory))

            {

                Directory.CreateDirectory(SceneDirectory);

                AssetDatabase.Refresh();

            }



            // 1. Create a fresh empty scene

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);



            // 2. Safe read-only asset resolution

            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");

            Sprite capsuleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Capsule.png");

            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");



            TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");

            if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;



            Sprite map3_0 = LoadSprite("Assets/Sprites/Level3/Map3_0.png");

            Sprite map3_1 = LoadSprite("Assets/Sprites/Level3/Map3_1.png");

            Sprite map3_layer0 = LoadSprite("Assets/Sprites/Level3/Map3_Layer0.png");

            Sprite map3_layer1 = LoadSprite("Assets/Sprites/Level3/Map3_Layer1.png");

            Sprite denViet = LoadSprite("Assets/Sprites/Level3/Den_Viet.png");

            Sprite denHan = LoadSprite("Assets/Sprites/Level3/Den_Han.png");



            Sprite playerSprite = LoadSprite("Assets/Sprites/HaEun_1.png");

            if (playerSprite == null) playerSprite = capsuleSprite != null ? capsuleSprite : squareSprite;



            Sprite catSprite = LoadSprite("Assets/Sprites/Meo_1.png");

            if (catSprite == null) catSprite = squareSprite;



            RuntimeAnimatorController playerAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Visual_Sprite.controller");

            RuntimeAnimatorController catAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/NPC_Cat.controller");



            Sprite quizPanelSprite = LoadSprite("Assets/UI/Quiz.png");

            Sprite toastFrameSprite = LoadSprite("Assets/UI/Toast_Frame.png");

            if (toastFrameSprite == null) toastFrameSprite = quizPanelSprite;

            Sprite buttonSprite = LoadSprite("Assets/UI/Button.png");

            Sprite inventorySprite = LoadSprite("Assets/Sprites/Inventory.png");



            AudioClip bgmMap3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03_Bridge.mp3");



            // 3. Hierarchy Root Groups

            GameObject rootManagers = new GameObject("--- MANAGERS ---");

            GameObject rootCameras = new GameObject("--- CAMERAS ---");

            GameObject rootEnvironment = new GameObject("--- ENVIRONMENT ---");

            GameObject rootGameplay = new GameObject("--- GAMEPLAY OBJECTS ---");

            GameObject rootCanvas = new GameObject("--- UI CANVAS ---");



            // -------------------------------------------------------------

            // GROUP 1: MANAGERS

            // -------------------------------------------------------------

            var im = rootManagers.AddComponent<Map3_InventoryManager>();

            var qm = rootManagers.AddComponent<Map3_QuizManager>();

            var um = rootManagers.AddComponent<Map3_UIManager>();



            // EventSystem

            GameObject eventSystemGo = new GameObject("EventSystem");

            eventSystemGo.transform.SetParent(rootManagers.transform, false);

            eventSystemGo.AddComponent<EventSystem>();

            eventSystemGo.AddComponent<InputSystemUIInputModule>();



            // Audio BGM

            GameObject audioGo = new GameObject("BGM_Source");

            audioGo.transform.SetParent(rootManagers.transform, false);

            var audioSource = audioGo.AddComponent<AudioSource>();

            audioSource.clip = bgmMap3;

            audioSource.loop = true;

            audioSource.playOnAwake = true;

            audioSource.volume = 0.55f;

            if (bgmMap3 != null) audioSource.Play();



            // -------------------------------------------------------------

            // GROUP 3: ENVIRONMENT (3 AREAS)

            // -------------------------------------------------------------

            // Area 1: Khu Vườn Ngoài Trời (Center at 0, 0)

            GameObject gardenAreaGo = new GameObject("Area1_OutdoorGarden");

            gardenAreaGo.transform.SetParent(rootEnvironment.transform, false);



            // Broken background (Map3_0)

            GameObject mapBrokenGo = new GameObject("Map3_Broken");

            mapBrokenGo.transform.SetParent(gardenAreaGo.transform, false);

            mapBrokenGo.transform.position = Vector3.zero;

            var srBroken = mapBrokenGo.AddComponent<SpriteRenderer>();

            srBroken.sprite = map3_0 != null ? map3_0 : squareSprite;

            srBroken.sortingOrder = 0;

            srBroken.color = Color.white;



            // Fixed background (Map3_1)

            GameObject mapFixedGo = new GameObject("Map3_Fixed");

            mapFixedGo.transform.SetParent(gardenAreaGo.transform, false);

            mapFixedGo.transform.position = Vector3.zero;

            var srFixed = mapFixedGo.AddComponent<SpriteRenderer>();

            srFixed.sprite = map3_1 != null ? map3_1 : squareSprite;

            srFixed.sortingOrder = 1;

            srFixed.color = new Color(1f, 1f, 1f, 0f); // Alpha = 0 initially



            // Foreground layers (Tán cây / Mái nhà)

            GameObject brokenLayerGo = new GameObject("Map3_Broken layer");

            brokenLayerGo.transform.SetParent(mapBrokenGo.transform, false);

            var srBrokenLayer = brokenLayerGo.AddComponent<SpriteRenderer>();

            srBrokenLayer.sprite = map3_layer0 != null ? map3_layer0 : squareSprite;

            srBrokenLayer.sortingLayerName = "Foreground";

            srBrokenLayer.sortingOrder = 10;

            srBrokenLayer.color = Color.white;



            GameObject fixedLayerGo = new GameObject("Map3_Fixed layer");

            fixedLayerGo.transform.SetParent(mapFixedGo.transform, false);

            var srFixedLayer = fixedLayerGo.AddComponent<SpriteRenderer>();

            srFixedLayer.sprite = map3_layer1 != null ? map3_layer1 : squareSprite;

            srFixedLayer.sortingLayerName = "Foreground";

            srFixedLayer.sortingOrder = 20;

            srFixedLayer.color = new Color(1f, 1f, 1f, 0f);



            // Camera Bounds for Outdoor Garden (27.52 x 15.36)

            GameObject gardenBoundsGo = new GameObject("Garden_Camera_Bounds");

            gardenBoundsGo.transform.SetParent(gardenAreaGo.transform, false);

            gardenBoundsGo.transform.position = Vector3.zero;

            var gardenCamCol = gardenBoundsGo.AddComponent<BoxCollider2D>();

            gardenCamCol.isTrigger = true;

            gardenCamCol.size = new Vector2(27.5f, 15.3f);



            // Water Block Collider (Blocks stream at bridge X ≈ 1.8 when broken)

            GameObject waterBlockGo = new GameObject("WaterBlockCollider");

            waterBlockGo.transform.SetParent(gardenAreaGo.transform, false);

            waterBlockGo.transform.position = new Vector2(1.8f, 0f);

            var waterCol = waterBlockGo.AddComponent<BoxCollider2D>();

            waterCol.isTrigger = false;

            waterCol.size = new Vector2(1.8f, 15.3f);



            // Outer garden boundary walls

            GameObject gardenWallsGo = new GameObject("Garden_Walls");

            gardenWallsGo.transform.SetParent(gardenAreaGo.transform, false);

            CreateWall(gardenWallsGo.transform, "Wall_Top", new Vector3(0f, 7.8f, 0f), new Vector2(28f, 0.8f));

            CreateWall(gardenWallsGo.transform, "Wall_Bottom", new Vector3(0f, -7.8f, 0f), new Vector2(28f, 0.8f));

            CreateWall(gardenWallsGo.transform, "Wall_Left", new Vector3(-13.8f, 0f, 0f), new Vector2(0.8f, 16f));

            CreateWall(gardenWallsGo.transform, "Wall_Right", new Vector3(13.8f, 0f, 0f), new Vector2(0.8f, 16f));



            // Area 2: Không gian bên trong Đền Việt - Cutaway Room (Center at X = 0, Y = 100)

            GameObject vietShrineAreaGo = new GameObject("Area2_VietnamShrine");

            vietShrineAreaGo.transform.SetParent(rootEnvironment.transform, false);

            vietShrineAreaGo.transform.position = new Vector3(0f, 100f, 0f);



            // Black outer box

            GameObject vietBlackBg = new GameObject("Black_Surround");

            vietBlackBg.transform.SetParent(vietShrineAreaGo.transform, false);

            vietBlackBg.transform.localScale = new Vector3(32f, 22f, 1f);

            var srVietBlack = vietBlackBg.AddComponent<SpriteRenderer>();

            srVietBlack.sprite = squareSprite;

            srVietBlack.color = Color.black;

            srVietBlack.sortingOrder = -15;



            // Room Background

            GameObject vietRoomBg = new GameObject("Room_Background");

            vietRoomBg.transform.SetParent(vietShrineAreaGo.transform, false);

            var srVietRoom = vietRoomBg.AddComponent<SpriteRenderer>();

            srVietRoom.sprite = denViet != null ? denViet : squareSprite;

            srVietRoom.sortingOrder = -5;



            // Vietnam Shrine Camera Bounds

            GameObject vietBoundsGo = new GameObject("VietShrine_Camera_Bounds");

            vietBoundsGo.transform.SetParent(vietShrineAreaGo.transform, false);

            var vietCamCol = vietBoundsGo.AddComponent<BoxCollider2D>();

            vietCamCol.isTrigger = true;

            vietCamCol.size = new Vector2(13.75f, 7.68f);



            // Vietnam Shrine Walls

            GameObject vietWallsGo = new GameObject("Room_Walls");

            vietWallsGo.transform.SetParent(vietShrineAreaGo.transform, false);

            CreateWall(vietWallsGo.transform, "Wall_Top", new Vector3(0f, 3.85f, 0f), new Vector2(14f, 0.5f));

            CreateWall(vietWallsGo.transform, "Wall_Left", new Vector3(-6.85f, 0f, 0f), new Vector2(0.5f, 8f));

            CreateWall(vietWallsGo.transform, "Wall_Right", new Vector3(6.85f, 0f, 0f), new Vector2(0.5f, 8f));

            CreateWall(vietWallsGo.transform, "Wall_Bottom_Left", new Vector3(-4.5f, -3.85f, 0f), new Vector2(5.5f, 0.5f));

            CreateWall(vietWallsGo.transform, "Wall_Bottom_Right", new Vector3(4.5f, -3.85f, 0f), new Vector2(5.5f, 0.5f));



            // Area 3: Không gian bên trong Đền Hàn - Cutaway Room (Center at X = 50, Y = 100)

            GameObject koreaShrineAreaGo = new GameObject("Area3_KoreaShrine");

            koreaShrineAreaGo.transform.SetParent(rootEnvironment.transform, false);

            koreaShrineAreaGo.transform.position = new Vector3(50f, 100f, 0f);



            // Black outer box

            GameObject koreaBlackBg = new GameObject("Black_Surround");

            koreaBlackBg.transform.SetParent(koreaShrineAreaGo.transform, false);

            koreaBlackBg.transform.localScale = new Vector3(32f, 22f, 1f);

            var srKoreaBlack = koreaBlackBg.AddComponent<SpriteRenderer>();

            srKoreaBlack.sprite = squareSprite;

            srKoreaBlack.color = Color.black;

            srKoreaBlack.sortingOrder = -15;



            // Room Background

            GameObject koreaRoomBg = new GameObject("Room_Background");

            koreaRoomBg.transform.SetParent(koreaShrineAreaGo.transform, false);

            var srKoreaRoom = koreaRoomBg.AddComponent<SpriteRenderer>();

            srKoreaRoom.sprite = denHan != null ? denHan : squareSprite;

            srKoreaRoom.sortingOrder = -5;



            // Korea Shrine Camera Bounds

            GameObject koreaBoundsGo = new GameObject("KoreaShrine_Camera_Bounds");

            koreaBoundsGo.transform.SetParent(koreaShrineAreaGo.transform, false);

            var koreaCamCol = koreaBoundsGo.AddComponent<BoxCollider2D>();

            koreaCamCol.isTrigger = true;

            koreaCamCol.size = new Vector2(13.75f, 7.68f);



            // Korea Shrine Walls

            GameObject koreaWallsGo = new GameObject("Room_Walls");

            koreaWallsGo.transform.SetParent(koreaShrineAreaGo.transform, false);

            CreateWall(koreaWallsGo.transform, "Wall_Top", new Vector3(0f, 3.85f, 0f), new Vector2(14f, 0.5f));

            CreateWall(koreaWallsGo.transform, "Wall_Left", new Vector3(-6.85f, 0f, 0f), new Vector2(0.5f, 8f));

            CreateWall(koreaWallsGo.transform, "Wall_Right", new Vector3(6.85f, 0f, 0f), new Vector2(0.5f, 8f));

            CreateWall(koreaWallsGo.transform, "Wall_Bottom_Left", new Vector3(-4.5f, -3.85f, 0f), new Vector2(5.5f, 0.5f));

            CreateWall(koreaWallsGo.transform, "Wall_Bottom_Right", new Vector3(4.5f, -3.85f, 0f), new Vector2(5.5f, 0.5f));



            // -------------------------------------------------------------

            // GROUP 4: GAMEPLAY OBJECTS & PLAYER

            // -------------------------------------------------------------

            // 1. Player (Top-down character)

            GameObject playerGo = new GameObject("Player");

            playerGo.transform.SetParent(rootGameplay.transform, false);

            playerGo.tag = "Player";

            playerGo.transform.position = new Vector3(-4.5f, -2.5f, 0f);



            var playerRb = playerGo.AddComponent<Rigidbody2D>();

            playerRb.gravityScale = 0f;

            playerRb.freezeRotation = true;

            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;



            var playerCol = playerGo.AddComponent<BoxCollider2D>();

            playerCol.isTrigger = false;

            playerCol.size = new Vector2(0.8f, 1.25f);

            playerCol.offset = new Vector2(0f, -0.2f);



            GameObject playerVisualGo = new GameObject("Visual_Sprite");

            playerVisualGo.transform.SetParent(playerGo.transform, false);

            var playerSr = playerVisualGo.AddComponent<SpriteRenderer>();

            playerSr.sprite = playerSprite;

            playerSr.sortingOrder = 5;



            if (playerAnimCtrl != null)

            {

                var pAnim = playerVisualGo.AddComponent<Animator>();

                pAnim.runtimeAnimatorController = playerAnimCtrl;

            }



            var playerCtrl = playerGo.AddComponent<Map3_PlayerController>();



            // -------------------------------------------------------------

            // 10 GAMEPLAY INTERACTABLE POINTS (PRECISE ART-ALIGNED COORDINATES)

            // -------------------------------------------------------------

            // Điểm 0: CatHint_Zone (Bờ Việt: -4.2, -1.0, 0)

            GameObject catGo = CreateInteractableRoot(rootGameplay.transform, "CatHint_Zone", new Vector3(-4.2f, -1.0f, 0f),

                "Chú Mèo", "[E] Hỏi Mèo Gợi Ý", defaultFont, new Color(1f, 0.7f, 0.2f, 1f));

            catGo.AddComponent<Map3_CatHint>();

            GameObject catVisual = new GameObject("Cat_Visual");

            catVisual.transform.SetParent(catGo.transform, false);

            var catSr = catVisual.AddComponent<SpriteRenderer>();

            catSr.sprite = catSprite;

            catSr.sortingOrder = 4;

            if (catAnimCtrl != null)

            {

                var cAnim = catVisual.AddComponent<Animator>();

                cAnim.runtimeAnimatorController = catAnimCtrl;

            }



            // Điểm 1: KeyJar_Zone (Bờ Việt, chum sành cạnh Đền Việt: -4.5, 2.0, 0)

            GameObject keyJarGo = CreateInteractableRoot(rootGameplay.transform, "KeyJar_Zone", new Vector3(-4.5f, 2.0f, 0f),

                "Lu Sành Cổ", "[E] Tìm Kiếm Trong Lu", defaultFont, new Color(0.85f, 0.65f, 0.35f, 1f));

            keyJarGo.AddComponent<Map3_KeyJar>();



            // Điểm 2: Cửa Đền Việt (Entrance ngoài hiên: -6.8, 2.2, 0)

            GameObject vietEntGo = CreateInteractableRoot(rootGameplay.transform, "VietShrine_Entrance", new Vector3(-6.8f, 2.2f, 0f),

                "Cửa Đền Việt", "[E] Bước Vào Đền Việt", defaultFont, new Color(0.3f, 0.85f, 0.45f, 1f), colliderSize: new Vector2(2f, 1.8f));

            var vietEntDoor = vietEntGo.AddComponent<Map3_ShrineDoor>();



            GameObject vietInsideSpawn = new GameObject("VietShrine_SpawnPoint_Inside");

            vietInsideSpawn.transform.SetParent(vietShrineAreaGo.transform, false);

            vietInsideSpawn.transform.position = new Vector3(0f, 98.5f, 0f); // Placed safely inside (+2.2f above exit trigger)



            GameObject vietOutsideSpawn = new GameObject("VietShrine_SpawnPoint_Outside");

            vietOutsideSpawn.transform.SetParent(gardenAreaGo.transform, false);

            vietOutsideSpawn.transform.position = new Vector3(-6.8f, 1.2f, 0f);



            SetSerializedObjectReference(vietEntDoor, "targetSpawnPoint", vietInsideSpawn.transform);

            SetSerializedVector3(vietEntDoor, "fallbackTargetPosition", new Vector3(0f, 98.5f, 0f));

            SetSerializedObjectReference(vietEntDoor, "targetCameraBounds", vietCamCol);



            // VietShrine_Exit (Bậc cửa dưới phòng Đền Việt: 0, 96.3, 0)

            GameObject vietExitGo = CreateInteractableRoot(vietShrineAreaGo.transform, "VietShrine_Exit", new Vector3(0f, 96.3f, 0f),

                "Bậc Cửa Thoát", "[E] Ra Vườn Việt", defaultFont, new Color(0.9f, 0.4f, 0.4f, 1f), colliderSize: new Vector2(3f, 1.2f));

            var vietExitDoor = vietExitGo.AddComponent<Map3_ShrineDoor>();

            SetSerializedObjectReference(vietExitDoor, "targetSpawnPoint", vietOutsideSpawn.transform);

            SetSerializedVector3(vietExitDoor, "fallbackTargetPosition", new Vector3(-6.8f, 1.2f, 0f));

            SetSerializedObjectReference(vietExitDoor, "targetCameraBounds", gardenCamCol);

            SetSerializedBool(vietExitDoor, "isAutoTriggerOnTouch", true);



            // Điểm 3: VietChest_Zone (Rương gỗ cổ vẽ sẵn tại: 0, 99.8, 0)

            GameObject chestGo = CreateInteractableRoot(vietShrineAreaGo.transform, "VietChest_Zone", new Vector3(0f, 99.8f, 0f),

                "Rương Gỗ Cổ", "[E] Mở Rương Báu", defaultFont, new Color(1f, 0.85f, 0.2f, 1f), colliderSize: new Vector2(2.2f, 1.8f));

            chestGo.AddComponent<Map3_Chest>();



            // Điểm Lore 3 (Đền Việt): Interact_DanTranh (Cây đàn tranh bên trái: -4.0, 99.0, 0)

            GameObject danTranhGo = CreateInteractableRoot(vietShrineAreaGo.transform, "Interact_DanTranh", new Vector3(-4.0f, 99.0f, 0f),

                "Đàn Tranh", "[E] Khám Phá Đàn Tranh", defaultFont, new Color(0.9f, 0.75f, 0.2f, 1f), colliderSize: new Vector2(1.8f, 1.8f));

            var danTranhScript = danTranhGo.AddComponent<Map3_LoreInteractable>();

            danTranhScript.Setup("Quiz_Lore_DanTranh", "Đàn Tranh", "[E] Khám Phá Đàn Tranh");



            // Điểm Lore 4 (Đền Việt): Interact_DinhDong (Lư hương / Đỉnh đồng trên bàn thờ: 2.5, 100.2, 0)

            GameObject dinhDongGo = CreateInteractableRoot(vietShrineAreaGo.transform, "Interact_DinhDong", new Vector3(2.5f, 100.2f, 0f),

                "Bộ Đỉnh Đồng", "[E] Chiêm Ngưỡng Đỉnh Đồng", defaultFont, new Color(0.85f, 0.65f, 0.25f, 1f), colliderSize: new Vector2(1.8f, 1.8f));

            var dinhDongScript = dinhDongGo.AddComponent<Map3_LoreInteractable>();

            dinhDongScript.Setup("Quiz_Lore_DinhDong", "Bộ Đỉnh Đồng", "[E] Chiêm Ngưỡng Đỉnh Đồng");



            // Điểm 4: 3 Mảnh Gỗ Bờ Việt

            // Wood_Spot_1: Rừng trúc (-11.5, 3.2, 0)

            GameObject wood1Go = CreateInteractableRoot(rootGameplay.transform, "Wood_Spot_1", new Vector3(-11.5f, 3.2f, 0f),

                "Mảnh Gỗ 1", "[E] Nhặt Mảnh Gỗ 1", defaultFont, new Color(0.7f, 0.5f, 0.25f, 1f));

            var wood1Script = wood1Go.AddComponent<Map3_WoodPlank>();

            wood1Script.SetSpotIndex(1);

            CreatePickupVisual(wood1Go.transform, squareSprite, new Vector2(0.5f, 0.25f), new Color(0.7f, 0.48f, 0.26f, 1f), sortingOrder: 3);



            // Wood_Spot_2: Bến gỗ hồ sen (-3.0, -2.5, 0)

            GameObject wood2Go = CreateInteractableRoot(rootGameplay.transform, "Wood_Spot_2", new Vector3(-3.0f, -2.5f, 0f),

                "Mảnh Gỗ 2", "[E] Nhặt Mảnh Gỗ 2", defaultFont, new Color(0.7f, 0.5f, 0.25f, 1f));

            var wood2Script = wood2Go.AddComponent<Map3_WoodPlank>();

            wood2Script.SetSpotIndex(2);

            CreatePickupVisual(wood2Go.transform, squareSprite, new Vector2(0.5f, 0.25f), new Color(0.7f, 0.48f, 0.26f, 1f), sortingOrder: 3);



            // Wood_Spot_3: Cạnh hàng rào bờ suối (-5.5, -6.5, 0)

            GameObject wood3Go = CreateInteractableRoot(rootGameplay.transform, "Wood_Spot_3", new Vector3(-5.5f, -6.5f, 0f),

                "Mảnh Gỗ 3", "[E] Nhặt Mảnh Gỗ 3", defaultFont, new Color(0.7f, 0.5f, 0.25f, 1f));

            var wood3Script = wood3Go.AddComponent<Map3_WoodPlank>();

            wood3Script.SetSpotIndex(3);

            CreatePickupVisual(wood3Go.transform, squareSprite, new Vector2(0.5f, 0.25f), new Color(0.7f, 0.48f, 0.26f, 1f), sortingOrder: 3);



            // Điểm 5: Cây Cầu Gãy (Bridge_InteractZone tại đầu cầu bờ Việt: 0.5, 0.0, 0)

            GameObject bridgeGo = CreateInteractableRoot(rootGameplay.transform, "Bridge_InteractZone", new Vector3(0.5f, 0.0f, 0f),

                "Cây Cầu Gãy", "[E] Sửa Chữa Cây Cầu", defaultFont, new Color(0.95f, 0.65f, 0.2f, 1f), colliderSize: new Vector2(2.2f, 2.5f));

            var bridgeScript = bridgeGo.AddComponent<Map3_BridgeController>();



            // Wood Dust particles

            GameObject dustGo = new GameObject("WoodDust_Particles");

            dustGo.transform.SetParent(bridgeGo.transform, false);

            var dustPs = CreateParticleSystem(dustGo, new Color(0.75f, 0.55f, 0.3f, 0.8f), 20f, 0.8f);



            // Cherry Blossom particles on Korea bank

            GameObject cherryGo = new GameObject("CherryBlossom_Particles");

            cherryGo.transform.SetParent(gardenAreaGo.transform, false);

            cherryGo.transform.position = new Vector3(8.5f, 2f, 0f);

            var cherryPs = CreateParticleSystem(cherryGo, new Color(1f, 0.72f, 0.84f, 0.85f), 25f, 3.5f, boxSize: new Vector3(12f, 10f, 1f));



            SetSerializedObjectReference(bridgeScript, "brokenBridgeRenderer", srBroken);

            SetSerializedObjectReference(bridgeScript, "fixedBridgeRenderer", srFixed);

            SetSerializedObjectReference(bridgeScript, "brokenLayerRenderer", srBrokenLayer);

            SetSerializedObjectReference(bridgeScript, "fixedLayerRenderer", srFixedLayer);

            SetSerializedObjectReference(bridgeScript, "waterBlockCollider", waterCol);

            SetSerializedObjectReference(bridgeScript, "woodDustParticles", dustPs);

            SetSerializedObjectReference(bridgeScript, "cherryBlossomParticles", cherryPs);



            // Điểm 6: RockPile_Zone (Bờ Hàn dưới gốc anh đào: 5.5, 2.2, 0)

            GameObject rockGo = CreateInteractableRoot(rootGameplay.transform, "RockPile_Zone", new Vector3(5.5f, 2.2f, 0f),

                "Bãi Đá Cuội", "[E] Nhặt Đá Cầu Nguyện", defaultFont, new Color(0.55f, 0.75f, 0.95f, 1f), colliderSize: new Vector2(2f, 2f));

            rockGo.AddComponent<Map3_RockPile>();



            // Điểm 7: Cửa Đền Hàn (Entrance ngoài hiên: 9.5, 2.0, 0)

            GameObject koreaEntGo = CreateInteractableRoot(rootGameplay.transform, "KoreaShrine_Entrance", new Vector3(9.5f, 2.0f, 0f),

                "Cửa Đền Hàn", "[E] Bước Vào Đền Hàn", defaultFont, new Color(0.3f, 0.85f, 0.85f, 1f), colliderSize: new Vector2(2f, 1.8f));

            var koreaEntDoor = koreaEntGo.AddComponent<Map3_ShrineDoor>();



            GameObject koreaInsideSpawn = new GameObject("KoreaShrine_SpawnPoint_Inside");

            koreaInsideSpawn.transform.SetParent(koreaShrineAreaGo.transform, false);

            koreaInsideSpawn.transform.position = new Vector3(50f, 98.5f, 0f); // Placed safely inside (+2.2f above exit trigger)



            GameObject koreaOutsideSpawn = new GameObject("KoreaShrine_SpawnPoint_Outside");

            koreaOutsideSpawn.transform.SetParent(gardenAreaGo.transform, false);

            koreaOutsideSpawn.transform.position = new Vector3(9.5f, 1.0f, 0f);



            SetSerializedObjectReference(koreaEntDoor, "targetSpawnPoint", koreaInsideSpawn.transform);

            SetSerializedVector3(koreaEntDoor, "fallbackTargetPosition", new Vector3(50f, 98.5f, 0f));

            SetSerializedObjectReference(koreaEntDoor, "targetCameraBounds", koreaCamCol);



            // KoreaShrine_Exit (Bậc cửa dưới phòng Đền Hàn: 50, 96.3, 0)

            GameObject koreaExitGo = CreateInteractableRoot(koreaShrineAreaGo.transform, "KoreaShrine_Exit", new Vector3(50f, 96.3f, 0f),

                "Bậc Cửa Thoát", "[E] Ra Vườn Hàn", defaultFont, new Color(0.9f, 0.4f, 0.4f, 1f), colliderSize: new Vector2(3f, 1.2f));

            var koreaExitDoor = koreaExitGo.AddComponent<Map3_ShrineDoor>();

            SetSerializedObjectReference(koreaExitDoor, "targetSpawnPoint", koreaOutsideSpawn.transform);

            SetSerializedVector3(koreaExitDoor, "fallbackTargetPosition", new Vector3(9.5f, 1.0f, 0f));

            SetSerializedObjectReference(koreaExitDoor, "targetCameraBounds", gardenCamCol);

            SetSerializedBool(koreaExitDoor, "isAutoTriggerOnTouch", true);



            // Điểm 8: Doltap_Zone (Bàn tháp đá Đền Hàn: 50, 100.2, 0)

            GameObject doltapGo = CreateInteractableRoot(koreaShrineAreaGo.transform, "Doltap_Zone", new Vector3(50f, 100.2f, 0f),

                "Tháp Đá Doltap", "[E] Đặt Viên Đá Cầu Nguyện", defaultFont, new Color(0.7f, 0.8f, 0.95f, 1f), colliderSize: new Vector2(2f, 2.2f));

            var doltapScript = doltapGo.AddComponent<Map3_Doltap>();



            // Small stacked stones visual

            GameObject stoneVisualGo = new GameObject("Doltap_Stones");

            stoneVisualGo.transform.SetParent(doltapGo.transform, false);

            stoneVisualGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            stoneVisualGo.transform.localScale = new Vector3(0.6f, 0.8f, 1f);

            var stoneSr = stoneVisualGo.AddComponent<SpriteRenderer>();

            stoneSr.sprite = capsuleSprite != null ? capsuleSprite : squareSprite;

            stoneSr.color = new Color(0.65f, 0.7f, 0.75f, 1f);

            stoneSr.sortingOrder = 2;

            SetSerializedObjectReference(doltapScript, "doltapRenderer", stoneSr);



            // Top stone visual child (activated when stone is placed)

            GameObject topStoneGo = new GameObject("TopStone_Visual");

            topStoneGo.transform.SetParent(doltapGo.transform, false);

            topStoneGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            topStoneGo.transform.localScale = new Vector3(0.35f, 0.3f, 1f);

            var srTopStone = topStoneGo.AddComponent<SpriteRenderer>();

            srTopStone.sprite = capsuleSprite != null ? capsuleSprite : squareSprite;

            srTopStone.color = new Color(0.98f, 0.88f, 0.45f, 1f);

            srTopStone.sortingOrder = 3;

            topStoneGo.SetActive(false);

            SetSerializedObjectReference(doltapScript, "topStoneVisual", topStoneGo);



            // Điểm Lore 1 (Đền Hàn): Interact_Ilworobongdo (Bức bình phong sau bàn trà: 47.2, 100.5, 0)

            GameObject binhPhongGo = CreateInteractableRoot(koreaShrineAreaGo.transform, "Interact_Ilworobongdo", new Vector3(47.2f, 100.5f, 0f),

                "Bình Phong Ilworobongdo", "[E] Chiêm Ngưỡng Bình Phong", defaultFont, new Color(0.35f, 0.75f, 0.95f, 1f), colliderSize: new Vector2(1.8f, 1.8f));

            var binhPhongScript = binhPhongGo.AddComponent<Map3_LoreInteractable>();

            binhPhongScript.Setup("Quiz_Lore_BinhPhong", "Bình Phong Ilworobongdo", "[E] Chiêm Ngưỡng Bình Phong");



            // Điểm Lore 2 (Đền Hàn): Interact_Celadon (Bình gốm ngọc bích: 53.5, 99.2, 0)

            GameObject celadonGo = CreateInteractableRoot(koreaShrineAreaGo.transform, "Interact_Celadon", new Vector3(53.5f, 99.2f, 0f),

                "Gốm Sứ Celadon", "[E] Tìm Hiểu Gốm Celadon", defaultFont, new Color(0.25f, 0.9f, 0.7f, 1f), colliderSize: new Vector2(1.8f, 1.8f));

            var celadonScript = celadonGo.AddComponent<Map3_LoreInteractable>();

            celadonScript.Setup("Quiz_Lore_GomSu", "Gốm Sứ Celadon", "[E] Tìm Hiểu Gốm Celadon");



            // Điểm 9: FinalAltar_Zone (Bệ đá tế ngọc vẽ sẵn tại góc dưới bờ Hàn: 9.5, -3.0, 0)

            GameObject altarGo = CreateInteractableRoot(rootGameplay.transform, "FinalAltar_Zone", new Vector3(9.5f, -3.0f, 0f),

                "Đàn Tế Ngọc Bội", "[E] Hợp Nhất Ngọc Bội", defaultFont, new Color(0.1f, 1f, 0.6f, 1f), colliderSize: new Vector2(2.5f, 2.5f));

            var altarScript = altarGo.AddComponent<Map3_FinalAltar>();



            // Jade aura glowing particle effect

            GameObject auraGo = new GameObject("JadeAura_Effect");

            auraGo.transform.SetParent(altarGo.transform, false);

            auraGo.transform.localPosition = new Vector3(0f, 0.3f, 0f);

            CreateParticleSystem(auraGo, new Color(0.2f, 1f, 0.65f, 0.85f), 30f, 1.5f, boxSize: new Vector3(2f, 1.5f, 1f));

            SetSerializedObjectReference(altarScript, "jadeAuraEffect", auraGo);

            auraGo.SetActive(false);



            // -------------------------------------------------------------

            // GROUP 2: CAMERAS

            // -------------------------------------------------------------

            GameObject camGo = new GameObject("Main Camera");

            camGo.transform.SetParent(rootCameras.transform, false);

            camGo.tag = "MainCamera";

            camGo.transform.position = new Vector3(-4.5f, -2.5f, -10f);



            var cam = camGo.AddComponent<Camera>();

            cam.orthographic = true;

            cam.orthographicSize = 6.5f;

            cam.clearFlags = CameraClearFlags.SolidColor;

            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);



            camGo.AddComponent<AudioListener>();



            var camCtrl = camGo.AddComponent<Map3_CameraController>();

            SetSerializedObjectReference(camCtrl, "target", playerGo.transform);

            SetSerializedObjectReference(camCtrl, "currentBounds", gardenCamCol);



            // -------------------------------------------------------------

            // GROUP 5: UI CANVAS

            // -------------------------------------------------------------

            var canvas = rootCanvas.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;



            var scaler = rootCanvas.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution = new Vector2(1920, 1080);

            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            scaler.matchWidthOrHeight = 0.5f;



            rootCanvas.AddComponent<GraphicRaycaster>();



            // 1. HUD_TopLeft_Objective

            GameObject objPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopLeft_Objective",

                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),

                new Vector2(30f, -30f), new Vector2(420f, 65f),

                uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.88f));



            GameObject objTxtGo = CreateUIText(objPanelGo.transform, "Txt_Objective",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                "MỤC TIÊU: Khám phá Vườn Giao Thoa", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);

            var txtObjective = objTxtGo.GetComponent<TextMeshProUGUI>();



            // 2. HUD_TopCenter_Quest

            GameObject questPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopCenter_Quest",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),

                new Vector2(0f, -30f), new Vector2(820f, 65f),

                uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.92f));



            GameObject questTxtGo = CreateUIText(questPanelGo.transform, "Txt_QuestHint",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                "Tương tác với chú Mèo để tìm gợi ý...", 21, FontStyles.Bold, new Color(1f, 0.84f, 0.31f, 1f), TextAlignmentOptions.Center, defaultFont);

            var txtQuestHint = questTxtGo.GetComponent<TextMeshProUGUI>();



            // 3. HUD_BottomCenter_Inventory (5 Slots matching Map 1 & Map 2)

            GameObject invPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_BottomCenter_Inventory",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),

                new Vector2(0f, 35f), new Vector2(460f, 90f),

                inventorySprite != null ? inventorySprite : uiSprite,

                inventorySprite != null ? Color.white : new Color(0.12f, 0.15f, 0.2f, 0.88f));



            Image[] invSlotIcons = new Image[5];

            Image[] invSlotBgs = new Image[5];

            TextMeshProUGUI[] invSlotLabels = new TextMeshProUGUI[5];

            float startX = -170f;

            float slotSpacing = 85f;



            for (int i = 0; i < 5; i++)

            {

                GameObject slotGo = CreateUIPanel(invPanelGo.transform, $"Slot_0{i + 1}",

                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                    new Vector2(startX + i * slotSpacing, 0f), new Vector2(68f, 68f),

                    uiSprite, new Color(0.25f, 0.3f, 0.38f, 0.92f));

                invSlotBgs[i] = slotGo.GetComponent<Image>();



                GameObject iconGo = CreateUIPanel(slotGo.transform, "Icon_Item",

                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                    new Vector2(0f, 4f), new Vector2(48f, 48f),

                    squareSprite, Color.white);

                invSlotIcons[i] = iconGo.GetComponent<Image>();

                iconGo.SetActive(false);



                GameObject labelGo = CreateUIText(slotGo.transform, "Txt_ItemName",

                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),

                    new Vector2(0f, 8f), new Vector2(66f, 18f),

                    "", 11, FontStyles.Normal, Color.white, TextAlignmentOptions.Center, defaultFont);

                invSlotLabels[i] = labelGo.GetComponent<TextMeshProUGUI>();

                labelGo.SetActive(false);

            }



            // 4. Controls_Mobile

            GameObject controlsGo = new GameObject("Controls_Mobile");

            controlsGo.transform.SetParent(rootCanvas.transform, false);

            var controlsRt = controlsGo.AddComponent<RectTransform>();

            controlsRt.anchorMin = Vector2.zero;

            controlsRt.anchorMax = Vector2.one;

            controlsRt.offsetMin = Vector2.zero;

            controlsRt.offsetMax = Vector2.zero;



            // Virtual Joystick

            GameObject joystickBaseGo = CreateUIPanel(controlsGo.transform, "Virtual_Joystick",

                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(160f, 160f), new Vector2(170f, 170f),

                uiSprite, new Color(1f, 1f, 1f, 0.25f));



            GameObject joystickKnobGo = CreateUIPanel(joystickBaseGo.transform, "Joystick_Knob",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(75f, 75f),

                uiSprite, new Color(1f, 1f, 1f, 0.75f));



            var vj = joystickBaseGo.AddComponent<Map3_VirtualJoystick>();

            SetSerializedObjectReference(vj, "joystickHandle", joystickKnobGo.GetComponent<RectTransform>());

            SetSerializedObjectReference(vj, "playerController", playerCtrl);



            // Btn_Interact

            GameObject btnInteractGo = CreateUIButton(controlsGo.transform, "Btn_Interact",

                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(-160f, 160f), new Vector2(180f, 80f),

                buttonSprite != null ? buttonSprite : uiSprite,

                new Color(0.18f, 0.7f, 0.35f, 1f),

                "TƯƠNG TÁC [E]", 22, FontStyles.Bold, Color.white, defaultFont);



            var interactBtnScript = btnInteractGo.AddComponent<Map3_InteractButton>();

            SetSerializedObjectReference(interactBtnScript, "playerController", playerCtrl);

            SetSerializedObjectReference(interactBtnScript, "buttonImage", btnInteractGo.GetComponent<Image>());

            SetSerializedObjectReference(interactBtnScript, "buttonText", btnInteractGo.GetComponentInChildren<TextMeshProUGUI>());



            // 5. Toast Notification Panel (Styled with compact Toast Frame)

            GameObject toastPanelGo = CreateUIPanel(rootCanvas.transform, "Panel_Toast",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),

                new Vector2(0f, -105f), new Vector2(860f, 142f),

                toastFrameSprite != null ? toastFrameSprite : uiSprite, Color.white);

            var toastImg = toastPanelGo.GetComponent<Image>();

            if (toastImg != null && toastFrameSprite != null)

            {

                toastImg.type = Image.Type.Sliced;

            }



            GameObject txtToastTitleGo = CreateUIText(toastPanelGo.transform, "Txt_Toast_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -18f), new Vector2(700f, 26f),

                "THÔNG BÁO", 20, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            GameObject txtToastGo = CreateUIText(toastPanelGo.transform, "Txt_Toast",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                new Vector2(60f, 26f), new Vector2(-60f, -38f),

                "Thông báo trò chơi...", 20, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);

            var txtToastTmp = txtToastGo.GetComponent<TextMeshProUGUI>();

            if (txtToastTmp != null)

            {

                txtToastTmp.enableAutoSizing = false;

                txtToastTmp.fontSize = 20f;

                txtToastTmp.fontStyle = FontStyles.Bold;

                txtToastTmp.lineSpacing = 4f;

                txtToastTmp.paragraphSpacing = 0f;

                txtToastTmp.textWrappingMode = TextWrappingModes.Normal;

                txtToastTmp.overflowMode = TextOverflowModes.Overflow;

            }

            toastPanelGo.SetActive(false);



            // 5b. Panel_Hint_Dialog (Cat Hint Dialog Modal with Quiz Card Frame)

            GameObject hintModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Hint_Dialog",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject hintCardGo = CreateUIPanel(hintModalGo.transform, "Card_Hint",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(880f, 480f),

                quizPanelSprite != null ? quizPanelSprite : uiSprite, Color.white);

            var hintCardImg = hintCardGo.GetComponent<Image>();

            if (hintCardImg != null && quizPanelSprite != null)

            {

                hintCardImg.type = Image.Type.Sliced;

            }



            GameObject btnCloseHintGo = CreateUIButton(hintCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-35f, -35f), new Vector2(44f, 44f),

                uiSprite, HexColor("#D32F2F"),

                "X", 24, FontStyles.Bold, Color.white, defaultFont);



            GameObject txtHintTitleGo = CreateUIText(hintCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -48f), new Vector2(700f, 45f),

                "GỢI Ý TỪ CHÚ MÈO", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            // Cat Avatar on the left

            GameObject catAvatarBorderGo = CreateUIPanel(hintCardGo.transform, "Border_CatAvatar",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(-270f, 15f), new Vector2(130f, 130f),

                buttonSprite != null ? buttonSprite : uiSprite, HexColor("#4E342E"));



            GameObject catAvatarGo = CreateUIPanel(catAvatarBorderGo.transform, "Img_CatAvatar",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(100f, 100f),

                catSprite != null ? catSprite : squareSprite, Color.white);

            var catAvatarImg = catAvatarGo.GetComponent<Image>();

            if (catAvatarImg != null) catAvatarImg.preserveAspect = true;



            // Content message on right

            GameObject txtHintMessageGo = CreateUIText(hintCardGo.transform, "Txt_Message",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(85f, 15f), new Vector2(530f, 220f),

                Map3_CatHint.CatHintMessage, 21f, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.MidlineLeft, defaultFont);

            var txtHintMsgTmp = txtHintMessageGo.GetComponent<TextMeshProUGUI>();

            if (txtHintMsgTmp != null)

            {

                txtHintMsgTmp.textWrappingMode = TextWrappingModes.Normal;

                txtHintMsgTmp.lineSpacing = 6f;

            }



            GameObject btnConfirmHintGo = CreateUIButton(hintCardGo.transform, "Btn_Confirm",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 52f), new Vector2(250f, 52f),

                buttonSprite != null ? buttonSprite : uiSprite, Color.white,

                "ĐÃ HIỂU", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            hintModalGo.SetActive(false);



            // 6. Interaction Prompt [E]

            GameObject promptPanelGo = CreateUIPanel(rootCanvas.transform, "Prompt_Interact",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 140f), new Vector2(360f, 50f),

                uiSprite, new Color(0.1f, 0.12f, 0.18f, 0.85f));



            GameObject txtPromptGo = CreateUIText(promptPanelGo.transform, "Txt_Prompt",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                "[E] Tương tác", 21, FontStyles.Bold, new Color(1f, 0.85f, 0.2f, 1f), TextAlignmentOptions.Center, defaultFont);

            promptPanelGo.SetActive(false);



            // 7. Panel_Quiz_Dialog

            GameObject quizModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Quiz_Dialog",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.8f));



            GameObject quizCardGo = CreateUIPanel(quizModalGo.transform, "Card_Content",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1400f, 760f),
                quizPanelSprite != null ? quizPanelSprite : uiSprite,
                quizPanelSprite != null ? Color.white : new Color(0.12f, 0.14f, 0.18f, 1f));

            var cardImg = quizCardGo.GetComponent<Image>();
            if (cardImg != null)
            {
                cardImg.type = Image.Type.Sliced;
                cardImg.preserveAspect = false;
            }

            // Close button
            GameObject btnCloseGo = CreateUIButton(quizCardGo.transform, "Btn_Close",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-40f, -40f), new Vector2(44f, 44f),
                uiSprite, new Color(0.85f, 0.2f, 0.2f, 1f),
                "X", 24, FontStyles.Bold, Color.white, defaultFont);

            // Title
            GameObject txtTitleGo = CreateUIText(quizCardGo.transform, "Txt_Title",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 312f), new Vector2(950f, 54f),
                "CÂU HỎI VĂN HÓA", 26, FontStyles.Bold, new Color(1f, 0.835f, 0.31f, 1f), TextAlignmentOptions.Center, defaultFont);

            // --- LEFT COLUMN: ILLUSTRATION PANEL ---
            GameObject panelIllustrationGo = CreateUIPanel(quizCardGo.transform, "Panel_Illustration",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-440f, -40f), new Vector2(390f, 520f),
                uiSprite, new Color32(20, 24, 32, 255));

            GameObject imgIllustrationGo = CreateUIPanel(panelIllustrationGo.transform, "Img_Illustration",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(310f, 310f),
                null, Color.white);
            var imgIllustration = imgIllustrationGo.GetComponent<Image>();
            imgIllustration.preserveAspect = true;

            GameObject txtNameGo = CreateUIText(panelIllustrationGo.transform, "Txt_Illustration_Name",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -175f), new Vector2(370f, 46f),
                "CHÚ MÈO", 24, FontStyles.Bold, new Color(1f, 0.835f, 0.31f, 1f), TextAlignmentOptions.Center, defaultFont);

            // --- RIGHT COLUMN: QUESTION & CHOICES ---
            // Question (hạ xuống Y = 135 để nằm trọn vẹn trong khung vàng, không tràn lên viền gỗ)
            GameObject txtQuestContentGo = CreateUIText(quizCardGo.transform, "Txt_Question",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(235f, 125f), new Vector2(850f, 130f),
                "Nội dung câu hỏi văn hóa...", 28f, FontStyles.Normal, new Color(0.22f, 0.13f, 0.09f, 1f), TextAlignmentOptions.TopLeft, defaultFont);

            var txtQuestionTmp = txtQuestContentGo.GetComponent<TextMeshProUGUI>();
            txtQuestionTmp.margin = new Vector4(12f, 4f, 12f, 4f);
            txtQuestionTmp.enableAutoSizing = true;
            txtQuestionTmp.fontSizeMin = 24f;
            txtQuestionTmp.fontSizeMax = 28f;
            txtQuestionTmp.textWrappingMode = TextWrappingModes.Normal;
            txtQuestionTmp.overflowMode = TextOverflowModes.Ellipsis;

            // Feedback Text
            GameObject txtFeedGo = CreateUIText(quizCardGo.transform, "Txt_Feedback",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(235f, 48f), new Vector2(850f, 26f),
                "", 19, FontStyles.Bold, new Color(0.2f, 0.9f, 0.4f, 1f), TextAlignmentOptions.Center, defaultFont);

            // 4 Answer Buttons
            Button[] ansBtns = new Button[4];
            TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];
            string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };
            float[] btnYs = { -6f, -92f, -178f, -264f };

            for (int i = 0; i < 4; i++)
            {
                GameObject aBtnGo = CreateUIButton(quizCardGo.transform, btnNames[i],
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(235f, btnYs[i]), new Vector2(850f, 78f),
                    buttonSprite != null ? buttonSprite : uiSprite,
                    new Color32(78, 52, 46, 255), // #4E342E
                    $"Phương án {i + 1}", 21, FontStyles.Normal, Color.white, defaultFont);

                ansBtns[i] = aBtnGo.GetComponent<Button>();
                ansTexts[i] = aBtnGo.GetComponentInChildren<TextMeshProUGUI>();
                ansTexts[i].alignment = TextAlignmentOptions.MidlineLeft;
                ansTexts[i].margin = new Vector4(24f, 2f, 24f, 2f);
                ansTexts[i].enableAutoSizing = true;
                ansTexts[i].fontSizeMin = 18f;
                ansTexts[i].fontSizeMax = 21f;
                ansTexts[i].textWrappingMode = TextWrappingModes.Normal;
                ansTexts[i].overflowMode = TextOverflowModes.Ellipsis;
            }

            // Explanation Text
            GameObject txtExplanationGo = CreateUIText(quizCardGo.transform, "Txt_Explanation",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(220f, -255f), new Vector2(840f, 76f),
                "", 17, FontStyles.Normal, new Color(0.9f, 0.95f, 1f, 1f), TextAlignmentOptions.TopLeft, defaultFont);

            var txtExplanationTmp = txtExplanationGo.GetComponent<TextMeshProUGUI>();
            txtExplanationTmp.textWrappingMode = TextWrappingModes.Normal;
            txtExplanationGo.SetActive(false);

            // Continue Button (shown after correct answer to proceed)
            GameObject btnContinueGo = CreateUIButton(quizCardGo.transform, "Btn_Continue",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(220f, -255f), new Vector2(220f, 50f),
                uiSprite, new Color(0.18f, 0.7f, 0.35f, 1f),
                "Tiếp tục", 20, FontStyles.Bold, Color.white, defaultFont);
            btnContinueGo.SetActive(false);

            quizModalGo.SetActive(false);



            Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");
            if (quizCardSprite == null) quizCardSprite = uiSprite;

            // 8. Panel_Victory

            GameObject victoryModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Victory",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.88f));



            GameObject victoryCardGo = CreateUIPanel(victoryModalGo.transform, "Card_Victory",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(780f, 440f),

                quizCardSprite != null ? quizCardSprite : uiSprite, Color.white);

            var vicCardImg = victoryCardGo.GetComponent<Image>();

            if (vicCardImg != null)

            {

                vicCardImg.type = Image.Type.Sliced;

                vicCardImg.preserveAspect = false;

            }



            CreateUIText(victoryCardGo.transform, "Txt_Victory_Header",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -48f), new Vector2(680f, 48f),
                "XUẤT SẮC!", 34, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);

            GameObject txtVicTitleGo = CreateUIText(victoryCardGo.transform, "Txt_Victory_Title",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 45f), new Vector2(700f, 65f),
                "HOÀN THÀNH MÀN CUỐI: KHU VƯỜN GIAO THOA!", 26, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);

            GameObject txtVicDescGo = CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -32f), new Vector2(680f, 85f),
                "Chúc mừng bạn đã hợp nhất thành công hai nửa Ngọc Bội Việt - Hàn!\nKý ức gia đình và tình hữu nghị hai dân tộc đã được gắn kết trọn vẹn!", 21, FontStyles.Bold, HexColor("#4E342E"), TextAlignmentOptions.Center, defaultFont);

            Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");
            if (btnWoodSprite == null) btnWoodSprite = uiSprite;

            GameObject btnRestartGo = CreateUIButton(victoryCardGo.transform, "Btn_Restart",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 60f), new Vector2(240f, 52f),
                btnWoodSprite, Color.white,
                "Chơi lại", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            victoryModalGo.SetActive(false);



            // 10. Top-Right HUD Buttons & Modals (Pause, Sound, Home, Guide)

            SetupTopRightHUDAndModals(canvas, defaultFont, uiSprite);



            // 9. Fade Overlay (Topmost UI element)

            GameObject fadeGo = CreateUIPanel(rootCanvas.transform, "Fade_Overlay",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                squareSprite, Color.black);

            var fadeImg = fadeGo.GetComponent<Image>();

            fadeImg.raycastTarget = false;

            Color fc = fadeImg.color;

            fc.a = 0f;

            fadeImg.color = fc;

            fadeGo.transform.SetAsLastSibling();



            // -------------------------------------------------------------

            // WIRE ALL SERIALIZED REFERENCES

            // -------------------------------------------------------------

            // Wire Map3_InventoryManager

            SerializedObject soIm = new SerializedObject(im);

            SerializedProperty spSlots = soIm.FindProperty("slotIcons");

            spSlots.arraySize = 5;

            for (int i = 0; i < 5; i++) spSlots.GetArrayElementAtIndex(i).objectReferenceValue = invSlotIcons[i];



            SerializedProperty spBgs = soIm.FindProperty("slotBackgrounds");

            spBgs.arraySize = 5;

            for (int i = 0; i < 5; i++) spBgs.GetArrayElementAtIndex(i).objectReferenceValue = invSlotBgs[i];



            SerializedProperty spLabels = soIm.FindProperty("slotLabels");

            spLabels.arraySize = 5;

            for (int i = 0; i < 5; i++) spLabels.GetArrayElementAtIndex(i).objectReferenceValue = invSlotLabels[i];

            soIm.ApplyModifiedPropertiesWithoutUndo();



            // Wire Map3_QuizManager

            SerializedObject soQm = new SerializedObject(qm);

            soQm.FindProperty("quizPanel").objectReferenceValue = quizModalGo;

            soQm.FindProperty("txtTitle").objectReferenceValue = txtTitleGo.GetComponent<TextMeshProUGUI>();

            soQm.FindProperty("txtQuestion").objectReferenceValue = txtQuestionTmp;

            soQm.FindProperty("txtFeedback").objectReferenceValue = txtFeedGo.GetComponent<TextMeshProUGUI>();

            soQm.FindProperty("txtExplanation").objectReferenceValue = txtExplanationTmp;

            soQm.FindProperty("btnContinue").objectReferenceValue = btnContinueGo.GetComponent<Button>();

            soQm.FindProperty("btnClose").objectReferenceValue = btnCloseGo.GetComponent<Button>();

            soQm.FindProperty("illustrationContainer").objectReferenceValue = panelIllustrationGo;
            soQm.FindProperty("imgIllustration").objectReferenceValue = imgIllustration;
            soQm.FindProperty("txtIllustrationName").objectReferenceValue = txtNameGo.GetComponent<TextMeshProUGUI>();



            SerializedProperty spAnsBtns = soQm.FindProperty("answerButtons");

            spAnsBtns.arraySize = 4;

            for (int i = 0; i < 4; i++) spAnsBtns.GetArrayElementAtIndex(i).objectReferenceValue = ansBtns[i];



            SerializedProperty spAnsTxts = soQm.FindProperty("answerTexts");

            spAnsTxts.arraySize = 4;

            for (int i = 0; i < 4; i++) spAnsTxts.GetArrayElementAtIndex(i).objectReferenceValue = ansTexts[i];

            soQm.ApplyModifiedPropertiesWithoutUndo();



            // Wire Map3_UIManager

            SerializedObject soUm = new SerializedObject(um);

            soUm.FindProperty("fadeOverlay").objectReferenceValue = fadeImg;

            soUm.FindProperty("toastPanel").objectReferenceValue = toastPanelGo;

            soUm.FindProperty("txtToast").objectReferenceValue = txtToastGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("txtToastTitle").objectReferenceValue = txtToastTitleGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("hintDialogPanel").objectReferenceValue = hintModalGo;

            soUm.FindProperty("txtHintTitle").objectReferenceValue = txtHintTitleGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("txtHintMessage").objectReferenceValue = txtHintMessageGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("btnHintConfirm").objectReferenceValue = btnConfirmHintGo.GetComponent<Button>();

            soUm.FindProperty("btnHintClose").objectReferenceValue = btnCloseHintGo.GetComponent<Button>();
            Transform catHintBtnFound = canvas.transform.Find("Btn_CatHint");
            if (catHintBtnFound != null)
            {
                soUm.FindProperty("btnCatHint").objectReferenceValue = catHintBtnFound.GetComponent<Button>();
            }

            soUm.FindProperty("interactPrompt").objectReferenceValue = promptPanelGo;

            soUm.FindProperty("txtInteractPrompt").objectReferenceValue = txtPromptGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("txtObjective").objectReferenceValue = txtObjective;

            soUm.FindProperty("txtQuestHint").objectReferenceValue = txtQuestHint;

            soUm.FindProperty("victoryPanel").objectReferenceValue = victoryModalGo;

            soUm.FindProperty("txtVictoryTitle").objectReferenceValue = txtVicTitleGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("txtVictoryDesc").objectReferenceValue = txtVicDescGo.GetComponent<TextMeshProUGUI>();

            soUm.FindProperty("btnRestart").objectReferenceValue = btnRestartGo.GetComponent<Button>();

            soUm.ApplyModifiedPropertiesWithoutUndo();



            // 5. Save Scene

            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log($"[Map3_SceneBuilder] Successfully saved new scene to: {ScenePath}");



            // 6. Ensure scene is registered in EditorBuildSettings

            AddSceneToBuildSettings(ScenePath);



            Debug.Log("[Map3_SceneBuilder] Map3_Final Scene build COMPLETE and READY TO PLAY!");

        }



        private static Sprite LoadSprite(string path)

        {

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null) return sprite;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in all)

            {

                if (obj is Sprite s) return s;

            }

            return null;

        }



        private static GameObject CreateInteractableRoot(Transform parent, string name, Vector3 pos,

            string displayName, string prompt, TMP_FontAsset font, Color gizmoCol, Vector2? colliderSize = null)

        {

            GameObject go = new GameObject(name);

            go.transform.SetParent(parent, false);

            go.transform.position = pos;



            var col = go.AddComponent<BoxCollider2D>();

            col.isTrigger = true;

            col.size = colliderSize ?? new Vector2(1.8f, 1.8f);



            GameObject indGo = new GameObject("Interact_Indicator");

            indGo.transform.SetParent(go.transform, false);

            indGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);



            var tmpInd = indGo.AddComponent<TextMeshPro>();

            if (font != null) tmpInd.font = font;

            tmpInd.text = "[!]";

            tmpInd.fontSize = 3.6f;

            tmpInd.fontStyle = FontStyles.Bold;

            tmpInd.alignment = TextAlignmentOptions.Center;

            tmpInd.color = new Color(1f, 0.84f, 0.31f, 1f);

            tmpInd.sortingOrder = 12;

            indGo.SetActive(false);



            return go;

        }



        private static void CreatePickupVisual(Transform parent, Sprite sprite, Vector2 size, Color color, int sortingOrder)

        {

            GameObject boxGo = new GameObject("Visual_Plank");

            boxGo.transform.SetParent(parent, false);

            boxGo.transform.localPosition = Vector3.zero;

            boxGo.transform.localScale = new Vector3(size.x, size.y, 1f);



            var sr = boxGo.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;

            sr.color = color;

            sr.sortingOrder = sortingOrder;

        }



        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector2 size)

        {

            GameObject wall = new GameObject(name);

            wall.transform.SetParent(parent, false);

            wall.transform.localPosition = pos;



            var col = wall.AddComponent<BoxCollider2D>();

            col.isTrigger = false;

            col.size = size;

        }



        private static ParticleSystem CreateParticleSystem(GameObject go, Color color, float emissionRate, float lifetime, Vector3? boxSize = null)

        {

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;

            main.startLifetime = lifetime;

            main.startSpeed = 0.5f;

            main.startSize = 0.25f;

            main.startColor = color;

            main.simulationSpace = ParticleSystemSimulationSpace.World;

            main.playOnAwake = false;



            var emission = ps.emission;

            emission.rateOverTime = emissionRate;



            var shape = ps.shape;

            shape.shapeType = ParticleSystemShapeType.Box;

            shape.scale = boxSize ?? new Vector3(2f, 2f, 1f);



            var renderer = go.GetComponent<ParticleSystemRenderer>();

            renderer.sortingOrder = 10;



            return ps;

        }



        private static GameObject CreateUIPanel(Transform parent, string name,

            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,

            Vector2 anchoredPosition, Vector2 sizeDelta,

            Sprite sprite, Color color)

        {

            GameObject go = new GameObject(name);

            go.transform.SetParent(parent, false);



            var rt = go.AddComponent<RectTransform>();

            rt.anchorMin = anchorMin;

            rt.anchorMax = anchorMax;

            rt.pivot = pivot;

            rt.anchoredPosition = anchoredPosition;

            rt.sizeDelta = sizeDelta;



            var img = go.AddComponent<Image>();

            img.sprite = sprite;

            img.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;

            img.color = color;



            return go;

        }



        private static GameObject CreateUIButton(Transform parent, string name,

            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,

            Vector2 anchoredPosition, Vector2 sizeDelta,

            Sprite sprite, Color color,

            string text, float fontSize, FontStyles style, Color textColor, TMP_FontAsset font)

        {

            GameObject btnGo = CreateUIPanel(parent, name, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, sprite, color);

            btnGo.AddComponent<Button>();



            GameObject txtGo = CreateUIText(btnGo.transform, "Text",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                text, fontSize, style, textColor, TextAlignmentOptions.Center, font);



            return btnGo;

        }



        private static GameObject CreateUIText(Transform parent, string name,

            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,

            Vector2 anchoredPosition, Vector2 sizeDelta,

            string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, TMP_FontAsset font)

        {

            GameObject go = new GameObject(name);

            go.transform.SetParent(parent, false);



            var rt = go.AddComponent<RectTransform>();

            rt.anchorMin = anchorMin;

            rt.anchorMax = anchorMax;

            rt.pivot = pivot;

            rt.anchoredPosition = anchoredPosition;

            rt.sizeDelta = sizeDelta;



            var tmp = go.AddComponent<TextMeshProUGUI>();

            if (font != null) tmp.font = font;

            tmp.text = text;

            tmp.fontSize = fontSize;

            tmp.fontStyle = style;

            tmp.color = color;

            tmp.alignment = alignment;

            tmp.raycastTarget = false;



            return go;

        }



        private static void AddSceneToBuildSettings(string path)

        {

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (!scenes.Any(s => s.path == path))

            {

                scenes.Add(new EditorBuildSettingsScene(path, true));

                EditorBuildSettings.scenes = scenes.ToArray();

                Debug.Log($"[Map3_SceneBuilder] Added {path} to EditorBuildSettings.");

            }

        }



        private static void SetSerializedObjectReference(Component target, string propName, Object value)

        {

            if (target == null) return;

            SerializedObject so = new SerializedObject(target);

            SerializedProperty prop = so.FindProperty(propName);

            if (prop != null)

            {

                prop.objectReferenceValue = value;

                so.ApplyModifiedPropertiesWithoutUndo();

            }

            else

            {

                Debug.LogWarning($"[Map3_SceneBuilder] Property '{propName}' not found on {target.GetType().Name}");

            }

        }



        private static void SetSerializedBool(Component target, string propName, bool value)

        {

            if (target == null) return;

            SerializedObject so = new SerializedObject(target);

            SerializedProperty prop = so.FindProperty(propName);

            if (prop != null)

            {

                prop.boolValue = value;

                so.ApplyModifiedPropertiesWithoutUndo();

            }

        }



        private static void SetSerializedVector3(Component target, string propName, Vector3 value)

        {

            if (target == null) return;

            SerializedObject so = new SerializedObject(target);

            SerializedProperty prop = so.FindProperty(propName);

            if (prop != null)

            {

                prop.vector3Value = value;

                so.ApplyModifiedPropertiesWithoutUndo();

            }

        }



        public static void SetupTopRightHUDAndModals(Canvas rootCanvas, TMP_FontAsset defaultFont, Sprite uiSprite)

        {

            if (rootCanvas == null) return;



            // Clean existing elements if present (idempotent)

            Transform existingTopRight = rootCanvas.transform.Find("HUD_TopRight_Buttons");

            if (existingTopRight != null) Object.DestroyImmediate(existingTopRight.gameObject);

            Transform existingPause = rootCanvas.transform.Find("Panel_Pause");

            if (existingPause != null) Object.DestroyImmediate(existingPause.gameObject);

            Transform existingGuide = rootCanvas.transform.Find("Panel_Guide");

            if (existingGuide != null) Object.DestroyImmediate(existingGuide.gameObject);

            Transform existingConfirmHome = rootCanvas.transform.Find("Panel_Confirm_Home");

            if (existingConfirmHome != null) Object.DestroyImmediate(existingConfirmHome.gameObject);

            Transform existingSoundModal = rootCanvas.transform.Find("Panel_Sound");

            if (existingSoundModal != null) Object.DestroyImmediate(existingSoundModal.gameObject);

            Transform existingCatHint = rootCanvas.transform.Find("Btn_CatHint");
            if (existingCatHint != null) Object.DestroyImmediate(existingCatHint.gameObject);



            // Load Sprites

            Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");

            if (quizCardSprite == null) quizCardSprite = uiSprite;



            Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");

            if (btnWoodSprite == null) btnWoodSprite = uiSprite;



            Sprite iconPause = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Pause.png");

            Sprite iconSoundOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Sound_On.png");

            Sprite iconSoundOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Sound_Off.png");

            Sprite iconHome = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Home.png");

            Sprite iconHelp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Help.png");

            Sprite iconExclamation = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Exclamation.png");
            if (iconExclamation == null) iconExclamation = iconHelp;



            // Container: HUD_TopRight_Buttons

            GameObject topRightContainer = new GameObject("HUD_TopRight_Buttons");

            topRightContainer.transform.SetParent(rootCanvas.transform, false);

            var rtContainer = topRightContainer.AddComponent<RectTransform>();

            rtContainer.anchorMin = new Vector2(1f, 1f);

            rtContainer.anchorMax = new Vector2(1f, 1f);

            rtContainer.pivot = new Vector2(1f, 1f);

            rtContainer.anchoredPosition = new Vector2(-30f, -30f);

            rtContainer.sizeDelta = new Vector2(250f, 60f);



            var hlg = topRightContainer.AddComponent<HorizontalLayoutGroup>();

            hlg.spacing = 10f;

            hlg.childAlignment = TextAnchor.MiddleRight;

            hlg.childControlWidth = false;

            hlg.childControlHeight = false;

            hlg.childForceExpandWidth = false;

            hlg.childForceExpandHeight = false;



            // 4 Buttons in order: Guide (?), Sound, Pause, Home

            GameObject btnHelpGo = CreateIconButton(topRightContainer.transform, "Btn_Help", btnWoodSprite, iconHelp, 54f);

            GameObject btnSoundGo = CreateIconButton(topRightContainer.transform, "Btn_Sound", btnWoodSprite, iconSoundOn, 54f);

            GameObject btnPauseGo = CreateIconButton(topRightContainer.transform, "Btn_Pause", btnWoodSprite, iconPause, 54f);

            GameObject btnHomeGo = CreateIconButton(topRightContainer.transform, "Btn_Home", btnWoodSprite, iconHome, 54f);



            Image imgSound = btnSoundGo.transform.Find("Icon")?.GetComponent<Image>();

            // Cat Hint [ ! ] Shortcut Button (Placed ~1.5 cm / 74px to the left of [ ? ] / Btn_Help)
            // Btn_Help is from X=-276 to X=-222. With 74px margin, Btn_CatHint right edge is at -350px.
            GameObject btnCatHintGo = CreateIconButton(rootCanvas.transform, "Btn_CatHint", btnWoodSprite, iconExclamation, 54f);
            var rtCatHint = btnCatHintGo.GetComponent<RectTransform>();
            rtCatHint.anchorMin = new Vector2(1f, 1f);
            rtCatHint.anchorMax = new Vector2(1f, 1f);
            rtCatHint.pivot = new Vector2(1f, 1f);
            rtCatHint.anchoredPosition = new Vector2(-350f, -33f);
            btnCatHintGo.SetActive(false);



            // 2. Panel_Pause

            GameObject pauseModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Pause",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject pauseCardGo = CreateUIPanel(pauseModalGo.transform, "Card_Pause",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(620f, 470f),

                quizCardSprite, Color.white);



            CreateUIText(pauseCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -55f), new Vector2(450f, 45f),

                "TẠM DỪNG", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            CreateUIText(pauseCardGo.transform, "Txt_Subtitle",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -125f), new Vector2(480f, 32f),

                "Trò chơi đang tạm dừng.", 18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



            GameObject btnResumePause = CreateUIButton(pauseCardGo.transform, "Btn_Resume",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -190f), new Vector2(360f, 48f),

                btnWoodSprite, Color.white,

                "Tiếp tục", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            GameObject btnRestartPause = CreateUIButton(pauseCardGo.transform, "Btn_Restart",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -250f), new Vector2(360f, 48f),

                btnWoodSprite, Color.white,

                "Chơi lại màn 3", 22, FontStyles.Bold, HexColor("#FFE082"), defaultFont);



            GameObject btnHomePause = CreateUIButton(pauseCardGo.transform, "Btn_Home",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -310f), new Vector2(360f, 48f),

                btnWoodSprite, Color.white,

                "Về trang chủ", 22, FontStyles.Bold, Color.white, defaultFont);



            GameObject btnClosePause = CreateUIButton(pauseCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-55f, -55f), new Vector2(44f, 44f),

                btnWoodSprite, Color.white,

                "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            pauseModalGo.SetActive(false);



            // 3. Panel_Guide (Tutorial / Help)

            GameObject guideModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Guide",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject guideCardGo = CreateUIPanel(guideModalGo.transform, "Card_Guide",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(1100f, 700f),

                quizCardSprite, Color.white);



            CreateUIText(guideCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -55f), new Vector2(850f, 44f),

                "HƯỚNG DẪN CHƠI - BẢN ĐỒ 3", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            string guideText =

                "<color=#7C2D12><b>1. HỎI GỢI Ý TỪ MÈO:</b></color>\n" +

                "Gặp và trò chuyện với <b>Chú Mèo</b> để nhận gợi ý khám phá khu vườn và mở khóa câu hỏi văn hóa.\n\n" +

                "<color=#7C2D12><b>2. THU THẬP NGUYÊN LIỆU & BẢO VẬT:</b></color>\n" +

                "• <b>Chìa khóa</b> (trong Lu sành cổ)    • <b>3 thanh Gỗ quý</b>    • <b>Đá cầu nguyện</b>\n" +

                "• <b>Ngọc bích Việt Nam</b> (trong Rương đền Việt)    • <b>Ngọc bích Hàn Quốc</b> (trên Tháp đá Doltap)\n\n" +

                "<color=#7C2D12><b>3. SỬA CẦU VÀ MỞ CỔNG ĐỀN:</b></color>\n" +

                "Mang 3 thanh gỗ đến bờ suối để <b>sửa cầu qua suối</b>, sau đó dùng Chìa khóa để mở cổng <b>Đền Việt</b> và <b>Đền Hàn</b>.\n\n" +

                "<color=#7C2D12><b>4. KÍCH HOẠT ĐỀN THỜ HÒA HỢP:</b></color>\n" +

                "Đặt cả 2 viên ngọc lên <b>Bàn thờ hòa hợp</b> để kết nối tinh hoa văn hóa và hoàn thành chuyến hành trình!\n\n" +

                "<color=#7C2D12><b>ĐIỀU KHIỂN:</b></color>\n" +

                "• Di chuyển: Dùng <b>Joystick ảo</b> hoặc các phím <b>W, A, S, D / Mũi tên</b>.\n" +

                "• Tương tác: Nhấn nút <b>TƯƠNG TÁC [E]</b> hoặc phím <b>[E] / [Space]</b> khi đứng gần mục tiêu.";



            var txtContentGo = CreateUIText(guideCardGo.transform, "Txt_Content",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -20f), new Vector2(920f, 470f),

                guideText, 19.5f, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.TopLeft, defaultFont);

            var contentTmp = txtContentGo.GetComponent<TextMeshProUGUI>();

            if (contentTmp != null)

            {

                contentTmp.alignment = TextAlignmentOptions.TopLeft;

                contentTmp.margin = new Vector4(12f, 8f, 12f, 8f);

                contentTmp.enableAutoSizing = true;

                contentTmp.fontSizeMin = 18f;

                contentTmp.fontSizeMax = 20.5f;

                contentTmp.lineSpacing = 0f;

                contentTmp.paragraphSpacing = 8f;

                contentTmp.textWrappingMode = TextWrappingModes.Normal;

                contentTmp.overflowMode = TextOverflowModes.Truncate;

            }



            GameObject btnCloseGuide = CreateUIButton(guideCardGo.transform, "Btn_Understood",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 48f), new Vector2(240f, 46f),

                btnWoodSprite, Color.white,

                "ĐÃ HIỂU", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            GameObject btnCloseGuideX = CreateUIButton(guideCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-55f, -55f), new Vector2(46f, 46f),

                btnWoodSprite, Color.white,

                "X", 24, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            guideModalGo.SetActive(false);



            // 4. Panel_Confirm_Home

            GameObject confirmHomeModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Confirm_Home",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject confirmHomeCardGo = CreateUIPanel(confirmHomeModalGo.transform, "Card_Confirm_Home",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(620f, 370f),

                quizCardSprite, Color.white);



            CreateUIText(confirmHomeCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -55f), new Vector2(500f, 45f),

                "VỀ TRANG CHỦ", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            CreateUIText(confirmHomeCardGo.transform, "Txt_Msg",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -10f), new Vector2(500f, 85f),

                "Bạn có chắc chắn muốn quay về Trang chủ không?\nTiến trình chưa hoàn thành có thể sẽ phải chơi lại.",

                18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



            GameObject btnYesHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_Yes",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(-110f, 58f), new Vector2(180f, 46f),

                btnWoodSprite, Color.white,

                "Xác nhận", 20, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            GameObject btnNoHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_No",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(110f, 58f), new Vector2(180f, 46f),

                btnWoodSprite, Color.white,

                "Hủy", 20, FontStyles.Bold, Color.white, defaultFont);



            GameObject btnCloseConfirmHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-55f, -55f), new Vector2(44f, 44f),

                btnWoodSprite, Color.white,

                "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            confirmHomeModalGo.SetActive(false);



            // 5. Panel_Sound (Audio Settings Modal)

            GameObject soundModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Sound",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject soundCardGo = CreateUIPanel(soundModalGo.transform, "Card_Sound",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(620f, 420f),

                quizCardSprite, Color.white);



            CreateUIText(soundCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -55f), new Vector2(500f, 45f),

                "CÀI ĐẶT ÂM THANH", 26, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            CreateUIText(soundCardGo.transform, "Txt_Subtitle",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -125f), new Vector2(480f, 32f),

                "Điều chỉnh âm thanh trò chơi", 18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



            GameObject btnToggleSoundModal = CreateUIButton(soundCardGo.transform, "Btn_ToggleSound",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -185f), new Vector2(360f, 50f),

                btnWoodSprite, Color.white,

                "Âm thanh: ĐANG BẬT", 21, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);

            var txtSoundStatusModal = btnToggleSoundModal.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();



            CreateUIText(soundCardGo.transform, "Txt_Hint",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -245f), new Vector2(480f, 30f),

                "Nhấn nút trên để bật hoặc tắt toàn bộ âm thanh", 16, FontStyles.Normal, HexColor("#5D4037"), TextAlignmentOptions.Center, defaultFont);



            GameObject btnCloseSoundBottom = CreateUIButton(soundCardGo.transform, "Btn_Close_Bottom",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 55f), new Vector2(200f, 46f),

                btnWoodSprite, Color.white,

                "Đóng", 20, FontStyles.Bold, Color.white, defaultFont);



            GameObject btnCloseSoundX = CreateUIButton(soundCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-55f, -55f), new Vector2(44f, 44f),

                btnWoodSprite, Color.white,

                "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            soundModalGo.SetActive(false);



            // 6. Add or Wire Map3_GameHUDController

            var hudCtrl = rootCanvas.GetComponent<Map3_GameHUDController>();

            if (hudCtrl == null) hudCtrl = rootCanvas.gameObject.AddComponent<Map3_GameHUDController>();



            SerializedObject soHud = new SerializedObject(hudCtrl);

            soHud.FindProperty("btnPause").objectReferenceValue = btnPauseGo.GetComponent<Button>();

            soHud.FindProperty("btnSound").objectReferenceValue = btnSoundGo.GetComponent<Button>();

            soHud.FindProperty("btnHome").objectReferenceValue = btnHomeGo.GetComponent<Button>();

            soHud.FindProperty("btnHelp").objectReferenceValue = btnHelpGo.GetComponent<Button>();
            soHud.FindProperty("btnCatHint").objectReferenceValue = btnCatHintGo.GetComponent<Button>();



            soHud.FindProperty("imgSoundIcon").objectReferenceValue = imgSound;

            soHud.FindProperty("soundOnSprite").objectReferenceValue = iconSoundOn;

            soHud.FindProperty("soundOffSprite").objectReferenceValue = iconSoundOff;



            soHud.FindProperty("panelPause").objectReferenceValue = pauseModalGo;

            soHud.FindProperty("panelGuide").objectReferenceValue = guideModalGo;

            soHud.FindProperty("panelConfirmHome").objectReferenceValue = confirmHomeModalGo;

            soHud.FindProperty("panelSound").objectReferenceValue = soundModalGo;



            soHud.FindProperty("btnResumePause").objectReferenceValue = btnResumePause.GetComponent<Button>();

            soHud.FindProperty("btnRestartPause").objectReferenceValue = btnRestartPause.GetComponent<Button>();

            soHud.FindProperty("btnHomePause").objectReferenceValue = btnHomePause.GetComponent<Button>();

            soHud.FindProperty("btnClosePause").objectReferenceValue = btnClosePause.GetComponent<Button>();



            soHud.FindProperty("btnCloseGuide").objectReferenceValue = btnCloseGuide.GetComponent<Button>();

            soHud.FindProperty("btnCloseGuideAlt").objectReferenceValue = btnCloseGuideX.GetComponent<Button>();



            soHud.FindProperty("btnConfirmHomeYes").objectReferenceValue = btnYesHome.GetComponent<Button>();

            soHud.FindProperty("btnConfirmHomeNo").objectReferenceValue = btnNoHome.GetComponent<Button>();

            soHud.FindProperty("btnCloseConfirmHome").objectReferenceValue = btnCloseConfirmHome.GetComponent<Button>();



            soHud.FindProperty("btnToggleSoundModal").objectReferenceValue = btnToggleSoundModal.GetComponent<Button>();

            soHud.FindProperty("txtSoundStatusModal").objectReferenceValue = txtSoundStatusModal;

            soHud.FindProperty("btnCloseSound").objectReferenceValue = btnCloseSoundBottom.GetComponent<Button>();

            soHud.FindProperty("btnCloseSoundAlt").objectReferenceValue = btnCloseSoundX.GetComponent<Button>();



            soHud.ApplyModifiedPropertiesWithoutUndo();

            var umInstance = Object.FindFirstObjectByType<Map3_UIManager>();
            if (umInstance != null)
            {
                SerializedObject soUmInst = new SerializedObject(umInstance);
                soUmInst.FindProperty("btnCatHint").objectReferenceValue = btnCatHintGo.GetComponent<Button>();
                soUmInst.ApplyModifiedPropertiesWithoutUndo();
            }

        }



        public static void SetupHintFrameAndDialog(Canvas rootCanvas, TMP_FontAsset defaultFont, Sprite uiSprite, Sprite quizCardSprite, Sprite buttonSprite, Sprite catSprite, Sprite toastFrameSprite = null)

        {

            if (rootCanvas == null) return;



            Sprite activeToastSprite = toastFrameSprite != null ? toastFrameSprite : LoadSprite("Assets/UI/Toast_Frame.png");

            if (activeToastSprite == null) activeToastSprite = quizCardSprite;



            // 1. Reconfigure Panel_Toast to use compact Toast Frame

            Transform existingToast = rootCanvas.transform.Find("Panel_Toast");

            GameObject toastPanelGo;

            if (existingToast != null)

            {

                toastPanelGo = existingToast.gameObject;

            }

            else

            {

                toastPanelGo = new GameObject("Panel_Toast");

                toastPanelGo.transform.SetParent(rootCanvas.transform, false);

            }



            var toastRt = toastPanelGo.GetComponent<RectTransform>();

            if (toastRt == null) toastRt = toastPanelGo.AddComponent<RectTransform>();

            toastRt.anchorMin = new Vector2(0.5f, 1f);

            toastRt.anchorMax = new Vector2(0.5f, 1f);

            toastRt.pivot = new Vector2(0.5f, 1f);

            toastRt.anchoredPosition = new Vector2(0f, -105f);

            toastRt.sizeDelta = new Vector2(860f, 142f);



            var toastImg = toastPanelGo.GetComponent<Image>();

            if (toastImg == null) toastImg = toastPanelGo.AddComponent<Image>();

            toastImg.sprite = activeToastSprite != null ? activeToastSprite : uiSprite;

            toastImg.type = activeToastSprite != null ? Image.Type.Sliced : Image.Type.Simple;

            toastImg.color = Color.white;

            toastImg.raycastTarget = false;



            // Toast Title (Header bar of Toast frame)

            Transform existingToastTitle = toastPanelGo.transform.Find("Txt_Toast_Title");

            GameObject txtToastTitleGo;

            if (existingToastTitle != null)

            {

                txtToastTitleGo = existingToastTitle.gameObject;

            }

            else

            {

                txtToastTitleGo = new GameObject("Txt_Toast_Title");

                txtToastTitleGo.transform.SetParent(toastPanelGo.transform, false);

            }

            var titleRt = txtToastTitleGo.GetComponent<RectTransform>();

            if (titleRt == null) titleRt = txtToastTitleGo.AddComponent<RectTransform>();

            titleRt.anchorMin = new Vector2(0.5f, 1f);

            titleRt.anchorMax = new Vector2(0.5f, 1f);

            titleRt.pivot = new Vector2(0.5f, 0.5f);

            titleRt.anchoredPosition = new Vector2(0f, -18f);

            titleRt.sizeDelta = new Vector2(700f, 26f);



            var titleTmp = txtToastTitleGo.GetComponent<TextMeshProUGUI>();

            if (titleTmp == null) titleTmp = txtToastTitleGo.AddComponent<TextMeshProUGUI>();

            if (defaultFont != null) titleTmp.font = defaultFont;

            titleTmp.text = "THÔNG BÁO";

            titleTmp.fontSize = 20f;

            titleTmp.fontStyle = FontStyles.Bold;

            titleTmp.color = HexColor("#FFD54F");

            titleTmp.alignment = TextAlignmentOptions.Center;



            // Toast Content (Yellow parchment area)

            Transform existingToastTxt = toastPanelGo.transform.Find("Txt_Toast");

            GameObject txtToastGo;

            if (existingToastTxt != null)

            {

                txtToastGo = existingToastTxt.gameObject;

            }

            else

            {

                txtToastGo = new GameObject("Txt_Toast");

                txtToastGo.transform.SetParent(toastPanelGo.transform, false);

            }

            var contentRt = txtToastGo.GetComponent<RectTransform>();

            if (contentRt == null) contentRt = txtToastGo.AddComponent<RectTransform>();

            contentRt.anchorMin = Vector2.zero;

            contentRt.anchorMax = Vector2.one;

            contentRt.pivot = new Vector2(0.5f, 0.5f);

            // Sliced borders: left=45, top=36, right=45, bottom=28.

            // Inner yellow box padding: left 60, bottom 26, right -60, top -38 (width 740, height 78)

            contentRt.offsetMin = new Vector2(60f, 26f);

            contentRt.offsetMax = new Vector2(-60f, -38f);



            var contentTmp = txtToastGo.GetComponent<TextMeshProUGUI>();

            if (contentTmp == null) contentTmp = txtToastGo.AddComponent<TextMeshProUGUI>();

            if (defaultFont != null) contentTmp.font = defaultFont;

            contentTmp.fontSize = 20f;

            contentTmp.enableAutoSizing = false;

            contentTmp.lineSpacing = 4f;

            contentTmp.paragraphSpacing = 0f;

            contentTmp.fontStyle = FontStyles.Bold;

            contentTmp.color = HexColor("#372016");

            contentTmp.alignment = TextAlignmentOptions.Center;

            contentTmp.textWrappingMode = TextWrappingModes.Normal;

            contentTmp.overflowMode = TextOverflowModes.Overflow;



            EditorUtility.SetDirty(toastPanelGo);

            EditorUtility.SetDirty(toastRt);

            EditorUtility.SetDirty(toastImg);

            EditorUtility.SetDirty(txtToastTitleGo);

            EditorUtility.SetDirty(titleRt);

            EditorUtility.SetDirty(titleTmp);

            EditorUtility.SetDirty(txtToastGo);

            EditorUtility.SetDirty(contentRt);

            EditorUtility.SetDirty(contentTmp);



            toastPanelGo.SetActive(false);



            // 2. Setup Panel_Hint_Dialog (Dialogue Modal with Quiz Frame)

            Transform existingHint = rootCanvas.transform.Find("Panel_Hint_Dialog");

            if (existingHint != null) Object.DestroyImmediate(existingHint.gameObject);



            GameObject hintModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Hint_Dialog",

                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

                Vector2.zero, Vector2.zero,

                uiSprite, new Color(0f, 0f, 0f, 0.75f));



            GameObject hintCardGo = CreateUIPanel(hintModalGo.transform, "Card_Hint",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(880f, 480f),

                quizCardSprite != null ? quizCardSprite : uiSprite, Color.white);

            var hintCardImg = hintCardGo.GetComponent<Image>();

            if (hintCardImg != null && quizCardSprite != null)

            {

                hintCardImg.type = Image.Type.Sliced;

            }



            GameObject btnCloseHintGo = CreateUIButton(hintCardGo.transform, "Btn_Close",

                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(-35f, -35f), new Vector2(44f, 44f),

                uiSprite, HexColor("#D32F2F"),

                "X", 24, FontStyles.Bold, Color.white, defaultFont);



            GameObject txtHintTitleGo = CreateUIText(hintCardGo.transform, "Txt_Title",

                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -48f), new Vector2(700f, 45f),

                "GỢI Ý TỪ CHÚ MÈO", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



            // Cat Avatar on the left

            GameObject catAvatarBorderGo = CreateUIPanel(hintCardGo.transform, "Border_CatAvatar",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(-270f, 15f), new Vector2(130f, 130f),

                buttonSprite != null ? buttonSprite : uiSprite, HexColor("#4E342E"));



            GameObject catAvatarGo = CreateUIPanel(catAvatarBorderGo.transform, "Img_CatAvatar",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                Vector2.zero, new Vector2(100f, 100f),

                catSprite, Color.white);

            var catAvatarImg = catAvatarGo.GetComponent<Image>();

            if (catAvatarImg != null) catAvatarImg.preserveAspect = true;



            // Content message on right

            GameObject txtHintMessageGo = CreateUIText(hintCardGo.transform, "Txt_Message",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(85f, 15f), new Vector2(530f, 220f),

                Map3_CatHint.CatHintMessage, 21f, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.MidlineLeft, defaultFont);

            var msgTmp = txtHintMessageGo.GetComponent<TextMeshProUGUI>();

            if (msgTmp != null)

            {

                msgTmp.textWrappingMode = TextWrappingModes.Normal;

                msgTmp.lineSpacing = 6f;

            }



            GameObject btnConfirmHintGo = CreateUIButton(hintCardGo.transform, "Btn_Confirm",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 52f), new Vector2(250f, 52f),

                buttonSprite != null ? buttonSprite : uiSprite, Color.white,

                "ĐÃ HIỂU", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



            hintModalGo.SetActive(false);



            // 3. Wire to Map3_UIManager

            var um = Object.FindFirstObjectByType<Map3_UIManager>();

            if (um != null)

            {

                SerializedObject soUm = new SerializedObject(um);

                soUm.FindProperty("toastPanel").objectReferenceValue = toastPanelGo;

                soUm.FindProperty("txtToast").objectReferenceValue = contentTmp;

                soUm.FindProperty("txtToastTitle").objectReferenceValue = titleTmp;

                soUm.FindProperty("hintDialogPanel").objectReferenceValue = hintModalGo;

                soUm.FindProperty("txtHintTitle").objectReferenceValue = txtHintTitleGo.GetComponent<TextMeshProUGUI>();

                soUm.FindProperty("txtHintMessage").objectReferenceValue = msgTmp;

                soUm.FindProperty("btnHintConfirm").objectReferenceValue = btnConfirmHintGo.GetComponent<Button>();

                soUm.FindProperty("btnHintClose").objectReferenceValue = btnCloseHintGo.GetComponent<Button>();
            Transform catHintBtnFound = rootCanvas.transform.Find("Btn_CatHint");
            if (catHintBtnFound != null)
            {
                soUm.FindProperty("btnCatHint").objectReferenceValue = catHintBtnFound.GetComponent<Button>();
            }

                soUm.ApplyModifiedPropertiesWithoutUndo();

            }

        }



        private static GameObject CreateIconButton(Transform parent, string name, Sprite bgSprite, Sprite iconSprite, float size)

        {

            GameObject btnGo = new GameObject(name);

            btnGo.transform.SetParent(parent, false);

            var rt = btnGo.AddComponent<RectTransform>();

            rt.sizeDelta = new Vector2(size, size);



            var img = btnGo.AddComponent<Image>();

            img.sprite = bgSprite;

            img.color = Color.white;

            img.type = Image.Type.Sliced;



            var btn = btnGo.AddComponent<Button>();

            var colors = btn.colors;

            colors.normalColor = Color.white;

            colors.highlightedColor = new Color(1f, 1f, 0.85f, 1f);

            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

            btn.colors = colors;



            GameObject iconGo = new GameObject("Icon");

            iconGo.transform.SetParent(btnGo.transform, false);

            var iconRt = iconGo.AddComponent<RectTransform>();

            iconRt.anchorMin = new Vector2(0.5f, 0.5f);

            iconRt.anchorMax = new Vector2(0.5f, 0.5f);

            iconRt.pivot = new Vector2(0.5f, 0.5f);

            iconRt.anchoredPosition = Vector2.zero;

            iconRt.sizeDelta = new Vector2(size * 0.62f, size * 0.62f);



            var iconImg = iconGo.AddComponent<Image>();

            iconImg.sprite = iconSprite;

            iconImg.preserveAspect = true;

            iconImg.color = Color.white;

            iconImg.raycastTarget = false;



            return btnGo;

        }



        private static Sprite LoadSpriteSubAsset(string path)

        {

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var a in assets)

            {

                if (a is Sprite s) return s;

            }

            return null;

        }



        private static Color HexColor(string hex)

        {

            Color c;

            ColorUtility.TryParseHtmlString(hex, out c);

            return c;

        }



    }

}

