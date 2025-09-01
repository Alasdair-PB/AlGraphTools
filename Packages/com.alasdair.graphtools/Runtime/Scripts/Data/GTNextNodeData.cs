using System;
using UnityEngine;
using System.Collections.Generic;

namespace GT.Data
{
    [Serializable]
    public class GTNextNodeData 
    {
        public GTNextNodeData()
        {
            data = "";
            nodeData = new();
        }

        [field: SerializeField] public string data { get; set; }
        [field: SerializeReference] public SerializableNodeData nodeData { get; set; }

        public GTNextNodeData CreateNewCopy()
        {
            GTNextNodeData newNode = new GTNextNodeData();
            newNode.data = this.data;
            newNode.nodeData = this.nodeData;
            return newNode;
        }

        // Get all references (Ptrs) to allow remapping between graph and SO data
        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            if (nodeData != null)
            {
                foreach (var accessor in nodeData.GetAllReferences())
                    yield return accessor;
            }
        }
    }
}