using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    using GT.Data;
    using System;
    using Utilities;


    [Serializable] public class SingleChoiceData : GTNodeData {
        GTPortConnection<string> choiceData;
    }

    [NodeForData(typeof(SingleChoiceData))]
    public class GTSingleChoiceNode : GTNode
    {
        public override bool OnEdgeConnected(Edge edge)
        {
            if (CanEdgeConnect<GTPortConnection<string>, string>(edge, () => "")) return true;
            return true;
        }

        public override void OnAwake()
        {
           // nodeData.choiceData = new GTPortConnection<string>(() => "");
        }

        public override void OnDraw()
        {
            /*foreach (GTNodeConnection outChannel in nodeData.ConnectedPorts)
            {
                if (outChannel is GTPortConnectionString)
                {
                    Port outPort = this.CreatePort(((GTPortConnectionString) outChannel).GetValue() + " Data");
                    outPort.userData = outChannel;
                    outputContainer.Add(outPort);
                } else
                {
                    Port outPort = this.CreatePort(" Data");
                    outPort.userData = outChannel;
                    outputContainer.Add(outPort);

                }
            }*/
            RefreshExpandedState();
        }
    }
}
