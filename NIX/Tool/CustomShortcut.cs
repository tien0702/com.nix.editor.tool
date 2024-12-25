using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nix.Editor.Tool
{
    public class CustomShortcut
    {
        [MenuItem("NIX/Toggle Active DemoUI #t")]
        private static void ToggleEnable()
        {
            string targetObjectName = "DemoUI";
            GameObject targetObject = FindGameObjectByName(targetObjectName);

            if (targetObject != null)
            {
                Undo.RecordObject(targetObject, "Toggle GameObject Enable");
                targetObject.SetActive(!targetObject.activeSelf);
            }
            else
            {
                Debug.LogWarning($"GameObject '{targetObjectName}' not found in the scene.");
            }
        }

        [MenuItem("NIX/Rename GameObject #r")]
        private static void RenameGameObject()
        {
            string[] specialNames = { "CurrencyBar" };
            string[] componentTypes = { "Panel", "Popup", "Layout" };
            GameObject target = Selection.activeGameObject;
            Component[] components = target.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                string className = components[i].GetType().Name;

                if (specialNames.Any(t => className.Equals(t)))
                {
                    target.name = className;
                    return;
                }

                if (!componentTypes.Any(t => className.EndsWith(t))) continue;
                target.name = className;
                return;
            }
        }

        public static GameObject FindGameObjectByName(string name)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == name && obj.hideFlags == HideFlags.None)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}