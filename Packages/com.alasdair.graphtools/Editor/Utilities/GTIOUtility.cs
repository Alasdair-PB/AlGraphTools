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

    public class GTIOUtility
    {
        private GTGraphView graphView;
        private string graphFileName;
        private string graphFilePath;

        private List<GTNode> nodes;
        private List<GTGroup> groups;

        private Dictionary<string, GTGroup> loadedGroups;
        private Dictionary<GTNodeData, GTNode> loadedNodes;
        private Dictionary<GTNodeData, GTNodeData> graphToObjectMap;

        private Type graphType;
        private string assetExtension;

        public GTIOUtility(Type in_graphType, string in_assetExtension)
        {
            graphType = in_graphType;
            assetExtension = in_assetExtension;
        }

        public void Initialize(GTGraphView gtGraphView, string filePath, string graphName)
        {
            graphView = gtGraphView;
            graphFileName = graphName;
            graphFilePath = filePath;

            nodes = new List<GTNode>();
            groups = new List<GTGroup>();

            loadedGroups = new Dictionary<string, GTGroup>();
            loadedNodes = new Dictionary<GTNodeData, GTNode>();
            graphToObjectMap = new Dictionary<GTNodeData, GTNodeData>();
        }
        GTNodeData CreateCopyFromGraph(GTNode in_node)
        {
            GTNodeData nodeData = in_node.nodeData.CreateNewCopy();
            graphToObjectMap.Add(in_node.nodeData, nodeData);
            return nodeData;
        }

        GTNode CreateCopyFromObject(GTNodeData nodeData)
        {
            Type nodeType = NodeTypeResolver.GetNodeTypeForData(nodeData.GetType());
            Debug.Log(nodeData.GetType().Name);
            if (nodeType == null) return null;
            GTNode newNode = (GTNode)graphView.CreateNode(nodeData.name, nodeType, nodeData.position, false);
            newNode.nodeData = nodeData.CreateNewCopy();
            newNode.Draw();

            graphToObjectMap.Add(newNode.nodeData, nodeData);
            loadedNodes.Add(newNode.nodeData, newNode);
            graphView.AddElement(newNode);

            return newNode;
        }

        public void Save()
        {
            GetElementsFromGraphView();
            GTGraph graphData = CreateAsset(graphFilePath, $"{graphFileName}");
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

        private void SaveNodeReferences(GTGraph graphData)
        {
            foreach (GTNodeData node in graphData.Nodes)
            {
                foreach (var (getter, setter) in node.GetAllReferences())
                {
                    var oldRef = getter();
                    if (oldRef != null && graphToObjectMap.ContainsKey(oldRef))
                        setter(graphToObjectMap[oldRef]);
                }
            }
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
            SaveNodeReferences(graphData);
        }

        private void SaveNodeToGraph(GTNode in_node, GTGraph graphData)
        {
            GTNodeData nodeData = CreateCopyFromGraph(in_node);
            nodeData.groupID = in_node.group?.id;
            nodeData.position = in_node.GetPosition().position;
            graphData.Nodes.Add(nodeData);
        }

        public string GetFullFilePath(string graphFilePath, string graphFileName)
        {
            return $"{graphFilePath}/{graphFileName}.{assetExtension}";
        }

        public bool Load()
        {
            string fullPath = GetFullFilePath(graphFilePath, graphFileName);
            GTGraph graphData = (GTGraph)AssetDatabase.LoadAssetAtPath(fullPath, graphType);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"{fullPath}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );
                return false;
            }
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodeReferecnes();
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
        private void LoadNodeReferecnes()
        {
            foreach (GTNode node in loadedNodes.Values)
            {
                var reverseMap = graphToObjectMap.ToDictionary(kv => kv.Value, kv => kv.Key);

                foreach (var (getter, setter) in node.nodeData.GetAllReferences())
                {
                    var currentValue = getter();
                    if (currentValue != null && reverseMap.TryGetValue(currentValue, out var key))
                        setter(key);
                }
            }
        }
        private void LoadNodes(List<GTNodeData> in_node)
        {
            foreach (GTNodeData nodeData in in_node)
            {
                GTNode newNode = CreateCopyFromObject(nodeData);

                if (string.IsNullOrEmpty(nodeData.groupID))
                    continue;

                GTGroup group = loadedGroups[nodeData.groupID];
                newNode.group = group;
                group.AddElement(newNode);
            }
        }
        private void LoadNodesConnections()
        {
            foreach (KeyValuePair<GTNodeData, GTNode> loadedNode in loadedNodes)
            {
                foreach (Port outPort in loadedNode.Value.outputContainer.Children())
                {
                    GTNodeData portConnection = loadedNode.Value.GetNodeConnection(outPort);
                    if (portConnection == null)
                        continue;

                    GTNode nextNode = loadedNodes[portConnection];
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

        public GTGraph CreateAsset(string path, string assetName)
        {
            string fullPath = GetFullFilePath(graphFilePath, graphFileName);
            GTGraph asset = (GTGraph)AssetDatabase.LoadAssetAtPath(fullPath, graphType);

            if (asset == null)
            {
                asset = (GTGraph) ScriptableObject.CreateInstance(graphType);
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
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