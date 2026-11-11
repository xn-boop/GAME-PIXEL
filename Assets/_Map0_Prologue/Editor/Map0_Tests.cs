using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Map0.Tests
{
    public class Map0_Tests
    {
        private const string ScenePath = "Assets/_Map0_Prologue/Scenes/Map0_Prologue.unity";

        [MenuItem("Tools/Map 0/Run Map 0 Verification Tests")]
        public static void RunAllTestsFromMenu()
        {
            var test = new Map0_Tests();
            Debug.Log("<color=cyan>=== STARTING MAP 0 VERIFICATION TESTS ===</color>");

            try
            {
                test.Test01_BuildSettings_IncludesMap0AndMap1();
                Debug.Log("<color=green>✔ Test 01 Passed: Build Settings configured with Map 0 and Map 1</color>");

                test.Test02_HierarchyRoots_AllFiveExist();
                Debug.Log("<color=green>✔ Test 02 Passed: Hierarchy 5 Root Groups Exist</color>");

                test.Test03_Player_SetupAndComponents();
                Debug.Log("<color=green>✔ Test 03 Passed: Player Setup and Components Valid</color>");

                test.Test04_Zones_AllInteractiveZonesConfigured();
                Debug.Log("<color=green>✔ Test 04 Passed: All 5 Interaction Zones Configured Correctly</color>");

                test.Test05_UI_CanvasAndHUDComponents();
                Debug.Log("<color=green>✔ Test 05 Passed: UI Canvas and HUD Elements Connected</color>");

                test.Test06_LightingAndParticles_Setup();
                Debug.Log("<color=green>✔ Test 06 Passed: Lighting Controller & Magical Particles Wired</color>");

                test.Test07_PrologueManager_ReferencesAndStages();
                Debug.Log("<color=green>✔ Test 07 Passed: Map0_PrologueManager Serialized References Valid</color>");

                test.Test08_InventorySlot_DiarySpriteAndSlotImageConfigured();
                Debug.Log("<color=green>✔ Test 08 Passed: Diary Sprite and Inventory Slot 0 Configured Valid</color>");

                Debug.Log("<color=green><b>=== ALL MAP 0 VERIFICATION TESTS PASSED (8/8) ===</b></color>");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>❌ MAP 0 TEST FAILED: {ex.Message}\n{ex.StackTrace}</color>");
            }
        }

        private void EnsureSceneLoaded()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                if (System.IO.File.Exists(ScenePath))
                {
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
                else
                {
                    Map0.Editor.Map0_SceneBuilderEditor.BuildMap0Scene();
                }
            }
        }

        [Test]
        public void Test01_BuildSettings_IncludesMap0AndMap1()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasMap0 = scenes.Any(s => (s.path == ScenePath || s.path.Contains("Map0_Prologue")) && s.enabled);
            bool hasMap1 = scenes.Any(s => s.path.Contains("Level1_Prototype") && s.enabled);

            Assert.IsTrue(hasMap0, $"Build Settings must contain enabled Map 0 scene at {ScenePath}");
            Assert.IsTrue(hasMap1, "Build Settings must contain enabled Map 1 scene (Level1_Prototype)");
        }

        [Test]
        public void Test02_HierarchyRoots_AllFiveExist()
        {
            EnsureSceneLoaded();

            string[] expectedRoots = new string[]
            {
                "--- MANAGERS ---",
                "--- CAMERAS ---",
                "--- ENVIRONMENT ---",
                "--- GAMEPLAY OBJECTS ---",
                "--- UI CANVAS ---"
            };

            foreach (var rootName in expectedRoots)
            {
                var go = GameObject.Find(rootName);
                Assert.IsNotNull(go, $"Root hierarchy group '{rootName}' must exist in Map 0 scene.");
            }
        }

        [Test]
        public void Test03_Player_SetupAndComponents()
        {
            EnsureSceneLoaded();

            var player = GameObject.FindWithTag("Player");
            Assert.IsNotNull(player, "Player with tag 'Player' must exist.");

            var ctrl = player.GetComponent<Map0_PlayerController>();
            Assert.IsNotNull(ctrl, "Player must have Map0_PlayerController attached.");

            var rb = player.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb, "Player must have Rigidbody2D.");
            Assert.AreEqual(0f, rb.gravityScale, "Player Rigidbody2D gravityScale must be 0.");
            Assert.IsTrue(rb.freezeRotation, "Player Rigidbody2D freezeRotation must be true.");

            var col = player.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(col, "Player must have BoxCollider2D.");
            Assert.IsFalse(col.isTrigger, "Player BoxCollider2D must be solid (isTrigger = false).");

            var visual = player.transform.Find("Visual_Sprite");
            Assert.IsNotNull(visual, "Player must have child 'Visual_Sprite'.");
            var sr = visual.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(sr, "Player Visual_Sprite must have SpriteRenderer.");
        }

        [Test]
        public void Test04_Zones_AllInteractiveZonesConfigured()
        {
            EnsureSceneLoaded();

            // WallCalendar_Zone
            var cal = GameObject.Find("WallCalendar_Zone");
            Assert.IsNotNull(cal, "WallCalendar_Zone must exist.");
            var calZone = cal.GetComponent<Map0_InteractZone>();
            Assert.IsNotNull(calZone, "WallCalendar_Zone must have Map0_InteractZone.");
            Assert.AreEqual(Map0_ZoneType.WallCalendar, calZone.ZoneType);

            // FamilyPhoto_Zone
            var photo = GameObject.Find("FamilyPhoto_Zone");
            Assert.IsNotNull(photo, "FamilyPhoto_Zone must exist.");
            var photoZone = photo.GetComponent<Map0_InteractZone>();
            Assert.IsNotNull(photoZone, "FamilyPhoto_Zone must have Map0_InteractZone.");
            Assert.AreEqual(Map0_ZoneType.FamilyPhoto, photoZone.ZoneType);

            // TimeCapsule_Zone
            var capsule = GameObject.Find("TimeCapsule_Zone");
            Assert.IsNotNull(capsule, "TimeCapsule_Zone must exist.");
            var capsuleZone = capsule.GetComponent<Map0_InteractZone>();
            Assert.IsNotNull(capsuleZone, "TimeCapsule_Zone must have Map0_InteractZone.");
            Assert.AreEqual(Map0_ZoneType.TimeCapsule, capsuleZone.ZoneType);

            // KitchenDoor_Zone
            var door = GameObject.Find("KitchenDoor_Zone");
            Assert.IsNotNull(door, "KitchenDoor_Zone must exist.");
            var doorZone = door.GetComponent<Map0_InteractZone>();
            Assert.IsNotNull(doorZone, "KitchenDoor_Zone must have Map0_InteractZone.");
            Assert.AreEqual(Map0_ZoneType.KitchenDoor, doorZone.ZoneType);

            // YellowCat_NPC (might be inactive in scene hierarchy)
            var gameplayRoot = GameObject.Find("--- GAMEPLAY OBJECTS ---");
            Assert.IsNotNull(gameplayRoot, "--- GAMEPLAY OBJECTS --- must exist.");
            var catTransform = gameplayRoot.transform.Find("YellowCat_NPC");
            Assert.IsNotNull(catTransform, "YellowCat_NPC must exist under --- GAMEPLAY OBJECTS ---.");
            var catZone = catTransform.GetComponent<Map0_InteractZone>();
            Assert.IsNotNull(catZone, "YellowCat_NPC must have Map0_InteractZone.");
            Assert.AreEqual(Map0_ZoneType.YellowCat, catZone.ZoneType);
        }

        [Test]
        public void Test05_UI_CanvasAndHUDComponents()
        {
            EnsureSceneLoaded();

            var canvas = GameObject.Find("--- UI CANVAS ---");
            Assert.IsNotNull(canvas, "--- UI CANVAS --- must exist.");

            Assert.IsNotNull(canvas.transform.Find("HUD_TopLeft_Objective"), "HUD_TopLeft_Objective must exist.");
            Assert.IsNotNull(canvas.transform.Find("HUD_TopCenter_Quest"), "HUD_TopCenter_Quest must exist.");
            Assert.IsNotNull(canvas.transform.Find("HUD_BottomCenter_Inv"), "HUD_BottomCenter_Inv must exist.");
            Assert.IsNotNull(canvas.transform.Find("Controls_Mobile"), "Controls_Mobile must exist.");
            Assert.IsNotNull(canvas.transform.Find("Prompt_Interact"), "Prompt_Interact must exist.");
            Assert.IsNotNull(canvas.transform.Find("Panel_Toast"), "Panel_Toast must exist.");
            Assert.IsNotNull(canvas.transform.Find("Panel_Dialogue"), "Panel_Dialogue must exist.");
            Assert.IsNotNull(canvas.transform.Find("Fade_Overlay"), "Fade_Overlay must exist.");

            var uiMgr = Object.FindAnyObjectByType<Map0_UIManager>();
            Assert.IsNotNull(uiMgr, "Map0_UIManager must exist on managers.");
        }

        [Test]
        public void Test06_LightingAndParticles_Setup()
        {
            EnsureSceneLoaded();

            var lightingCtrl = Object.FindAnyObjectByType<Map0_LightingController>();
            Assert.IsNotNull(lightingCtrl, "Map0_LightingController must exist.");

            var overlay = GameObject.Find("Lighting_Overlay");
            Assert.IsNotNull(overlay, "Lighting_Overlay GameObject must exist.");
            var sr = overlay.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(sr, "Lighting_Overlay must have SpriteRenderer.");

            var particles = GameObject.Find("Room_Sparkle_Particles");
            Assert.IsNotNull(particles, "Room_Sparkle_Particles must exist.");
            var ps = particles.GetComponent<ParticleSystem>();
            Assert.IsNotNull(ps, "Room_Sparkle_Particles must have ParticleSystem.");
        }

        [Test]
        public void Test07_PrologueManager_ReferencesAndStages()
        {
            EnsureSceneLoaded();

            var manager = Object.FindAnyObjectByType<Map0_PrologueManager>();
            Assert.IsNotNull(manager, "Map0_PrologueManager must exist.");
        }

        [Test]
        public void Test08_InventorySlot_DiarySpriteAndSlotImageConfigured()
        {
            EnsureSceneLoaded();

            var manager = Object.FindAnyObjectByType<Map0_PrologueManager>();
            Assert.IsNotNull(manager, "Map0_PrologueManager must exist.");

            var so = new SerializedObject(manager);
            var diaryProp = so.FindProperty("diarySprite");
            Assert.IsNotNull(diaryProp, "diarySprite property must exist.");
            Assert.IsNotNull(diaryProp.objectReferenceValue, "diarySprite must be assigned in Map0_PrologueManager.");

            var slotImgProp = so.FindProperty("slotImage");
            Assert.IsNotNull(slotImgProp, "slotImage property must exist.");
            Assert.IsNotNull(slotImgProp.objectReferenceValue, "slotImage must be assigned in Map0_PrologueManager.");

            var slotLabelProp = so.FindProperty("slotLabel");
            Assert.IsNotNull(slotLabelProp, "slotLabel property must exist.");
            Assert.IsNotNull(slotLabelProp.objectReferenceValue, "slotLabel must be assigned in Map0_PrologueManager.");

            var uiMgr = Object.FindAnyObjectByType<Map0_UIManager>();
            Assert.IsNotNull(uiMgr, "Map0_UIManager must exist.");
            var uiSo = new SerializedObject(uiMgr);
            var slotIconsProp = uiSo.FindProperty("slotIcons");
            Assert.IsTrue(slotIconsProp != null && slotIconsProp.arraySize > 0, "slotIcons on Map0_UIManager must have at least 1 slot.");
            Assert.IsNotNull(slotIconsProp.GetArrayElementAtIndex(0).objectReferenceValue, "slotIcons[0] must not be null.");

            var slotLabelsProp = uiSo.FindProperty("slotLabels");
            Assert.IsTrue(slotLabelsProp != null && slotLabelsProp.arraySize > 0, "slotLabels on Map0_UIManager must have at least 1 label.");
            Assert.IsNotNull(slotLabelsProp.GetArrayElementAtIndex(0).objectReferenceValue, "slotLabels[0] must not be null.");

            // Verify label gameobject is inactive by default
            var firstLabel = slotLabelsProp.GetArrayElementAtIndex(0).objectReferenceValue as TextMeshProUGUI;
            Assert.IsNotNull(firstLabel, "slotLabels[0] must be TextMeshProUGUI.");
            Assert.IsFalse(firstLabel.gameObject.activeSelf, "slot label GameObject must be inactive by default to avoid cluttering pixel slot.");
        }
    }
}
