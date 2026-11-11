using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class McpAutoConnectFix
    {
        private static double lastCheckTime = 0;

        static McpAutoConnectFix()
        {
            // Set CI=true so IvanMurzak.Unity.MCP EnvironmentUtils.IsCi() returns true
            // This prevents Startup.Editor.cs from auto-connecting on assembly reload and playmode state changes.
            try
            {
                Environment.SetEnvironmentVariable("CI", "true");
            }
            catch { }

            EditorApplication.delayCall += EnsureMcpAutoConnectDisabled;
            EditorApplication.delayCall += ClearConsole;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - lastCheckTime < 2.0)
            {
                return;
            }
            lastCheckTime = EditorApplication.timeSinceStartup;

            EnsureMcpAutoConnectDisabled();
        }

        [MenuItem("Tools/MCP/Disable MCP Auto Connect & Stop Errors")]
        public static void EnsureMcpAutoConnectDisabled()
        {
            try
            {
                Environment.SetEnvironmentVariable("CI", "true");

                Type editorType = null;
                Type pluginType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (editorType == null && asm.GetName().Name == "com.IvanMurzak.Unity.MCP.Editor")
                    {
                        editorType = asm.GetType("com.IvanMurzak.Unity.MCP.UnityMcpPluginEditor");
                    }
                    if (pluginType == null && asm.GetName().Name == "com.IvanMurzak.Unity.MCP.Runtime")
                    {
                        pluginType = asm.GetType("com.IvanMurzak.Unity.MCP.UnityMcpPlugin");
                    }
                }

                if (editorType == null)
                {
                    return;
                }

                // Force KeepConnected = false
                PropertyInfo keepConnectedProp = editorType.GetProperty("KeepConnected", BindingFlags.Public | BindingFlags.Static);
                if (keepConnectedProp != null)
                {
                    bool isKeepConnected = (bool)keepConnectedProp.GetValue(null);
                    if (isKeepConnected)
                    {
                        keepConnectedProp.SetValue(null, false);
                    }
                }

                // Get instance
                PropertyInfo instanceProp = editorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                object instance = instanceProp?.GetValue(null);
                if (instance != null)
                {
                    // Check if HasMcpPluginInstance
                    PropertyInfo hasPluginProp = instance.GetType().GetProperty("HasMcpPluginInstance", BindingFlags.Public | BindingFlags.Instance);
                    bool hasPlugin = hasPluginProp != null && (bool)hasPluginProp.GetValue(instance);

                    if (hasPlugin)
                    {
                        // Check ConnectionState
                        PropertyInfo stateProp = editorType.GetProperty("ConnectionState", BindingFlags.Public | BindingFlags.Static);
                        object stateReactive = stateProp?.GetValue(null);
                        string stateStr = stateReactive?.ToString() ?? "";

                        // If not Disconnected, immediately disconnect to stop spam
                        if (!stateStr.Contains("Disconnected"))
                        {
                            MethodInfo disconnectImm = instance.GetType().GetMethod("DisconnectImmediate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (disconnectImm != null)
                            {
                                disconnectImm.Invoke(instance, null);
                            }
                            else
                            {
                                MethodInfo disconnectMethod = instance.GetType().GetMethod("Disconnect", BindingFlags.Public | BindingFlags.Instance);
                                disconnectMethod?.Invoke(instance, null);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently ignore to avoid clutter
            }
        }

        public static void DisableInvalidUnityConnect()
        {
            // Do not call UnbindProject() as it triggers token exchange web request failures (UnityConnectWebRequestException)
        }

        [MenuItem("Tools/MCP/Clear Console")]
        public static void ClearConsole()
        {
            try
            {
                Type logEntries = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    logEntries = asm.GetType("UnityEditor.LogEntries");
                    if (logEntries != null) break;
                }
                var clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                clearMethod?.Invoke(null, null);
            }
            catch { }
        }
    }
}
