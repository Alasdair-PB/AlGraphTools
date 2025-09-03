using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    using GT.Data;
    using System;
    using Utilities;
    [Serializable] public class SingleChoiceData : GTNodeData {}

    [NodeForData(typeof(SingleChoiceData))]
    public class GTSingleChoiceNode : GTNode
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
