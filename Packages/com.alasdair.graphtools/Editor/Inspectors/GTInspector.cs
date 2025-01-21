using System.Collections.Generic;
using UnityEditor;

namespace GT.Inspectors
{
    using Utilities;
    //using ScriptableObjects;

    [CustomEditor(typeof(GTNode))]
    public class GTInspector : Editor
    {
        private SerializedProperty nodeContainerProperty;
        private SerializedProperty nodeProperty;

        private SerializedProperty startingNodesOnlyProperty;
        private SerializedProperty selectedNodeIndexProperty;

        private void OnEnable()
        {
            nodeContainerProperty = serializedObject.FindProperty("nodeContainer");
            nodeProperty = serializedObject.FindProperty("node");
            startingNodesOnlyProperty = serializedObject.FindProperty("startingNodesOnly");
            selectedNodeIndexProperty = serializedObject.FindProperty("selectedNodeIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawNodeContainerArea();
            /*GTNodeContainerSO currentNodeContainer = (GTNodeContainerSO) nodeContainerProperty.objectReferenceValue;

            if (currentNodeContainer == null)
            {
                StopDrawing("Select a Node Container to see the rest of the Inspector.");
                return;
            }

            DrawFiltersArea();

            bool currentStartingNodesOnlyFilter = startingNodesOnlyProperty.boolValue;
            
            List<string> nodeNames;

            string nodeFolderPath = $"Assets/NodeSystem/Nodes/{currentNodeContainer.FileName}";
            string nodeInfoMessage;

            nodeNames = currentNodeContainer.GetUngroupedNodeNames(currentStartingNodesOnlyFilter);
            nodeFolderPath += "/Global/Nodes";
            nodeInfoMessage = "There are no" + (currentStartingNodesOnlyFilter ? " Starting" : "") + " Ungrouped Nodes in this Node Container.";

            if (nodeNames.Count == 0)
            {
                StopDrawing(nodeInfoMessage);
                return;
            }

            DrawNodeArea(nodeNames, nodeFolderPath);

            */
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNodeContainerArea()
        {
            GTInspectorUtility.DrawHeader("Node Container");
            nodeContainerProperty.DrawPropertyField();
            GTInspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            GTInspectorUtility.DrawHeader("Filters");
            startingNodesOnlyProperty.DrawPropertyField();
            GTInspectorUtility.DrawSpace();
        }

        private void DrawNodeArea(List<string> nodeNames, string nodeFolderPath)
        {
            GTInspectorUtility.DrawHeader("Node");

            /*int oldSelectedNodeIndex = selectedNodeIndexProperty.intValue;
            GTNodeSO oldNode = (GTNodeSO) nodeProperty.objectReferenceValue;

            bool isOldNodeNull = oldNode == null;
            string oldNodeName = isOldNodeNull ? "" : oldNode.NodeName;

            UpdateIndexOnNamesListUpdate(nodeNames, selectedNodeIndexProperty, oldSelectedNodeIndex, oldNodeName, isOldNodeNull);

            selectedNodeIndexProperty.intValue = GTInspectorUtility.DrawPopup("Node", selectedNodeIndexProperty, nodeNames.ToArray());
            string selectedNodeName = nodeNames[selectedNodeIndexProperty.intValue];

            GTNodeSO selectedNode = GTIOUtility.LoadAsset<GTNodeSO>(nodeFolderPath, selectedNodeName);

            nodeProperty.objectReferenceValue = selectedNode;*/
            GTInspectorUtility.DrawDisabledFielgt(() => nodeProperty.DrawPropertyField());
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            GTInspectorUtility.DrawHelpBox(reason, messageType);
            GTInspectorUtility.DrawSpace();
            GTInspectorUtility.DrawHelpBox("You need to select a Node for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;
                return;
            }

            bool oldIndexIsOutOfBoungtOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoungtOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                    return;
                }
                indexProperty.intValue = 0;
            }
        }
    }
}