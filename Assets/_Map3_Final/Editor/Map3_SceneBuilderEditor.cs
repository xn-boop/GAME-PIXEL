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
    public static class Map3_SceneBuilderEditor
    {
        private const string SceneDirectory = "Assets/_Map3_Final/Scenes";
        private const string ScenePath = "Assets/_Map3_Final/Scenes/Map3_Final.unity";

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

            // 5. Toast Notification Panel
            GameObject toastPanelGo = CreateUIPanel(rootCanvas.transform, "Panel_Toast",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -120f), new Vector2(780f, 90f),
                uiSprite, new Color(0.08f, 0.1f, 0.15f, 0.95f));

            GameObject txtToastGo = CreateUIText(toastPanelGo.transform, "Txt_Toast",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(25f, 15f), new Vector2(-50f, -30f),
                "Thông báo trò chơi...", 20, FontStyles.Normal, Color.white, TextAlignmentOptions.Center, defaultFont);
            toastPanelGo.SetActive(false);

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
                Vector2.zero, new Vector2(880f, 650f),
                quizPanelSprite != null ? quizPanelSprite : uiSprite,
                quizPanelSprite != null ? Color.white : new Color(0.12f, 0.14f, 0.18f, 1f));

            // Close button
            GameObject btnCloseGo = CreateUIButton(quizCardGo.transform, "Btn_Close",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-35f, -35f), new Vector2(44f, 44f),
                uiSprite, new Color(0.85f, 0.2f, 0.2f, 1f),
                "X", 24, FontStyles.Bold, Color.white, defaultFont);

            // Title
            GameObject txtTitleGo = CreateUIText(quizCardGo.transform, "Txt_Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -45f), new Vector2(760f, 50f),
                "CÂU HỎI VĂN HÓA", 28, FontStyles.Bold, new Color(1f, 0.8f, 0.2f, 1f), TextAlignmentOptions.Center, defaultFont);

            // Question
            GameObject txtQuestContentGo = CreateUIText(quizCardGo.transform, "Txt_Question",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(800f, 130f),
                "Nội dung câu hỏi văn hóa...", 20, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, defaultFont);
            var txtQuestionTmp = txtQuestContentGo.GetComponent<TextMeshProUGUI>();
            txtQuestionTmp.textWrappingMode = TextWrappingModes.Normal;

            // 4 Answer Buttons
            Button[] ansBtns = new Button[4];
            TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];
            string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };
            float startBtnY = -245f;
            float btnStepY = 60f;

            for (int i = 0; i < 4; i++)
            {
                GameObject aBtnGo = CreateUIButton(quizCardGo.transform, btnNames[i],
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, startBtnY - i * btnStepY), new Vector2(800f, 52f),
                    buttonSprite != null ? buttonSprite : uiSprite,
                    new Color32(78, 52, 46, 255), // #4E342E
                    $"Phương án {i + 1}", 17, FontStyles.Normal, Color.white, defaultFont);

                ansBtns[i] = aBtnGo.GetComponent<Button>();
                ansTexts[i] = aBtnGo.GetComponentInChildren<TextMeshProUGUI>();
                ansTexts[i].alignment = TextAlignmentOptions.MidlineLeft;
                ansTexts[i].margin = new Vector4(20f, 0f, 20f, 0f);
                ansTexts[i].textWrappingMode = TextWrappingModes.Normal;
            }

            // Explanation Text
            GameObject txtExplanationGo = CreateUIText(quizCardGo.transform, "Txt_Explanation",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 85f), new Vector2(800f, 75f),
                "", 17, FontStyles.Normal, new Color(0.9f, 0.95f, 1f, 1f), TextAlignmentOptions.TopLeft, defaultFont);
            var txtExplanationTmp = txtExplanationGo.GetComponent<TextMeshProUGUI>();
            txtExplanationTmp.textWrappingMode = TextWrappingModes.Normal;
            txtExplanationGo.SetActive(false);

            // Feedback Text
            GameObject txtFeedGo = CreateUIText(quizCardGo.transform, "Txt_Feedback",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 140f), new Vector2(800f, 36f),
                "", 21, FontStyles.Bold, new Color(0.2f, 0.9f, 0.4f, 1f), TextAlignmentOptions.Center, defaultFont);

            // Continue Button (shown after correct answer to proceed)
            GameObject btnContinueGo = CreateUIButton(quizCardGo.transform, "Btn_Continue",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 35f), new Vector2(220f, 48f),
                uiSprite, new Color(0.18f, 0.7f, 0.35f, 1f),
                "Tiếp tục", 20, FontStyles.Bold, Color.white, defaultFont);
            btnContinueGo.SetActive(false);

            quizModalGo.SetActive(false);

            // 8. Panel_Victory
            GameObject victoryModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Victory",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                uiSprite, new Color(0f, 0f, 0f, 0.88f));

            GameObject victoryCardGo = CreateUIPanel(victoryModalGo.transform, "Card_Victory",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(820f, 480f),
                uiSprite, new Color(0.1f, 0.14f, 0.22f, 1f));

            CreateUIText(victoryCardGo.transform, "Txt_Victory_Header",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -50f), new Vector2(740f, 50f),
                "XUẤT SẮC!", 34, FontStyles.Bold, new Color(1f, 0.84f, 0.31f, 1f), TextAlignmentOptions.Center, defaultFont);

            GameObject txtVicTitleGo = CreateUIText(victoryCardGo.transform, "Txt_Victory_Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -125f), new Vector2(740f, 70f),
                "HOÀN THÀNH MÀN CUỐI: KHU VƯỜN GIAO THOA!", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);

            GameObject txtVicDescGo = CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -220f), new Vector2(720f, 90f),
                "Chúc mừng bạn đã hợp nhất thành công hai nửa Ngọc Bội Việt - Hàn! Ký ức gia đình và tình hữu nghị hai dân tộc đã được gắn kết trọn vẹn!", 20, FontStyles.Normal, new Color(0.85f, 0.9f, 0.95f, 1f), TextAlignmentOptions.Center, defaultFont);

            GameObject btnRestartGo = CreateUIButton(victoryCardGo.transform, "Btn_Restart",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 65f), new Vector2(240f, 55f),
                uiSprite, new Color(0.18f, 0.7f, 0.35f, 1f),
                "Chơi lại", 22, FontStyles.Bold, Color.white, defaultFont);

            victoryModalGo.SetActive(false);

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
    }
}
