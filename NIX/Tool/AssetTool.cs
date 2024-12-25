using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Nix.Editor.Tool
{
/*
 * Script 0 - 50
 * TMP 51 - 100
 * Image 101 - 150
 * Create 151 - 200
 */

    public class AssetTool : UnityEditor.Editor
    {
        #region Tools

        [MenuItem("Assets/NIX/Find Using Font in Scene", false, 50)]
        private static void FindUsingFontScene()
        {
            // Get the currently selected TMP_FontAsset
            TMP_FontAsset targetFont = Selection.activeObject as TMP_FontAsset;
            if (targetFont == null)
            {
                Debug.LogWarning("Selected object is not a TMP_FontAsset.");
                return;
            }

            // Find all TextMeshProUGUI components in the scene
            TextMeshProUGUI[] allTexts = ToolUtils.GetAllComponents<TextMeshProUGUI>();
            List<GameObject> foundGameObjects = new List<GameObject>();

            // Check if each TextMeshProUGUI component is using the selected font
            foreach (var text in allTexts)
            {
                if (text.font == targetFont)
                {
                    foundGameObjects.Add(text.gameObject); // Add the GameObject to the list
                }
            }

            // Select the found GameObjects in the Hierarchy
            if (foundGameObjects.Count > 0)
            {
                Selection.objects = foundGameObjects.ToArray(); // Set the selection to the found GameObjects
                Debug.Log(
                    $"Selected {foundGameObjects.Count} GameObjects using the selected TMP_FontAsset in the scene.");
            }
            else
            {
                Debug.Log("No TextMeshProUGUI components in the scene are using the selected TMP_FontAsset.");
            }
        }

        [MenuItem("Assets/NIX/Normalize Font Sizes", false, 3)]
        private static void NormalizeFontSizes()
        {
            if (!Selection.objects.Any(obj => obj is TMP_FontAsset))
            {
                Debug.Log("No TMP");
                return;
            }

            TMP_FontAsset[] selectedFonts = Selection.objects
                .Where(obj => obj is TMP_FontAsset)
                .Cast<TMP_FontAsset>()
                .ToArray();
            float referenceLineHeight = selectedFonts[0].faceInfo.lineHeight;

            if (selectedFonts.Length == 0)
            {
                Debug.LogWarning("No TMP_FontAsset selected.");
                return;
            }

            foreach (var fontAsset in selectedFonts)
            {
                if (fontAsset != null)
                {
                    float fontLineHeight = fontAsset.faceInfo.lineHeight;

                    float scaleFactor = referenceLineHeight / fontLineHeight;

                    var faceInfo = fontAsset.faceInfo;
                    faceInfo.scale = scaleFactor;
                    fontAsset.faceInfo = faceInfo;

                    EditorUtility.SetDirty(fontAsset);
                    Debug.Log($"Normalized font: {fontAsset.name}, Scale factor: {scaleFactor}");
                }
            }

            Debug.Log("Font sizes normalized for selected fonts.");
        }

        [MenuItem("Assets/NIX/Find references in scene", false, 3)]
        private static void FindReferencesSpriteInScene()
        {
            Texture2D sprite = Selection.activeObject as Texture2D;
            if (sprite == null)
            {
                Debug.LogWarning("Selected object is not a Sprite.");
                return;
            }

            // Find all TextMeshProUGUI components in the scene
            GameObject[] objs = ToolUtils.GetAllComponents<GameObject>();
            List<GameObject> foundGameObjects = new List<GameObject>();

            // Check if each TextMeshProUGUI component is using the selected font
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent<Image>(out var img) && img.sprite != null && img.sprite.texture == sprite)
                {
                    foundGameObjects.Add((GameObject)obj);
                }
                else if (obj.TryGetComponent<SpriteRenderer>(out var spriteRender) && spriteRender.sprite != null &&
                         spriteRender.sprite.texture == sprite)
                {
                    foundGameObjects.Add((GameObject)obj);
                }
            }

            // Select the found GameObjects in the Hierarchy
            if (foundGameObjects.Count > 0)
            {
                Selection.objects = foundGameObjects.ToArray(); // Set the selection to the found GameObjects
                Debug.Log(
                    $"Selected {foundGameObjects.Count} GameObjects using the selected {sprite.name} in the scene.");
            }
            else
            {
                Debug.Log("No Image or SpriteRenderer components in the scene are using the selected sprite.");
            }
        }

        [MenuItem("Assets/NIX/Select Script in Scene", false, 3)]
        private static void FindScriptReferencesInScene()
        {
            // Get the selected script in the Project window
            MonoScript selectedScript = Selection.activeObject as MonoScript;

            if (selectedScript == null)
            {
                Debug.LogWarning("Please select a valid script.");
                return;
            }

            // Get the type of the script we're looking for
            System.Type scriptType = selectedScript.GetClass();

            if (scriptType == null)
            {
                Debug.LogWarning("Selected script is not a valid MonoBehaviour.");
                return;
            }

            // Find all GameObjects in the scene
            GameObject[] allGameObjects = ToolUtils.GetAllComponents<GameObject>();
            List<GameObject> gameObjectsWithScript = new List<GameObject>();

            // Check each GameObject for the script
            foreach (GameObject go in allGameObjects)
            {
                Component[] components = go.GetComponents(scriptType);

                // Check if any component matches the script type
                if (components != null && components.Length > 0)
                {
                    gameObjectsWithScript.Add(go);
                    Debug.Log($"Found GameObject: {go.name}", go);
                }
            }

            // Select all GameObjects that have the specified script
            Selection.objects = gameObjectsWithScript.ToArray();

            // Log the result
            if (gameObjectsWithScript.Count > 0)
            {
                Debug.Log($"Found {gameObjectsWithScript.Count} GameObjects with script {selectedScript.name}.");
            }
            else
            {
                Debug.LogWarning($"No GameObjects found with script {selectedScript.name}.");
            }
        }

        [MenuItem("Assets/NIX/Create Prefab", false, 151)]
        private static void CreateButtonPrefab()
        {
            Object selectedObject = Selection.activeObject;
            string[] specialFolders = new[] { "Buttons", "Icons" };

            if (selectedObject != null && (selectedObject is Texture2D texture))
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                string directory = System.IO.Path.GetDirectoryName(path);
                if (directory == null || specialFolders.All(folder => directory.Contains(folder)))
                {
                    Debug.Log("Please select a valid directory.");
                    return;
                }

                if (directory.EndsWith("Buttons"))
                {
                    string prefabFolderPath = "Assets/Prefabs/Buttons";
                    if (!AssetDatabase.IsValidFolder(prefabFolderPath))
                    {
                        Directory.CreateDirectory(prefabFolderPath);
                        AssetDatabase.Refresh();
                    }

                    string btnName = selectedObject.name;
                    btnName = ConvertToPascalCase(btnName) + "Btn";
                    string savePrefabPath = Path.Combine(prefabFolderPath, btnName + ".prefab");

                    if (File.Exists(savePrefabPath))
                    {
                        Debug.LogWarning($"Prefab '{savePrefabPath}' is exists!.");
                        return;
                    }

                    // Create new GameObject and add Button component
                    GameObject newBtn = new GameObject();
                    newBtn.transform.position = Vector3.zero;
                    newBtn.AddComponent<Button>();

                    // Add Image to btn
                    string texturePath = AssetDatabase.GetAssetPath(texture);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    Image img = newBtn.AddComponent<Image>();
                    img.sprite = sprite;
                    img.SetNativeSize();

                    // Add TmpText
                    GameObject newTxt = new GameObject("BtnNameText");
                    newTxt.transform.SetParent(newBtn.transform);
                    newTxt.transform.localPosition = Vector3.zero;

                    var txt = newTxt.AddComponent<TextMeshProUGUI>();
                    txt.text = btnName;
                    txt.enableAutoSizing = true;
                    txt.fontSizeMax = 32;

                    txt.alignment = TextAlignmentOptions.Midline;

                    RectTransform rectTransform = newTxt.GetComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero; // Bottom-left corner (0, 0)
                    rectTransform.anchorMax = Vector2.one; // Top-right corner (1, 1)
                    rectTransform.offsetMin = Vector2.zero; // No offset
                    rectTransform.offsetMax = Vector2.zero;
                    // Save prefab
                    PrefabUtility.SaveAsPrefabAsset(newBtn, savePrefabPath);
                    AssetDatabase.Refresh();

                    // Destroy GameObject in scene
                    GameObject.DestroyImmediate(newBtn);

                    Debug.Log($"Add prefabs {btnName} at {prefabFolderPath}");
                }
                else if (directory.EndsWith("Icons"))
                {
                    string prefabFolderPath = "Assets/Prefabs/Icons";
                    if (!AssetDatabase.IsValidFolder(prefabFolderPath))
                    {
                        Directory.CreateDirectory(prefabFolderPath);
                        AssetDatabase.Refresh();
                    }

                    string iconName = selectedObject.name;
                    iconName = ConvertToPascalCase(iconName) + "Icon";
                    string savePrefabPath = Path.Combine(prefabFolderPath, iconName + ".prefab");

                    if (File.Exists(savePrefabPath))
                    {
                        Debug.LogWarning($"Prefab '{savePrefabPath}' is exists!.");
                        return;
                    }

                    // Create new GameObject and add Button component
                    GameObject newIcon = new GameObject(iconName);
                    newIcon.transform.position = Vector3.zero;

                    // Add Image to btn
                    string texturePath = AssetDatabase.GetAssetPath(texture);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    Image img = newIcon.AddComponent<Image>();
                    img.sprite = sprite;
                    img.SetNativeSize();

                    // Save prefab
                    PrefabUtility.SaveAsPrefabAsset(newIcon, savePrefabPath);
                    AssetDatabase.Refresh();
                    // Destroy GameObject in scene
                    GameObject.DestroyImmediate(newIcon);
                }
            }
            else
            {
                Debug.LogWarning("Object selected is not a Texture2D.");
            }
        }

        [MenuItem("Assets/NIX/Select duplicate this script", false, 4)]
        private static void SelectDuplicateScriptInScene()
        {
            // Get the selected script in the Project window
            MonoScript selectedScript = Selection.activeObject as MonoScript;

            if (selectedScript == null)
            {
                Debug.LogWarning("Please select a valid script.");
                return;
            }

            // Get the type of the script we're looking for
            System.Type scriptType = selectedScript.GetClass();

            if (scriptType == null)
            {
                Debug.LogWarning("Selected script is not a valid MonoBehaviour.");
                return;
            }

            // Find all GameObjects in the scene
            GameObject[] allGameObjects = ToolUtils.GetAllComponents<GameObject>();
            List<GameObject> gameObjectsWithScript = new List<GameObject>();

            // Check each GameObject for the script
            foreach (GameObject go in allGameObjects)
            {
                Component[] components = go.GetComponents(scriptType);

                // Check if any component matches the script type
                if (components != null && components.Length > 1)
                {
                    gameObjectsWithScript.Add(go);
                    Debug.Log($"Found GameObject: {go.name}", go);
                }
            }

            // Select all GameObjects that have the specified script
            Selection.objects = gameObjectsWithScript.ToArray();

            // Log the result
            if (gameObjectsWithScript.Count > 0)
            {
                Debug.Log($"Found {gameObjectsWithScript.Count} GameObjects with script {selectedScript.name}.");
            }
            else
            {
                Debug.LogWarning($"No GameObjects found with script {selectedScript.name}.");
            }
        }

        #endregion

        public static string ConvertToPascalCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string[] words = str.Split(new char[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLower());
            }

            return string.Join(string.Empty, words);
        }
    }
}