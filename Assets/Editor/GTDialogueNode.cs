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

    [NodeForData(typeof(DialogueTableGTData))]
    public class GTDialogueTable : GTNode
    {
        public override bool OnEdgeConnected(Edge edge)
        {
            if (!(edge.output.userData is TableConnection)) return false;

            TableConnection choiceData = (TableConnection) edge.output.userData;
            if (choiceData == null) return false;
            int inputIndex = edge.input.parent.IndexOf(edge.input);
            int outputIndex = edge.output.parent.IndexOf(edge.output);
            choiceData.nodeData.NodePtr = nodeData;
            return true;
        }

        public override void OnAwake()
        {
            /*GTNodeConnection choiceData = new GTNodeConnection()
            {
            };
            nodeData.connectedPorts.Add(choiceData);*/
        }

        public override void OnDraw()
        {
            /*foreach (GTNodeConnection outChannel in nodeData.connectedPorts)
            {
                Port outPort = this.CreatePort("Graph");
                outPort.userData = outChannel;
                outputContainer.Add(outPort);
            }
            RefreshExpandedState();*/
        }
    }

    [NodeForData(typeof(DialogueGTData))]
    public class GTDialogueNode : GTNode
    {

        public override void OnAwake()
        {
            if (!(nodeData is DialogueGTData)) return;

            //TableConnection choiceData = new TableConnection();
            //nodeData.connectedPorts.Add(choiceData);

            ((DialogueGTData)nodeData).dialogueTable = new TableConnection();
        }

        public override void OnDraw()
        {
            if (!(nodeData is DialogueGTData)) return;
            DialogueGTData nodeDData = (DialogueGTData)nodeData;

            Port outPort = this.CreatePort("Graph");
            outPort.userData = nodeDData.dialogueTable;
            outputContainer.Add(outPort);

            /*foreach (GTNodeConnection outChannel in nodeData.connectedPorts)
            {
                Port outPort = this.CreatePort("Graph");
                outPort.userData = outChannel;
                outputContainer.Add(outPort);
            }*/
            RefreshExpandedState();

            /*VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("gt-node__custom-data-container");
            Foldout textFoldout = GTElementUtility.CreateFoldout("Node Text");
            TextField textTextField = GTElementUtility.CreateTextArea(Data.Text, null, callback => Data.Text = callback.newValue);

            textTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);*/
        }
    }

}