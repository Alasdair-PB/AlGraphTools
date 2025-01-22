using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GT.Postprocessor
{
    using Data.Save;
    using GT.Windows;
    using System.IO;

    public class GTGraphOpen : AssetPostprocessor
    {
        [UnityEditor.Callbacks.OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            Object asset = EditorUtility.InstanceIDToObject(instanceID);

            if (asset is GTGraph graphAsset)
            {
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                GTEditorWindow.Open(assetPath);
                return true;
            }
            return false; 
        }
    }
}
