using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Enumerations;
    using GT.Data;
    using System;
    using Utilities;

    [Serializable] public class MultipleChoiceData : GTNodeData { }

    [NodeForData(typeof(MultipleChoiceData))]
    public class GTMultipleChoiceNode : GTNode
    {
        public override void OnAwake()
        {
            GTNodeConnection choiceData = new GTNodeConnection()
            {
                data = "Next Node",
            };
            nodeData.connectedPorts.Add(choiceData);
        }

        public override void OnDraw()
        {
            DrawAddOutChannel();
            //DrawAddOutDifChannel();
            foreach (GTNodeConnection outChannel in nodeData.connectedPorts)
            {
                Port outPort = CreateChoicePort(outChannel);
                outputContainer.Add(outPort);
            }
            RefreshExpandedState();
        }

        public void DrawAddOutChannel()
        {
            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTNodeConnection choiceData = new GTNodeConnection()
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
                GTNodeConnection choiceData = new GTNodeConnection()
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

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = userData;
            GTNodeConnection choiceData = (GTNodeConnection)userData;

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