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
    using Utilities;
    using Windows;

    public class GTNode : Node
    {
        public string ID { get; set; }
        public string NodeName { get; set; }
        public List<GTChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public GTNodeType NodeType { get; set; }
        public GTGroup Group { get; set; }

        protected GTGraphView graphView;
        private Color defaultBackgroundColor;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(string nodeName, GTGraphView gtGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            NodeName = nodeName;
            Choices = new List<GTChoiceSaveData>();
            Text = "Node text.";

            SetPosition(new Rect(position, Vector2.zero));

            graphView = gtGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("gt-node__main-container");
            extensionContainer.AddToClassList("gt-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            TextField myNodeNameTextField = GTElementUtility.CreateTextField(NodeName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(NodeName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(NodeName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    NodeName = target.value;

                    graphView.AddUngroupedNode(this);

                    return;
                }

                GTGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                NodeName = target.value;

                graphView.AddGroupedNode(this, currentGroup);
            });

            myNodeNameTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__text-field__hidden",
                "gt-node__filename-text-field"
            );

            titleContainer.Insert(0, myNodeNameTextField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Node Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */

            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("gt-node__custom-data-container");

            Foldout textFoldout = GTElementUtility.CreateFoldout("Node Text");

            TextField textTextField = GTElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__quote-text-field"
            );

            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
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
                {
                    continue;
                }

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