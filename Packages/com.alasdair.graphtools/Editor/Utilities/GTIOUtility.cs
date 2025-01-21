using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using Windows;

    public static class GTIOUtility
    {
        private static GTGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<GTNode> nodes;
        private static List<GTGroup> groups;

        private static Dictionary<string, GTGroup> loadedGroups;
        private static Dictionary<string, GTNode> loadedNodes;
        private static Dictionary<string, GTNodeData> createdNodes;

        public static void Initialize(GTGraphView gtGraphView, string graphName)
        {
            graphView = gtGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/NodeSystem/Nodes/{graphName}";

            nodes = new List<GTNode>();
            groups = new List<GTGroup>();

            createdNodes = new Dictionary<string, GTNodeData>();
            loadedGroups = new Dictionary<string, GTGroup>();
            loadedNodes = new Dictionary<string, GTNode>();
        }

        public static void Save()
        {
            CreateDefaultFolders();
            GetElementsFromGraphView();

            GTGraphSaveDataSO graphData = CreateAsset<GTGraphSaveDataSO>("Assets/Editor/NodeSystem/Graphs", $"{graphFileName}Graph");

            graphData.Initialize(graphFileName);

            SaveGroups(graphData);
            SaveNodes(graphData);
            SaveAsset(graphData);
        }

        private static void SaveGroups(GTGraphSaveDataSO graphData)
        {
            List<string> groupNames = new List<string>();
            foreach (GTGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                groupNames.Add(group.title);
            }
            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(GTGroup group, GTGraphSaveDataSO graphData)
        {
            GTGroupSaveData groupData = new GTGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            graphData.Groups.Add(groupData);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, GTGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();
                foreach (string groupToRemove in groupsToRemove)
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
            }
            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveNodes(GTGraphSaveDataSO graphData)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (GTNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToDataObject(node);
                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.NodeName);
                    continue;
                }
                ungroupedNodeNames.Add(node.NodeName);
            }
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(GTNode node, GTGraphSaveDataSO graphData)
        {
            List<GTChoiceSaveData> choices = CloneNodeChoices(node.Choices);
            GTNodeSaveData nodeData = new GTNodeSaveData()
            {
                ID = node.ID,
                Name = node.NodeName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                NodeType = node.NodeType,
                Position = node.GetPosition().position
            };
            graphData.Nodes.Add(nodeData);
        }
        private static void SaveNodeToDataObject(GTNode node)
        {
            GTNodeData myNode = new GTNodeData();

            myNode.Initialize(
                node.Text,
                ConvertNodeChoicesToNodeChoices(node.Choices),
                node.NodeType,
                node.IsStartingNode()
            );

            createdNodes.Add(node.ID, myNode);
        }
        private static List<GTNextNodeData> ConvertNodeChoicesToNodeChoices(List<GTChoiceSaveData> nodeChoices)
        {
            List<GTNextNodeData> myNodeChoices = new List<GTNextNodeData>();

            foreach (GTChoiceSaveData nodeChoice in nodeChoices)
            {
                GTNextNodeData choiceData = new GTNextNodeData()
                {
                    Text = nodeChoice.Text
                };
                myNodeChoices.Add(choiceData);
            }
            return myNodeChoices;
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, GTGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();

                    foreach (string nodeToRemove in nodesToRemove)
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Nodes", nodeToRemove);
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, GTGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();
                foreach (string nodeToRemove in nodesToRemove)
                    RemoveAsset($"{containerFolderPath}/Global/Nodes", nodeToRemove);
            }
            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        public static void Load()
        {
            GTGraphSaveDataSO graphData = LoadAsset<GTGraphSaveDataSO>("Assets/Editor/NodeSystem/Graphs", graphFileName);
            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/NodeSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );
                return;
            }

            GTEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<GTGroupSaveData> groups)
        {
            foreach (GTGroupSaveData groupData in groups)
            {
                GTGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<GTNodeSaveData> nodes)
        {
            foreach (GTNodeSaveData nodeData in nodes)
            {
                List<GTChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                GTNode node = graphView.CreateNode(nodeData.Name, nodeData.NodeType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                node.Draw();

                graphView.AddElement(node);
                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                    continue;

                GTGroup group = loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, GTNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    GTChoiceSaveData choiceData = (GTChoiceSaveData) choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                        continue;

                    GTNode nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = (Port) nextNode.inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    graphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Editor/NodeSystem", "Graphs");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(GTGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is GTNode node)
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    GTGroup group = (GTGroup) graphElement;
                    groups.Add(group);
                    return;
                }
            });
        }

        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
                return;

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static List<GTChoiceSaveData> CloneNodeChoices(List<GTChoiceSaveData> nodeChoices)
        {
            List<GTChoiceSaveData> choices = new List<GTChoiceSaveData>();
            foreach (GTChoiceSaveData choice in nodeChoices)
            {
                GTChoiceSaveData choiceData = new GTChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };
                choices.Add(choiceData);
            }
            return choices;
        }
    }
}