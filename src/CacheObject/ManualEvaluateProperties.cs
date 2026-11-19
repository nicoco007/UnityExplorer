using UnityExplorer.Config;

namespace UnityExplorer.CacheObject
{
    /// <summary>
    /// Properties whose getter is known to have a side effect, so the inspector shows an Evaluate button
    /// for them instead of evaluating them automatically.
    /// </summary>
    internal static class ManualEvaluateProperties
    {
        // "Namespace.DeclaringType.PropertyName". Matched against the property's declaring type and its base types.
        private static readonly string[] builtIn =
        {
            // Instantiate a copy of the shared material(s) and assign it to the renderer.
            "UnityEngine.Renderer.material",
            "UnityEngine.Renderer.materials",
            // Instantiates a copy of the shared mesh and assigns it to the filter.
            "UnityEngine.MeshFilter.mesh",
            // Instantiates a copy of the shared physics material and assigns it to the collider.
            "UnityEngine.Collider.material",
            // Create instances of the shared font material(s).
            "TMPro.TMP_Text.fontMaterial",
            "TMPro.TMP_Text.fontMaterials",
            // Runs material modifiers; masked graphics add reference-counted stencil materials which are never released.
            "UnityEngine.UI.Graphic.materialForRendering",
        };

        private static readonly HashSet<string> signatures = new();
        private static string lastUserValue;

        public static bool IsManualEvaluate(PropertyInfo property)
        {
            RefreshIfNeeded();

            for (Type type = property.DeclaringType; type != null; type = type.BaseType)
            {
                string typeName = type.FullName;
                if (typeName == null)
                    continue;

                if (signatures.Contains($"{typeName}.{property.Name}"))
                    return true;

                // MelonLoader prefixes non-Unity namespaces with "Il2Cpp" (e.g. Il2CppTMPro.TMP_Text).
                if (typeName.StartsWith("Il2Cpp") && signatures.Contains($"{typeName.Substring("Il2Cpp".Length)}.{property.Name}"))
                    return true;
            }

            return false;
        }

        private static void RefreshIfNeeded()
        {
            string userValue = ConfigManager.Reflection_Manual_Evaluate_Properties?.Value ?? string.Empty;
            if (userValue == lastUserValue)
                return;

            lastUserValue = userValue;
            signatures.Clear();

            foreach (string signature in builtIn)
                signatures.Add(signature);

            foreach (string signature in userValue.Split(';'))
            {
                string trimmed = signature.Trim();
                if (trimmed.Length > 0)
                    signatures.Add(trimmed);
            }
        }
    }
}
