using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nix.Editor.Tool
{
    public class ObjectTool : MonoBehaviour
    {
        [MenuItem("GameObject/NIX/Rename GameObjects By Sprite", false, 10)]
        private static void RenameSelectedGameObjects()
        {
            var selectedObjects = Selection.gameObjects;

            foreach (var obj in selectedObjects)
            {
                string newName = "";

                Image imageComponent = obj.GetComponent<Image>();
                if (imageComponent != null && imageComponent.sprite != null)
                {
                    newName = imageComponent.sprite.name;
                }

                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    newName = spriteRenderer.sprite.name;
                }

                if (!string.IsNullOrEmpty(newName))
                {
                    Undo.RecordObject(obj, "Rename GameObject");
                    obj.name = newName;
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }
    }
}