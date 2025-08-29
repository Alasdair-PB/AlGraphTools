using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Codice.CM.Common.Tree;
    using Data.Save;
    using Enumerations;
    using GT.Data;
    using Utilities;
    using Windows;

    public class GTNode : Node
    {
        public GTNodeData nodeData {get;set;}
        public GTGroup group { get; set; }
        protected GTGraphView graphView;
        private Color defaultBackgroundColor;

        public string GetNodeId()
        {
            return nodeData.id;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
        }

        public void InitializeGenerics(string in_NodeName, GTGraphView gtGraphView, Vector2 position)
        {
            nodeData = new GTNodeData();
            nodeData.id = Guid.NewGuid().ToString();
            nodeData.name = in_NodeName;
            SetPosition(new Rect(position, Vector2.zero));
            graphView = gtGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("gt-node__main-container");
            extensionContainer.AddToClassList("gt-node__extension-container");
        }

        public virtual void OnAwake() {}

        public void Initialize(string in_NodeName, GTGraphView gtGraphView, Vector2 position)
        {          
            InitializeGenerics(in_NodeName, gtGraphView, position);
            nodeData.connectedPorts = new List<GTNextNodeData>();
            OnAwake();
        }

        public virtual void Draw()
        {
            string name = nodeData.name;
            TextField myNodeNameTextField = GTElementUtility.CreateTextField(name, null, callback =>
            {
                TextField target = (TextField) callback.target;

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
            Port inputPort = this.CreatePort("Node Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
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