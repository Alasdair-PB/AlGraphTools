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
    using System.Reflection;
    using Windows;

    public class GTIOUtility
    {
        private GTGraphView graphView;
        private string graphFileName;
        private string graphFilePath;

        private List<GTNode> nodes;
        private List<GTGroup> groups;

        private Dictionary<string, GTGroup> loadedGroups;
        private Dictionary<string, GTNode> loadedNodes;
        private Dictionary<string, GTNodeData> createdNodes;

        public void Initialize(GTGraphView gtGraphView, string filePath, string graphName)
        {
            graphView = gtGraphView;
            graphFileName = graphName;
            graphFilePath = filePath;

            nodes = new List<GTNode>();
            groups = new List<GTGroup>();

            createdNodes = new Dictionary<string, GTNodeData>();
            loadedGroups = new Dictionary<string, GTGroup>();
            loadedNodes = new Dictionary<string, GTNode>();
        }

        public void Save()
        {
            GetElementsFromGraphView();

            GTGraph graphData = CreateAsset<GTGraph>(graphFilePath, $"{graphFileName}");
            graphData.Initialize(graphFileName);

            SaveGroups(graphData);
            SaveNodes(graphData);
            SaveAsset(graphData);
        }

        private void SaveGroups(GTGraph graphData)
        {
            List<string> groupNames = new List<string>();
            foreach (GTGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                groupNames.Add(group.title);
            }
        }

        private void SaveGroupToGraph(GTGroup group, GTGraph graphData)
        {
            GTGroupSaveData groupData = new GTGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            graphData.Groups.Add(groupData);
        }

        private void SaveNodes(GTGraph graphData)
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
        }
        private void SaveNodeToGraph(GTNode node, GTGraph graphData)
        {
            Type type = node.Choices.GetType();
            List<GTChoiceSaveData<type>> choices = CloneNodeChoices<type>(node.Choices);
            GTNodeSaveData<TData> nodeData = new GTNodeSaveData<TData>()
            {
                ID = node.ID,
                Name = node.NodeName,
                Choices = choices,
                Data = node.Data,
                GroupID = node.Group?.ID,
                NodeType = node.NodeType,
                Position = node.GetPosition().position
            };
            graphData.Nodes.Add(nodeData);
        }
        private void SaveNodeToDataObject(GTNode node)
        {
            GTNodeData myNode = new GTNodeData();

            myNode.Initialize(
                node.Data,
                ConvertNodeChoicesToNextNodeData(node.Choices),
                node.NodeType,
                node.IsStartingNode()
            );

            createdNodes.Add(node.ID, myNode);
        }
        private List<GTNextNodeData> ConvertNodeChoicesToNextNodeData(List<GTChoiceSaveData> nodeChoices)
        {
            List<GTNextNodeData> myNodeChoices = new List<GTNextNodeData>();
            foreach (GTChoiceSaveData nodeChoice in nodeChoices)
            {
                GTNextNodeData choiceData = new GTNextNodeData()
                {
                    Data = nodeChoice.Data
                };
                myNodeChoices.Add(choiceData);
            }
            return myNodeChoices;
        }

        public bool Load()
        {
            GTGraph graphData = LoadAsset<GTGraph>(graphFilePath, graphFileName);
            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"{graphFilePath}{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );
                return false;
            }
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
            return true;
        }

        private void LoadGroups(List<GTGroupSaveData> groups)
        {
            foreach (GTGroupSaveData groupData in groups)
            {
                GTGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private void LoadNodes(List<GTNodeSaveData> nodes)
        {
            foreach (GTNodeSaveData nodeData in nodes)
            {
                List<GTChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                GTNode node = graphView.CreateNode(nodeData.Name, nodeData.NodeType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Data = nodeData.Data;
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

        private void LoadNodesConnections()
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

        private void GetElementsFromGraphView()
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

        private static List<GTChoiceSaveData<TData>> CloneNodeChoices<TData>(List<GTChoiceSaveData<TData>> nodeChoices) where TData : GTData 
        {
            List<GTChoiceSaveData<TData>> choices = new List<GTChoiceSaveData<TData>>();
            foreach (GTChoiceSaveData<TData> choice in nodeChoices)
            {
                GTChoiceSaveData<TData> choiceData = new GTChoiceSaveData<TData>()
                {
                    Data = choice.Data,
                    NodeID = choice.NodeID
                };
                choices.Add(choiceData);
            }
            return choices;
        }
    }
}