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
        public override void OnAwake()
        {
            nodeData.nodeType = GTNodeType.SingleChoice;

            GTNextNodeData choiceData = new GTNextNodeData()
            {
                data = "Next Node",
                id = ""
            };
            nodeData.connectedPorts.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (GTNextNodeData outChannel in nodeData.connectedPorts)
            {
                // Data is not needed- this is the named output
                Port outPort = this.CreatePort(outChannel.data);
                outPort.userData = outChannel;
                outputContainer.Add(outPort);
            }
            Debug.Log(nodeData.connectedPorts.Count);
            RefreshExpandedState();
        }
    }
}
