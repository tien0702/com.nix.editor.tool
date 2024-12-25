using UnityEditor;
using UnityEngine;

namespace Nix.Editor.Tool
{
    public class CopyPathToClipboard
    {
        // Add "Copy Path to Clipboard" option to the right-click menu in the Project window
        [MenuItem("Assets/Copy Path to Clipboard", false, 20)]
        private static void CopyPath()
        {
            // Get the path of the selected object
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!string.IsNullOrEmpty(path))
            {
                // Convert to absolute path
                string fullPath = Application.dataPath.Replace("Assets", "") + path;

                // Copy the path to the system clipboard
                EditorGUIUtility.systemCopyBuffer = fullPath;

                // Log success
                Debug.Log($"Path copied to clipboard: {fullPath}");
            }
            else
            {
                // Log an error if no file or folder is selected
                Debug.LogError("No file or folder selected!");
            }
        }

        // Validate the menu item to ensure it only shows when an object is selected
        [MenuItem("Assets/Copy Path to Clipboard", true)]
        private static bool ValidateCopyPath()
        {
            // Only show the option if an object is selected
            return Selection.activeObject != null;
        }

        [MenuItem("NIX/Toggle Visibility GameObjects #v")] // Shortcut: Shift + H
        private static void ToggleObstructingObjectsVisibility()
        {
            // Get the currently selected GameObject
            GameObject selectedObject = Selection.activeObject as GameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected.");
                return;
            }

            // Find the Canvas containing the selected GameObject
            Canvas parentCanvas = selectedObject.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("Selected GameObject is not inside a Canvas.");
                return;
            }

            // Start grouping Undo actions
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            // Iterate through all child GameObjects of the Canvas
            Transform canvasTransform = parentCanvas.transform;
            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                var child = canvasTransform.GetChild(i);
                Undo.RecordObject(child.gameObject, "Toggle Active State");
                child.gameObject.SetActive(false);
            }

            Transform targetParent = selectedObject.transform;

            while (targetParent != null)
            {
                Undo.RecordObject(targetParent.gameObject, "Toggle Active State");
                targetParent.gameObject.SetActive(true);
                targetParent = targetParent.parent;
            }

            // Close the Undo group to support Ctrl+Z
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("Toggled visibility of all obstructing GameObjects. Undo supported.");
        }
    }
}