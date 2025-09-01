using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    using Data.Save;
    using Enumerations;
    using GT.Data;
    using Utilities;
    using Windows;

    public class GTSingleChoiceNode : GTNode
    {
        public override void CreateNodeData(string in_NodeName)
        {
            nodeData = new GTNodeData();
            nodeData.id = CreateNewGuid();
            nodeData.name = in_NodeName;
        }

        public override void OnAwake()
        {
            nodeData.nodeType = GTNodeType.SingleChoice;

            GTNextNodeData choiceData = new GTNextNodeData()
            {
                data = "Next Node",
            };
            nodeData.connectedPorts.Add(choiceData);
        }

        public override void OnDraw()
        {
            foreach (GTNextNodeData outChannel in nodeData.connectedPorts)
            {
                // Data is not needed- this is the named output
                Port outPort = this.CreatePort(outChannel.data);
                outPort.userData = outChannel;
                outputContainer.Add(outPort);
            }
            RefreshExpandedState();
        }
    }
}
