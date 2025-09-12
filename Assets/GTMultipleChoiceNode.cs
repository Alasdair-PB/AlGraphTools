using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using GT.Data;
    using System;

    [Serializable] public class MultipleChoiceData : GTNodeData {
        GTPortConnection<string> choiceData;
        public string data;
    }

    [NodeForData(typeof(MultipleChoiceData))]
    public class GTMultipleChoiceNode : GTNode
    {
        public override bool OnEdgeConnected(Edge edge)
        {
            if (CanEdgeConnect<GTPortConnection<string>, string>(edge, ()=> OutString())) return true;
            return true;
        }

        public string OutString()
        {
            return ((MultipleChoiceData)nodeData).data;
        }

        public override void OnAwake()
        {
            GTPortConnection<string> choiceData = new GTPortConnection<string>(() => "");
            //nodeData.choiceData = choiceData;
        }

        public override void OnDraw()
        {
            DrawAddOutChannel();
            /*foreach (GTNodeConnection outChannel in nodeData.ConnectedPorts)
            {
                Port outPort = CreateChoicePort(outChannel);
                outputContainer.Add(outPort);
            }*/
            RefreshExpandedState();
        }

        public void DrawAddOutChannel()
        {
            /*Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTPortConnection<string> choiceData = new GTPortConnection<string>(()=>"Next node");
                nodeData.ConnectedPorts.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });
            addChoiceButton.AddToClassList("gt-node__button");
            mainContainer.Insert(1, addChoiceButton);*/
        }

       /* private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            if (!(userData is GTPortConnection<string>)) return choicePort;

            choicePort.userData = userData;
            GTPortConnection<string> choiceData = (GTPortConnection<string>) userData;

            Button deleteChoiceButton = GTElementUtility.CreateButton("X", () =>
            {
                if (nodeData.ConnectedPorts.Count == 1)
                    return;

                if (choicePort.connected)
                    graphView.DeleteElements(choicePort.connections);

                nodeData.ConnectedPorts.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("gt-node__button");

            TextField choiceTextField = GTElementUtility.CreateTextField(((string)choiceData.GetValue()), null, callback =>
            {
                ((MultipleChoiceData)nodeData).data = callback.newValue;
            });

            choiceTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__text-field__hidden",
                "gt-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }*/
    }
}