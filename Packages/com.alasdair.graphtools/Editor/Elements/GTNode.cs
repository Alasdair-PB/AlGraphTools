using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Data.Save;
    using Enumerations;
    using GT.Data;
    using Utilities;
    using Windows;

    public abstract class GTNode : Node
    {
        public GTNodeData nodeData {get;set;}
        public GTGroup group { get; set; }
        protected GTGraphView graphView;
        private Color defaultBackgroundColor;

        protected GTNode()
        {
            var hasAttr = Attribute.IsDefined(this.GetType(), typeof(NodeForDataAttribute));
            if (!hasAttr)
                throw new InvalidOperationException($"{GetType().Name} must define [NodeForData] attribute.");
        }

        // Returns a pointer to node data from the port's userData if possible
        public GTNodeData GetNodeConnection(Port outPort)
        {
            GTNodeConnection choiceData = (GTNodeConnection) outPort.userData;
            return choiceData == null ? null : choiceData.nodeData.NodePtr;
        }

        // Removes reference to this node from an out channel if valid.
        // Virtual in case a new node would replace the current reference or remove it from a list of references
        public virtual void OnEdgeCleared(Edge edge)
        {
            GTNodeConnection channelData = (GTNodeConnection)edge.output.userData;
            channelData.nodeData = new();
        }

        // Assigns this node data to the out channel if valid. Otherwise return false. 
        public virtual bool OnEdgeConnected(Edge edge)
        {
            if (!(edge.output.userData is GTNodeConnection))
                return false;

            GTNodeConnection choiceData = (GTNodeConnection) edge.output.userData;
            if (choiceData == null) return false;
            choiceData.nodeData.NodePtr = nodeData;
            return true;
        }

        public GTNodeData CreateNodeData()
        {
            var attr = (NodeForDataAttribute)Attribute.GetCustomAttribute(
            this.GetType(),
            typeof(NodeForDataAttribute));

            if (attr == null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} is missing NodeForDataAttribute"
                );
            }
            GTNodeData newNodeData = (GTNodeData)Activator.CreateInstance(attr.DataType);
            return newNodeData;
        }

        public virtual void OnAwake() {}

        public virtual void OnDraw() {}

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
        }

        public void InitializeGenerics(string in_NodeName, GTGraphView gtGraphView, Vector2 position)
        {
            nodeData = CreateNodeData();
            nodeData.name = in_NodeName;

            SetPosition(new Rect(position, Vector2.zero));
            graphView = gtGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("gt-node__main-container");
            extensionContainer.AddToClassList("gt-node__extension-container");
            OnAwake();
        }

        public void Initialize(string in_NodeName, GTGraphView gtGraphView, Vector2 position)
        {          
            InitializeGenerics(in_NodeName, gtGraphView, position);
        }

        private void DrawTextField()
        {
            string name = nodeData.name;
            TextField myNodeNameTextField = GTElementUtility.CreateTextField(name, null, callback =>
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(name))
                        ++graphView.NameErrorsAmount;
                }
                else
                {
                    if (string.IsNullOrEmpty(name))
                        --graphView.NameErrorsAmount;
                }

                if (group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                    nodeData.name = target.value;
                    graphView.AddUngroupedNode(this);
                    return;
                }

                GTGroup currentGroup = group;
                graphView.RemoveGroupedNode(this, group);
                nodeData.name = target.value;
                graphView.AddGroupedNode(this, currentGroup);
            });

            myNodeNameTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__text-field__hidden",
                "gt-node__filename-text-field"
            );

            titleContainer.Insert(0, myNodeNameTextField);
        }

        public void Draw()
        {
            DrawTextField();
            // foreach input draw input
            Port inputPort = this.CreatePort("Node Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            // foreach output draw output
            OnDraw();
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                    continue;
                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();
            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}