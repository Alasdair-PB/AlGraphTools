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
        public override GTNodeData CreateNodeData()
        {
            GTNodeData newNodeData = new GTNodeData();
            GTNodeConnection choiceData = new GTNodeConnection()
            {
                data = "Next Node",
            };
            newNodeData.connectedPorts.Add(choiceData);
            newNodeData.nodeType = GTNodeType.SingleChoice;
            return newNodeData;
        }

        public override void OnDraw()
        {
            foreach (GTNodeConnection outChannel in nodeData.connectedPorts)
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
