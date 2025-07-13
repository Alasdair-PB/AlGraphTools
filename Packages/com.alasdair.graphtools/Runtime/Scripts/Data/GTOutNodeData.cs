using GT.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNextNodeData
    {
        [field: SerializeField] public GTNodeData NextNode { get; set; }
    }

    [Serializable]
    public class GTOutNodeData : GTNodeData
    {
        [field: SerializeField] public List<GTNextNodeData> NextNodes { get; set; }

        public void Initialize(List<GTNextNodeData> nextNodes, GTNodeType nodeType, bool isStartingNode)
        {
            NextNodes = nextNodes;
            NodeType = nodeType;
            IsStartingNode = isStartingNode;
        }
    }
}
