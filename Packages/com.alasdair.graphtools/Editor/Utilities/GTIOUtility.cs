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

        public void Initialize(GTGraphView gtGraphView, string filePath, string graphName)
        {
            graphView = gtGraphView;
            graphFileName = graphName;
            graphFilePath = filePath;

            nodes = new List<GTNode>();
            groups = new List<GTGroup>();

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
                id = group.id,
                name = group.title,
                position = group.GetPosition().position
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
                if (node.group != null)
                {
                    groupedNodeNames.AddItem(node.group.title, node.nodeData.name);
                    continue;
                }
                ungroupedNodeNames.Add(node.nodeData.name);
            }
        }
        private void SaveNodeToGraph(GTNode in_node, GTGraph graphData)
        {
            Debug.Log(in_node.nodeData.connectedPorts.Count + "This is the count on save called");
            GTNodeData nodeData = in_node.nodeData.CreateNewCopy();
            nodeData.groupID = in_node.group?.id;
            nodeData.position = in_node.GetPosition().position;
            graphData.Nodes.Add(nodeData);
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
                GTGroup group = graphView.CreateGroup(groupData.name, groupData.position);
                group.id = groupData.id;
                loadedGroups.Add(group.id, group);
            }
        }

        private void LoadNodes(List<GTNodeData> in_node)
        {
            foreach (GTNodeData nodeData in in_node)
            {
                GTNode node = graphView.CreateNode(nodeData.name, nodeData.nodeType, nodeData.position, false);
                node.nodeData = nodeData.CreateNewCopy();
                node.Draw();

                graphView.AddElement(node);
                loadedNodes.Add(node.GetNodeId(), node);

                if (string.IsNullOrEmpty(nodeData.groupID))
                    continue;

                GTGroup group = loadedGroups[nodeData.groupID];
                node.group = group;
                group.AddElement(node);
            }
        }

        private void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, GTNode> loadedNode in loadedNodes)
            {
                foreach (Port outPort in loadedNode.Value.outputContainer.Children())
                {
                    GTNextNodeData choiceData = (GTNextNodeData)outPort.userData;

                    if (string.IsNullOrEmpty(choiceData.id))
                        continue;

                    GTNode nextNode = loadedNodes[choiceData.id];
                    Port inPort = (Port) nextNode.inputContainer.Children().First();
                    Edge edge = outPort.ConnectTo(inPort);

                    graphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private void GetElementsFromGraphView()
        {
            Type groupType = typeof(GTGroup);
            nodes.Clear();
            groups.Clear();

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
    }
}