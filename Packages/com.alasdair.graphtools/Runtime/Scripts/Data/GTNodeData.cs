using Codice.CM.Client.Differences.Merge;
using GT.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNodeData
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [field: SerializeField] public string data { get; set; }
        [field: SerializeField] public List<GTNextNodeData> connectedPorts { get; set; }
        [field: SerializeField] public GTNodeType nodeType { get; set; }
        [field: SerializeField] public bool isStartingNode { get; set; }
        [field: SerializeField] public string groupID { get; set; }
        [field: SerializeField] public Vector2 position { get; set; }

        public GTNodeData()
        {
            id = "";
            name = "";
            data = "";
            connectedPorts = new List<GTNextNodeData>();
            nodeType = GTNodeType.SingleChoice;
            isStartingNode = false;
            groupID = "";
            position = new Vector2();
        }

        public GTNodeData CreateNewCopy()
        {
            GTNodeData newNode = new GTNodeData()
            {
                id = this.id,
                name = this.name,
                data = this.data,
                nodeType = this.nodeType,
                isStartingNode = this.isStartingNode,
                groupID = this.groupID,
                position = this.position,
            };

            newNode.connectedPorts = new List<GTNextNodeData>();
            foreach (GTNextNodeData outChannelData in connectedPorts)
            {
                newNode.connectedPorts.Add(outChannelData.CreateNewCopy());
            }
            Debug.Log(this.connectedPorts.Count + ": port count");
            return newNode;
        }
    }
}
