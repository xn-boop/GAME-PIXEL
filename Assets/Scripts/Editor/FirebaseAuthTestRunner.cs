using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.UI;



using System.IO;



using System.Reflection;



using System.Text;



using UnityEditor;



using UnityEditor.Compilation;



using UnityEditor.TestTools.TestRunner.Api;



using UnityEngine;







namespace JourneyOfMemories.Editor



{



    [InitializeOnLoad]



    public static class FirebaseAuthTestRunner



    {



        private const string TriggerFlagFile = "Temp/RunFirebaseAuthTests.flag";



        private const string DumpFlagFile = "Temp/DumpConsole.flag";



        private const string ClearFlagFile = "Temp/ClearConsole.flag";



        private const string ExitPlayFlagFile = "Temp/ExitPlayMode.flag";



        private const string PendingSessionKey = "FirebaseAuthTestRunner_PendingRun";



        private static readonly string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "FirebaseAuthTestResults.log");



        private static readonly string ConsoleLogPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "ConsoleErrors.log");



        private static TestRunnerApi activeApi;



        private static TestCallbacks activeCallbacks;







        static FirebaseAuthTestRunner()



        {



            EditorApplication.update -= CheckForTrigger;



            EditorApplication.update += CheckForTrigger;



        }







        private static void CheckForTrigger()



        {



            if (File.Exists(ExitPlayFlagFile))



            {



                try { File.Delete(ExitPlayFlagFile); } catch { }



                EditorApplication.ExitPlaymode();



                EditorApplication.isPlaying = false;



                Debug.Log("[FirebaseAuthTestRunner] ExitPlaymode executed!");



            }







            if (File.Exists(ClearFlagFile))



            {



                try { File.Delete(ClearFlagFile); } catch { }



                ClearConsole();



            }







            if (File.Exists(DumpFlagFile))



            {



                try { File.Delete(DumpFlagFile); } catch { }



                DumpConsoleEntries();



            }







            if (File.Exists("Temp/RevertL1LivingRoomFood.flag"))



            {



                try { File.Delete("Temp/RevertL1LivingRoomFood.flag"); } catch { }



                L1_SceneSetup.RevertLivingRoomFoodAndRestoreKitchenCulturalItems();



            }







            if (File.Exists("Temp/SetupL3HUD.flag"))



            {



                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)



                {



                    EditorApplication.ExitPlaymode();



                    EditorApplication.isPlaying = false;



                    return;



                }



                try { File.Delete("Temp/SetupL3HUD.flag"); } catch { }



                Game.Map3.Editor.Map3_SceneBuilderEditor.SetupTopRightHUDInScene();



            }







            if (File.Exists("Temp/SetupL2HUD.flag"))



            {



                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)



                {



                    EditorApplication.ExitPlaymode();



                    EditorApplication.isPlaying = false;



                    return;



                }



                try { File.Delete("Temp/SetupL2HUD.flag"); } catch { }



                L2_SceneSetup.SetupTopRightHUDInScene();



            }







            if (File.Exists("Temp/SetupL1HUD.flag"))



            {



                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)



                {



                    EditorApplication.ExitPlaymode();



                    EditorApplication.isPlaying = false;



                    return;



                }



                try { File.Delete("Temp/SetupL1HUD.flag"); } catch { }



                L1_SceneSetup.SetupTopRightHUDInScene();



            }







            if (File.Exists("Temp/UpdateVictoryUI.flag"))



            {



                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)



                {



                    EditorApplication.ExitPlaymode();



                    EditorApplication.isPlaying = false;



                    return;



                }



                try { File.Delete("Temp/UpdateVictoryUI.flag"); } catch { }



                Game.UI.Editor.VictoryUIUpdater.UpdateAllScenes();



            }







                        if (File.Exists("Temp/FixRegisterMenuAssets.flag"))
            {
                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorApplication.ExitPlaymode();
                    EditorApplication.isPlaying = false;
                    return;
                }
                try { File.Delete("Temp/FixRegisterMenuAssets.flag"); } catch { }
                FixRegisterMenuAssets();
            }


            if (File.Exists(TriggerFlagFile))



            {



                try



                {



                    File.Delete(TriggerFlagFile);



                }



                catch { }







                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)



                {



                    EditorApplication.isPlaying = false;



                }







                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/Editor/L1_SceneSetup.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/L1_GameManager.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/L1_ItemPickup.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/L1_QuizManager.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/L1_InventoryManager.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/Editor/L1_SceneSetup.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/Editor/L1_CoreTests.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level1_Core/L1_PlayerController.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level2_Core/L2_PlayerController.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/Scripts/Level2_Core/Editor/L2_CoreTests.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/_Map3_Final/Scripts/Map3_UIManager.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/_Map3_Final/Scripts/Map3_CatHint.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/_Map3_Final/Scripts/Map3_GameHUDController.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/_Map3_Final/Editor/Map3_SceneBuilderEditor.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.ImportAsset("Assets/_Map3_Final/Editor/Map3_Tests.cs", ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset("Assets/Scripts/Editor/Tests/RegisterMenuTests.cs", ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset("Assets/Scripts/Editor/FirebaseAuthTestRunner.cs", ImportAssetOptions.ForceUpdate);



                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);



                EditorUtility.RequestScriptReload();



                CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);



                SessionState.SetBool(PendingSessionKey, true);



                SessionState.SetFloat("FirebaseAuthTestRunner_TriggerTime", (float)EditorApplication.timeSinceStartup);



                return;



            }







            if (SessionState.GetBool(PendingSessionKey, false))



            {



                float triggerTime = SessionState.GetFloat("FirebaseAuthTestRunner_TriggerTime", 0f);



                if (EditorApplication.isCompiling ||



                    EditorApplication.isUpdating ||



                    EditorApplication.isPlaying ||



                    EditorApplication.timeSinceStartup - triggerTime < 3.0f)



                {



                    return;



                }







                SessionState.SetBool(PendingSessionKey, false);



                RunAllTests();



            }



        }







        [MenuItem("Tools/Run Firebase Auth Tests")]



        public static void RunAllTests()



        {



            try



            {



                string logDir = Path.GetDirectoryName(LogFilePath);



                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);







                File.WriteAllText(LogFilePath, $"=== Firebase Auth Test Run Initializing at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n", Encoding.UTF8);







                Debug.Log("[FirebaseAuthTestRunner] Bắt đầu chạy bộ kiểm thử Firebase Authentication và Account Save...");







                activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();



                activeCallbacks = new TestCallbacks();



                activeApi.RegisterCallbacks(activeCallbacks);







                Filter filter = new Filter



                {



                    testMode = TestMode.EditMode



                };







                activeApi.Execute(new ExecutionSettings(filter));



            }



            catch (Exception ex)



            {



                File.AppendAllText(LogFilePath, $"ERROR launching tests: {ex}\n", Encoding.UTF8);



                Debug.LogError("[FirebaseAuthTestRunner] Lỗi khi chạy tests: " + ex);



            }



        }







        [MenuItem("Tools/Clear Console")]



        public static void ClearConsole()



        {



            try



            {



                Type logEntriesType = null;



                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())



                {



                    logEntriesType = asm.GetType("UnityEditor.LogEntries");



                    if (logEntriesType != null) break;



                }



                var clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);



                clearMethod?.Invoke(null, null);



                Debug.Log("[FirebaseAuthTestRunner] Console cleared.");



            }



            catch (Exception ex)



            {



                Debug.LogWarning("[FirebaseAuthTestRunner] Failed to clear console: " + ex.Message);



            }



        }







        public static void DumpConsoleEntries()



        {



            try



            {



                Type logEntriesType = null;



                Type logEntryType = null;



                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())



                {



                    if (logEntriesType == null) logEntriesType = asm.GetType("UnityEditor.LogEntries");



                    if (logEntryType == null) logEntryType = asm.GetType("UnityEditor.LogEntry");



                }







                if (logEntriesType == null || logEntryType == null)



                {



                    File.WriteAllText(ConsoleLogPath, "Could not find UnityEditor.LogEntries or LogEntry types.\n");



                    return;



                }







                var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);



                var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);



                var messageField = logEntryType.GetField("message", BindingFlags.Public | BindingFlags.Instance);



                var modeField = logEntryType.GetField("mode", BindingFlags.Public | BindingFlags.Instance);



                var fileField = logEntryType.GetField("file", BindingFlags.Public | BindingFlags.Instance);



                var lineField = logEntryType.GetField("line", BindingFlags.Public | BindingFlags.Instance);







                int count = (int)(getCountMethod?.Invoke(null, null) ?? 0);



                StringBuilder sb = new StringBuilder();



                sb.AppendLine($"=== Console Entries Dump at {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Total: {count}) ===");







                object logEntryInstance = Activator.CreateInstance(logEntryType);







                for (int i = 0; i < count; i++)



                {



                    getEntryMethod?.Invoke(null, new object[] { i, logEntryInstance });



                    string msg = messageField?.GetValue(logEntryInstance) as string;



                    int mode = (int)(modeField?.GetValue(logEntryInstance) ?? 0);



                    string file = fileField?.GetValue(logEntryInstance) as string;



                    int line = (int)(lineField?.GetValue(logEntryInstance) ?? 0);







                    bool isError = (mode & (1 | 2 | 8 | 16 | 32 | 256)) != 0;



                    sb.AppendLine($"[{i}] [Mode: {mode}{(isError ? " ERROR" : "")}] {file}:{line}\n    {msg}");



                }







                File.WriteAllText(ConsoleLogPath, sb.ToString(), Encoding.UTF8);



            }



            catch (Exception ex)



            {



                File.WriteAllText(ConsoleLogPath, "Error dumping console entries: " + ex, Encoding.UTF8);



            }



        }







        private class TestCallbacks : ICallbacks



        {



            private StringBuilder sb = new StringBuilder();







            public void RunStarted(ITestAdaptor testsToRun)



            {



                sb.Clear();



                sb.AppendLine($"=== Firebase Auth Test Run Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");



                sb.AppendLine($"Total tests to run: {testsToRun?.TestCaseCount ?? 0}");



                sb.AppendLine("--------------------------------------------------------------------------------");



                File.WriteAllText(LogFilePath, sb.ToString(), Encoding.UTF8);



            }







            public void TestStarted(ITestAdaptor test) { }







            public void TestFinished(ITestResultAdaptor result)



            {



                if (!result.HasChildren)



                {



                    string icon = result.TestStatus == TestStatus.Passed ? "[PASS]" : "[FAIL]";



                    string line = $"{icon} {result.Test.FullName} ({result.Duration:F3}s)";







                    if (!string.IsNullOrEmpty(result.Message))



                    {



                        line += $"\n       Message: {result.Message.Trim()}";



                    }



                    if (!string.IsNullOrEmpty(result.StackTrace))



                    {



                        line += $"\n       StackTrace: {result.StackTrace.Trim()}";



                    }



                    File.AppendAllText(LogFilePath, line + "\n", Encoding.UTF8);



                }



            }







            public void RunFinished(ITestResultAdaptor result)



            {



                sb.Clear();



                sb.AppendLine();



                sb.AppendLine("================================================================================");



                sb.AppendLine($"TEST RUN FINISHED: Pass = {result.PassCount}, Fail = {result.FailCount}, Skip = {result.SkipCount}");



                sb.AppendLine($"Duration: {result.Duration:F2}s");



                sb.AppendLine("================================================================================");



                File.AppendAllText(LogFilePath, sb.ToString(), Encoding.UTF8);



                if (result.FailCount == 0)

                {

                    EditorApplication.delayCall += () =>

                    {

                        ClearConsole();

                        Debug.Log($"<color=#4CAF50><b>[FirebaseAuthTestRunner] K\u1ebft qu\u1ea3: {result.PassCount} PASS, 0 FAIL. Console s\u1ea1ch ho\u00e0n to\u00e0n.</b></color>");

                    };

                }

                else

                {

                    Debug.Log($"[FirebaseAuthTestRunner] Ho\u00e0n th\u00e0nh: Pass = {result.PassCount}, Fail = {result.FailCount}, Skip = {result.SkipCount}");

                }



            }



        }



    
        public static void FixRegisterMenuAssets()
        {
            try
            {
                string scenePath = "Assets/Scenes/RegisterMenu.unity";
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var frames = new List<Sprite>();
                for (int i = 0; i <= 30; i++)
                {
                    string path = $"Assets/UI/RegisterMenuFrames/frame_{i:02d}.png";
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sp == null)
                    {
                        var all = AssetDatabase.LoadAllAssetsAtPath(path);
                        foreach (var o in all)
                        {
                            if (o is Sprite s) { sp = s; break; }
                        }
                    }
                    if (sp != null)
                    {
                        frames.Add(sp);
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sp, out string g, out long id);
                        Debug.Log($"[FixRegister] Loaded frame_{i:02d}: {sp.name}, GUID={g}, ID={id}");
                    }
                    else
                    {
                        Debug.LogError($"[FixRegister] FAILED loading frame_{i:02d} at {path}");
                    }
                }

                var bgObj = GameObject.Find("BackgroundImage");
                if (bgObj != null)
                {
                    var bgImg = bgObj.GetComponent<Image>();
                    if (bgImg != null && frames.Count > 0)
                    {
                        bgImg.sprite = frames[0];
                        bgImg.color = Color.white;
                        EditorUtility.SetDirty(bgImg);
                    }
                    var anim = bgObj.GetComponent<UIFrameAnimator>();
                    if (anim != null && frames.Count > 0)
                    {
                        anim.frames = frames.ToArray();
                        anim.framesPerSecond = 10f;
                        anim.loop = true;
                        anim.unscaledTime = true;
                        EditorUtility.SetDirty(anim);
                    }
                    EditorUtility.SetDirty(bgObj);
                }

                var checkObj = GameObject.Find("Checkmark");
                if (checkObj != null)
                {
                    var cImg = checkObj.GetComponent<Image>();
                    if (cImg != null)
                    {
                        string cPath = "Assets/UI/checkbox_checked.png";
                        Sprite cSp = AssetDatabase.LoadAssetAtPath<Sprite>(cPath);
                        if (cSp == null)
                        {
                            var all = AssetDatabase.LoadAllAssetsAtPath(cPath);
                            foreach (var o in all)
                            {
                                if (o is Sprite s) { cSp = s; break; }
                            }
                        }
                        if (cSp != null)
                        {
                            cImg.sprite = cSp;
                            cImg.color = Color.white;
                            EditorUtility.SetDirty(cImg);
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(cSp, out string g, out long id);
                            Debug.Log($"[FixRegister] Loaded checkmark: {cSp.name}, GUID={g}, ID={id}");
                        }
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[FixRegister] Successfully saved {scenePath} with {frames.Count} animation frames!");

                RegisterMenuVerifier.RunVerification();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FixRegister] Exception fixing register menu: {ex}");
            }
        }


}



}



