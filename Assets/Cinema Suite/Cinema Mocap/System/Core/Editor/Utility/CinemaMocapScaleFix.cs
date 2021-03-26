using UnityEditor;

namespace CinemaSuite.CinemaMocap.System.Core.Editor.Utility
{
    public class CinemaMocapScaleFix : AssetPostprocessor
    {
        public void OnPreprocessModel()
        {
            ModelImporter modelImporter = (ModelImporter)assetImporter;

            if (modelImporter.assetPath.Length > 58 &&
                string.Compare(modelImporter.assetPath.Substring(0, 57), "Assets/Cinema Suite/Cinema Mocap/Animations/MoCapHumanoid") == 0)
            {
                modelImporter.globalScale = 0.01f;
            }
        }
    }
}