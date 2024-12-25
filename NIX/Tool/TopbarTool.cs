using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Nix.Editor.Tool
{
    public static class TopbarTool
    {
        [MenuItem("Tools/Nix/Find missing scripts/DeleteAll")]
        static void FindAndDeleteMissingScripts()
        {
            foreach (GameObject gameObject in ToolUtils.GetAllComponents<GameObject>())
            {
                foreach (Component component in gameObject.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        Debug.Log("scripts on object: " + gameObject.name);
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                        break;
                    }
                }
            }
        }

        [MenuItem("Tools/Nix/Ping GameObjects with Duplicate Scripts")]
        public static void SelectDuplicateScripts()
        {
            GameObject[] allGameObjects = ToolUtils.GetAllComponents<GameObject>();
            Dictionary<string, List<GameObject>> scriptDictionary = new Dictionary<string, List<GameObject>>();

            // Find all GameObjects and store their scripts
            foreach (GameObject go in allGameObjects)
            {
                Component[] components = go.GetComponents<Component>();

                foreach (Component component in components)
                {
                    string scriptName = component.GetType().Name;

                    // If the script name is not already in the dictionary, add it
                    if (!scriptDictionary.ContainsKey(scriptName))
                    {
                        scriptDictionary[scriptName] = new List<GameObject>();
                    }

                    scriptDictionary[scriptName].Add(go);
                }
            }

            // Store the list of GameObjects with duplicate scripts
            List<GameObject> duplicateGameObjects = new List<GameObject>();

            // Check which scripts are associated with more than one GameObject
            foreach (var kvp in scriptDictionary)
            {
                if (kvp.Value.Count > 1)
                {
                    duplicateGameObjects.AddRange(kvp.Value);
                }
            }

            // Ping all GameObjects that have duplicate scripts
            foreach (var gameObject in duplicateGameObjects)
            {
                Debug.Log($"GameObject {gameObject.name} is duplicate scripts!", gameObject);
            }

            // Log the number of GameObjects with duplicate scripts
            Debug.Log($"Selected {duplicateGameObjects.Count} GameObjects with duplicate scripts.");
        }


        [MenuItem("Tools/Nix/Fix TMP Font Size")]
        public static void FixTMPFontSize()
        {
            var allTMP = ToolUtils.GetAllComponents<TextMeshProUGUI>();
            foreach (var tmp in allTMP)
            {
                tmp.fontSizeMax = tmp.fontSize;
                tmp.enableAutoSizing = true;
            }
        }
    }
}