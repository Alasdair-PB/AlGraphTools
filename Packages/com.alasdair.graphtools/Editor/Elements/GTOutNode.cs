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

    public abstract class GTMultiOutNode : GTNode
    {
        public List<GTOutNodeSaveData> OutChannels { get; set; }

        public override void GTNodeInitialize(string nodeName, GTGraphView gtGraphView, Vector2 position)
        {
            OutChannels = new List<GTOutNodeSaveData>();
            GTOutNodeInitialize(nodeName, gtGraphView, position);

            NodeType = GTNodeType.MultipleChoice;
            OutChannels.Add(InitializeOutSaveData());
        }

        public abstract GTOutNodeSaveData InitializeOutSaveData();
        public abstract void GTOutNodeInitialize(string nodeName, GTGraphView gtGraphView, Vector2 position);
        public abstract void GTOutNodeDraw();
        public abstract GTOutNodeSaveData GTSaveOutNodeToGraph();
        public abstract GTNextNodeData SaveToNodeData(GTOutNodeSaveData data);
        public abstract GTOutNodeSaveData CloneSaveData(GTOutNodeSaveData data);
        private List<GTNextNodeData> ConvertNodeChoicesToNextNodeData(List<GTOutNodeSaveData> nodeChoices)
        {
            List<GTNextNodeData> myNodeChoices = new List<GTNextNodeData>();
            foreach (GTOutNodeSaveData nodeChoice in nodeChoices)
            {
                GTNextNodeData outNodeData = SaveToNodeData(nodeChoice);
                myNodeChoices.Add(outNodeData);
            }
            return myNodeChoices;
        }

        private List<GTOutNodeSaveData> CloneNodeChoices(List<GTOutNodeSaveData> outNodeData)
        {
            List<GTOutNodeSaveData> outNodes = new List<GTOutNodeSaveData>();
            foreach (GTOutNodeSaveData saveData in outNodeData)
            {
                GTOutNodeSaveData choiceData = CloneSaveData(saveData);
                outNodes.Add(choiceData);
            }
            return outNodes;
        }

        public override GTNodeSaveData GTSaveNodeToGraph() {
            GTOutNodeSaveData outData = GTSaveOutNodeToGraph();

            outData.outChannels = ConvertNodeChoicesToNextNodeData(OutChannels);
            return outData; 
        }

        public override GTNodeData GTSaveNodeToDataObject() {
            GTOutNodeData outNodeData = new GTOutNodeData();
            outNodeData.Initialize( , nodeType, isStartingNode);
            return outNodeData;
        }

        public override void GTLoadNodeData(GTNodeSaveData loadData) {
            var outNodeData = loadData as GTOutNodeSaveData;
            outNodeData.outChannels = CloneNodeChoices(OutChannels);

        }

        public override void GTNodeDraw()
        {
            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTOutNodeSaveData choiceData = new GTOutNodeSaveData()
                {
                    Data = new DialogueGTData() { Text = "new Choice" }
                };

                OutChannels.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("gt-node__button");
            mainContainer.Insert(1, addChoiceButton);

            foreach (GTOutNodeSaveData outChannel in OutChannels)
            {
                Port outPort = CreateChoicePort(outChannel);
                outputContainer.Add(outPort);
            }
            RefreshExpandedState();

            GTOutNodeDraw();
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = userData;
            GTOutNodeSaveData choiceData = userData as GTOutNodeSaveData;

            if (choiceData == null) return choicePort;

            Button deleteChoiceButton = GTElementUtility.CreateButton("X", () =>
            {
                if (OutChannels.Count == 1)
                    return;

                if (choicePort.connected)
                    graphView.DeleteElements(choicePort.connections);

                OutChannels.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("gt-node__button");

            TextField choiceTextField = GTElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Data.Text = callback.newValue;
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