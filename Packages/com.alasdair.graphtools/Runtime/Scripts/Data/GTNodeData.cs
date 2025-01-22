using GT.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNodeData
    {
        [field: SerializeField] public GTData Data { get; set; }
        [field: SerializeField] public List<GTNextNodeData> NextNodes { get; set; }
        [field: SerializeField] public GTNodeType NodeType { get; set; }
        [field: SerializeField] public bool IsStartingNode { get; set; }

        public void Initialize(GTData data, List<GTNextNodeData> nextNodes, GTNodeType nodeType, bool isStartingNode)
        {
            Data = data;
            NextNodes = nextNodes;
            NodeType = nodeType;
            IsStartingNode = isStartingNode;
        }
    }
}
