using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace GT.Utilities
{
    public static class GTStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }
            return element;
        }

        const string pathDirectory = "Packages/com.alasdair.graphtools/Editor/Styles/";
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(pathDirectory + styleSheetName);
                element.styleSheets.Add(styleSheet);
            }
            return element;
        }
    }
}