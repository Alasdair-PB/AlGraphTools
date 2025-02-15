using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Windows
{
    using Data.Error;
    using Data.Save;
    using Elements;
    using Enumerations;
    using Utilities;

    public class GTGraphView : GraphView
    {
        private GTEditorWindow editorWindow;
        private GTSearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, GTNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, GTGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, GTNodeErrorData>> groupedNodes;

        private int nameErrorsAmount;
        public int NameErrorsAmount
        {
            get { return nameErrorsAmount;}
            set {
                nameErrorsAmount = value;
                if (nameErrorsAmount == 0)
                    editorWindow.EnableSaving();
                if (nameErrorsAmount == 1)
                    editorWindow.DisableSaving();
            }
        }

        public GTGraphView(GTEditorWindow gtEditorWindow)
        {
            editorWindow = gtEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, GTNodeErrorData>();
            groups = new SerializableDictionary<string, GTGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, GTNodeErrorData>>();

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;
                if (startPort.node == port.node)
                    return;
                if (startPort.direction == port.direction)
                    return;
                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Top modify: Creates node of node type in GT.elements namespace
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", GTNodeType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", GTNodeType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, GTNodeType myNodeType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("NodeName", myNodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("NodeGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }

        public GTGroup CreateGroup(string title, Vector2 position)
        {
            GTGroup group = new GTGroup(title, position);

            AddGroup(group);
            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is GTNode))
                    continue;

                GTNode node = (GTNode) selectedElement;
                group.AddElement(node);
            }
            return group;
        }

        public GTNode CreateNode(string nodeName, GTNodeType myNodeType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"GT.Elements.GT{myNodeType}Node");
            GTNode node = (GTNode) Activator.CreateInstance(nodeType);
            node.Initialize(nodeName, this, position);

            if (shouldDraw)
                node.Draw();

            AddUngroupedNode(node);
            return node;
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(GTGroup);
                Type edgeType = typeof(Edge);

                List<GTGroup> groupsToDelete = new List<GTGroup>();
                List<GTNode> nodesToDelete = new List<GTNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (GraphElement selectedElement in selection)
                {
                    if (selectedElement is GTNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (selectedElement.GetType() == edgeType)
                    {
                        Edge edge = (Edge) selectedElement;
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if (selectedElement.GetType() != groupType)
                        continue;

                    GTGroup group = (GTGroup) selectedElement;
                    groupsToDelete.Add(group);
                }

                foreach (GTGroup groupToDelete in groupsToDelete)
                {
                    List<GTNode> groupNodes = new List<GTNode>();

                    foreach (GraphElement groupElement in groupToDelete.containedElements)
                    {
                        if (!(groupElement is GTNode))
                            continue;

                        GTNode groupNode = (GTNode) groupElement;
                        groupNodes.Add(groupNode);
                    }

                    groupToDelete.RemoveElements(groupNodes);
                    RemoveGroup(groupToDelete);
                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (GTNode nodeToDelete in nodesToDelete)
                {
                    if (nodeToDelete.Group != null)
                        nodeToDelete.Group.RemoveElement(nodeToDelete);

                    RemoveUngroupedNode(nodeToDelete);
                    nodeToDelete.DisconnectAllPorts();
                    RemoveElement(nodeToDelete);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is GTNode))
                        continue;

                    GTGroup gtGroup = (GTGroup) group;
                    GTNode node = (GTNode) element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, gtGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is GTNode))
                        continue;

                    GTGroup gtGroup = (GTGroup) group;
                    GTNode node = (GTNode) element;

                    RemoveGroupedNode(node, gtGroup);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                GTGroup gtGroup = (GTGroup) group;

                gtGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(gtGroup.title))
                {
                    if (!string.IsNullOrEmpty(gtGroup.OldTitle))
                        ++NameErrorsAmount;
                }
                else
                {
                    if (string.IsNullOrEmpty(gtGroup.OldTitle))
                        --NameErrorsAmount;
                }
                RemoveGroup(gtGroup);
                gtGroup.OldTitle = gtGroup.title;
                AddGroup(gtGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        GTNode nextNode = (GTNode) edge.input.node;
                        GTChoiceSaveData choiceData = (GTChoiceSaveData) edge.output.userData;
                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                            continue;

                        Edge edge = (Edge) element;
                        GTChoiceSaveData choiceData = (GTChoiceSaveData) edge.output.userData;
                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        public void AddUngroupedNode(GTNode node)
        {
            string nodeName = node.NodeName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                GTNodeErrorData nodeErrorData = new GTNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            List<GTNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            ungroupedNodesList.Add(node);
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(GTNode node)
        {
            string nodeName = node.NodeName.ToLower();
            List<GTNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            ungroupedNodesList.Remove(node);
            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
                ungroupedNodes.Remove(nodeName);
        }

        private void AddGroup(GTGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                GTGroupErrorData groupErrorData = new GTGroupErrorData();
                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);
                return;
            }

            List<GTGroup> groupsList = groups[groupName].Groups;
            groupsList.Add(group);
            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++NameErrorsAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(GTGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();
            List<GTGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;
                groupsList[0].ResetStyle();
                return;
            }

            if (groupsList.Count == 0)
                groups.Remove(oldGroupName);
        }

        public void AddGroupedNode(GTNode node, GTGroup group)
        {
            string nodeName = node.NodeName.ToLower();
            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
                groupedNodes.Add(group, new SerializableDictionary<string, GTNodeErrorData>());

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                GTNodeErrorData nodeErrorData = new GTNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                groupedNodes[group].Add(nodeName, nodeErrorData);
                return;
            }

            List<GTNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;
            groupedNodesList.Add(node);
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(GTNode node, GTGroup group)
        {
            string nodeName = node.NodeName.ToLower();
            node.Group = null;
            List<GTNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);
            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorsAmount;
                groupedNodesList[0].ResetStyle();
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);
                if (groupedNodes[group].Count == 0)
                    groupedNodes.Remove(group);
            }
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
                searchWindow = ScriptableObject.CreateInstance<GTSearchWindow>();

            searchWindow.Initialize(this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));
            Add(miniMap);
            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "GTGraphViewStyles.uss",
                "GTNodeStyles.uss"
            );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();
            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
    }
}