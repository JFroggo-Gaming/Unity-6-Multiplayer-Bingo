using UnityEngine;
using UnityEditor;
using System.IO;

namespace BingoGame.Editor
{
    public class CreateVisualVariants : EditorWindow
    {
        [MenuItem("BingoGame/Create Visual Variants")]
        public static void CreateVariants()
        {
            string folderPath = "Assets/BingoGame/Prefabs/Visuals";

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/BingoGame/Prefabs", "Visuals");
            }

            // Colors for each variant
            Color[] colors = new Color[]
            {
                Color.red,      // Variant 1
                Color.blue,     // Variant 2
                Color.green,    // Variant 3
                Color.yellow,   // Variant 4
                Color.magenta,  // Variant 5
                Color.cyan      // Variant 6
            };

            for (int i = 0; i < 6; i++)
            {
                // Create GameObject with Sphere
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = $"PlayerVisual_Variant_{i + 1}";

                // Scale it down a bit
                visual.transform.localScale = Vector3.one * 0.5f;

                // Create and assign material with color
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = colors[i];
                visual.GetComponent<Renderer>().material = mat;

                // Remove collider (we don't need physics for visual representation)
                DestroyImmediate(visual.GetComponent<SphereCollider>());

                // Save as prefab
                string prefabPath = $"{folderPath}/PlayerVisual_Variant_{i + 1}.prefab";

                // Delete old prefab if exists
                if (File.Exists(prefabPath))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }

                PrefabUtility.SaveAsPrefabAsset(visual, prefabPath);

                // Destroy temp object
                DestroyImmediate(visual);

                Debug.Log($"Created {prefabPath}");
            }

            AssetDatabase.Refresh();
            Debug.Log("✓ All 6 visual variants created successfully!");
            Debug.Log("Now assign them to BingoPlayer prefab -> Visual Prefabs array (Size: 6)");
        }
    }
}
