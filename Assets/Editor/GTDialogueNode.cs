using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Utilities;
    using Windows;

    [NodeForData(typeof(DialogueTableGTData))]
    public class GTDialogueTable : GTNode, IStringResult
    {
        // Refactor to reduce knowledge barrier and implementation steps for users
        public override bool OnEdgeConnected(Edge edge)
        {
            if (CanEdgeConnect<GTStringConnection>(edge)) return true;
            return true;
        }

        public override void OnAwake()
        {
        }

        public override void OnDraw()
        {

        }

        public string GetStringResult()
        {
            if (!(nodeData is DialogueTableGTData)) return "";
            DialogueTableGTData nodeDData = (DialogueTableGTData)nodeData;

            return nodeDData.dataTest;
        }
    }

    [NodeForData(typeof(DialogueAct))]
    public class GTDialogueNode : GTNode
    {
        public override bool OnEdgeConnected(Edge edge)
        {
            if (CanEdgeConnect<GTStringConnection>(edge)) return true;
            return true;
        }

        public override void OnAwake()
        {
            if (!(nodeData is DialogueAct)) return;
            DialogueAct nodeDData = (DialogueAct)nodeData;
            nodeDData.stringPort = null;
        }

        public override void OnDraw()
        {
            if (!(nodeData is DialogueAct)) return;
            DialogueAct nodeDData = (DialogueAct)nodeData;

            Port outPort = this.CreatePort(nodeDData.stringPort == null ? "Out port" : nodeDData.stringPort.GetStringResult());//(string) nodeDData.choiceData.GetValue()
            outPort.userData = nodeDData.stringPort;
            outputContainer.Add(outPort);

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