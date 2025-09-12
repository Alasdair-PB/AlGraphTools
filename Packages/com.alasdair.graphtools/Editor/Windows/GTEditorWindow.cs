using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GT.Windows
{
    using GT.Data.Save;
    using System;
    using System.Linq;
    using Utilities;

    public abstract class GTEditorWindow : EditorWindow
    {
        private GTGraphView graphView;
        private readonly string defaultFileName = "NodesFileName";
        private readonly string defaultFilePath = "Assets/";

        private TextField fileNameTextField;
        private TextField filePathTextField;

        private Button saveButton;
        private Button miniMapButton;
        private GTIOUtility loadManager;
        private bool isInitialized = false;

        public static void Open(GTGraph graphAsset, string assetPath)
        {
            var windowType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(GTEditorWindow)) && !t.IsAbstract)
                .FirstOrDefault(t =>
                {
                    var attr = t.GetCustomAttributes(typeof(GTEditorAttribute), false)
                                .FirstOrDefault() as GTEditorAttribute;
                    return attr != null && attr.GraphType == graphAsset.GetType();
                });

            if (windowType == null)
                return;

            var window = GetWindow(windowType, false, "Node Graph") as GTEditorWindow;
            window.Initialize();
            window.Load(assetPath);
        }

        public void Load(string assetPath) 
        {
            string directoryPath = Path.GetDirectoryName(assetPath); 
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            loadManager.Initialize(graphView, directoryPath, Path.GetFileNameWithoutExtension(assetPath)); 
            
            if (loadManager.Load()) { 
                UpdateFileName(fileName); 
                UpdateFilePath(directoryPath); 
            }
        }
        public abstract List<Type> GetCustomNodeTypes();
        protected abstract Type GetGraphType();
        protected abstract string GetFileExtension();

        protected virtual void Initialize()
        {
            if (isInitialized) return;

            AddGraphView();
            AddToolbar();
            AddStyles();
            UpdateFileName(defaultFileName);
            UpdateFilePath(defaultFilePath);
            loadManager = new GTIOUtility(GetGraphType(), GetFileExtension());
            isInitialized = true;
        }

        private void OnEnable()
        {
            Initialize();
        }
      private void OnDisable()
        {
            isInitialized = false;
        }

        private void AddGraphView()
        {
            graphView = new GTGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            fileNameTextField = GTElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            filePathTextField = GTElementUtility.CreateTextField(defaultFilePath, "File Path:", callback =>
            {
                filePathTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = GTElementUtility.CreateButton("Save", () => Save());

            Button loadButton = GTElementUtility.CreateButton("Load", () => Load());
            Button clearButton = GTElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = GTElementUtility.CreateButton("Reset", () => ResetGraph());
            miniMapButton = GTElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("GTToolbarStyles.uss");
            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("GTVariables.uss");
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value) || string.IsNullOrEmpty(filePathTextField.value))
            {
                string filePath = EditorUtility.OpenFilePanel("Node Graphs", "Assets/", "asset");
                if (string.IsNullOrEmpty(filePath))
                    return;
                string directoryPath = Path.GetDirectoryName(filePath);
            }

            loadManager.Initialize(graphView, filePathTextField.value, fileNameTextField.value);
            loadManager.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Node Graphs", "Assets/", "asset");

            if (string.IsNullOrEmpty(filePath))
                return;

            string projectAssetsPath = Application.dataPath.Replace("\\", "/");
            filePath = filePath.Replace("\\", "/");

            if (!filePath.StartsWith(projectAssetsPath))
            {
                Debug.LogError("File not inside Assets folder.");
                return;
            }

            string relativePath = "Assets" + filePath.Substring(projectAssetsPath.Length);
            string directoryPath = Path.GetDirectoryName(relativePath);
            string fileName = Path.GetFileNameWithoutExtension(relativePath);

            Clear();
            loadManager.Initialize(graphView, directoryPath, fileName);

            if (loadManager.Load())
            {
                UpdateFileName(fileName);
                UpdateFilePath(directoryPath);
            }
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
            UpdateFilePath(defaultFilePath);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();
            miniMapButton.ToggleInClassList("gt-toolbar__button__selected");
        }

        public void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void UpdateFilePath(string newFilePath)
        {
            filePathTextField.value = newFilePath;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}