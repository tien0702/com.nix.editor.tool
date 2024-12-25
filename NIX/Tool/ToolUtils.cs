using UnityEngine;

namespace Nix.Editor.Tool
{
    public static class ToolUtils
    {
        public static T[] GetAllComponents<T>() where T : Object
        {
            return Object.FindObjectsOfType<T>(true);
        }
    }
}