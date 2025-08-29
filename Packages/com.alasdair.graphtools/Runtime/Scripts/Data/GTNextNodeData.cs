using System;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNextNodeData
    {
        [field: SerializeField] public string data { get; set; }
        [field: SerializeField] public string id { get; set; }

        public GTNextNodeData CreateNewCopy()
        {
            GTNextNodeData newNode = new GTNextNodeData();
            newNode.data = this.data;
            newNode.id = this.id;
            return newNode;
        }
    }
}