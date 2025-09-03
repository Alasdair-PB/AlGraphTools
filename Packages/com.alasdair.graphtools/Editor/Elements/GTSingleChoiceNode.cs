using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    using GT.Data;
    using System;
    using Utilities;

    [Serializable] public class SingleChoiceData : GTNodeData {}

    [Serializable]
    public class StringDataConnection : GTNodeConnection
    {
        [field: SerializeField] public string data { get; set; }

        public StringDataConnection()
        {
            nodeData = new();
            data = "";
        }
    }

    [NodeForData(typeof(SingleChoiceData))]
    public class GTSingleChoiceNode : GTNode
    {
        public override bool OnEdgeConnected(Edge edge)
        {
            if (!(edge.output.userData is StringDataConnection)) return false;

            StringDataConnection choiceData = (StringDataConnection) edge.output.userData;
            if (choiceData == null) return false;
            choiceData.nodeData.NodePtr = nodeData;
            return true;
        }
        public override void OnAwake()
        {
            StringDataConnection choiceData = new StringDataConnection();
            choiceData.data = "Next Node";
            nodeData.connectedPorts.Add(choiceData);
        }

        public override void OnDraw()
        {
            foreach (GTNodeConnection outChannel in nodeData.connectedPorts)
            {
                if (outChannel is StringDataConnection)
                {
                    Port outPort = this.CreatePort(((StringDataConnection) outChannel).data + " Data");
                    outPort.userData = outChannel;
                    outputContainer.Add(outPort);
                } else
                {
                    Port outPort = this.CreatePort(" Data");
                    outPort.userData = outChannel;
                    outputContainer.Add(outPort);

                }
            }
            RefreshExpandedState();
        }
    }
}
