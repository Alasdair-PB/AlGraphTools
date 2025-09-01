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

    public class GTMultipleChoiceNode : GTNode
    {
        public override void OnAwake()
        {
            nodeData.nodeType = GTNodeType.MultipleChoice;
            GTNextNodeData choiceData = new GTNextNodeData()
            {
                data ="new Choice" 
            };
            nodeData.connectedPorts.Add(choiceData);
        }

        public void DrawAddOutChannel()
        {
            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTNextNodeData choiceData = new GTNextNodeData()
                {
                    data = "new Choice"
                };

                nodeData.connectedPorts.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });
            addChoiceButton.AddToClassList("gt-node__button");
            mainContainer.Insert(1, addChoiceButton);
        }

        /*public void DrawAddOutDifChannel()
        {
            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTNextNodeData choiceData = new GTNextNodeData()
                {
                    data = "new Choice"
                };

                outChannels.Add(choiceData);
                Port outPort = CreateChoicePort(choiceData);
                outputContainer.Add(outPort);
            });
            addChoiceButton.AddToClassList("gt-node__button");
            mainContainer.Insert(1, addChoiceButton);
        }*/

        public override void OnDraw()
        {
            DrawAddOutChannel();
            //DrawAddOutDifChannel();
            foreach (GTNextNodeData outChannel in nodeData.connectedPorts)
            {
                Port outPort = CreateChoicePort(outChannel);
                outputContainer.Add(outPort);
            }
            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = userData;
            GTNextNodeData choiceData = (GTNextNodeData)userData;

            Button deleteChoiceButton = GTElementUtility.CreateButton("X", () =>
            {
                if (nodeData.connectedPorts.Count == 1)
                    return;

                if (choicePort.connected)
                    graphView.DeleteElements(choicePort.connections);

                nodeData.connectedPorts.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("gt-node__button");

            TextField choiceTextField = GTElementUtility.CreateTextField(choiceData.data, null, callback =>
            {
                choiceData.data = callback.newValue;
            });

            choiceTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__text-field__hidden",
                "gt-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}