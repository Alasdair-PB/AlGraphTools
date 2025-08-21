using GT.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNodeData 
    {
        [field: SerializeField] public GTNodeType NodeType { get; set; }
        [field: SerializeField] public bool IsStartingNode { get; set; }

        public void Initialize(GTNodeType nodeType, bool isStartingNode)
        {
            NodeType = nodeType;
            IsStartingNode = isStartingNode;
        }
    }
}
