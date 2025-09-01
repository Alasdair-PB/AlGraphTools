using Codice.CM.Client.Differences.Merge;
using GT.Enumerations;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class SerializableNodeData
    {
        public SerializableNodeData () { NodePtr = null;}

        [NonSerialized] public GTNodeData NodePtr = null;
        public GTNodeData NodeData => NodePtr;

        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            yield return (() => NodePtr, value => NodePtr = value);
            //yield return (() => anotherRef, value => anotherRef = value);
        }
    }


    // To Refactor as abstract
    [Serializable]
    public class GTNodeData
    {
        [field: SerializeField] public string id { get; set; } // To replace with nodeGuid
        [SerializeField] private string nodeGuid = System.Guid.NewGuid().ToString();
        public string Guid => nodeGuid;
        [field: SerializeField] public string name { get; set; } // To Remove
        [field: SerializeField] public List<GTNodeConnection> connectedPorts { get; set; }
        [SerializeField] private List<string> connectedGuids = new();
        [field: SerializeField] public GTNodeType nodeType { get; set; } // To Remove
        [field: SerializeField] public bool isStartingNode { get; set; } // To Remove
        [field: SerializeField] public string groupID { get; set; } // Investigate refactor to remove from this object
        [field: SerializeField] public Vector2 position { get; set; } // Investigate refactor to remove from this object

        public virtual List<SerializableNodeData> GetSerializedNodes()
        {
            List<SerializableNodeData> serializedNodes = new List<SerializableNodeData>();
            foreach (var node in connectedPorts)
                serializedNodes.Add(node?.nodeData);
            return serializedNodes;
        }

        public virtual GTNodeData OnCreateCopy()
        {
            GTNodeData newNode = new GTNodeData();
            return newNode;
        }

        public GTNodeData CreateNewCopy()
        {
            GTNodeData newNode = OnCreateCopy();

            newNode.id = this.id;
            newNode.name = this.name;
            newNode.nodeType = this.nodeType;
            newNode.groupID = this.groupID;
            newNode.position = this.position;
            newNode.connectedPorts = new List<GTNodeConnection>();

            foreach (GTNodeConnection outChannelData in connectedPorts)
                newNode.connectedPorts.Add(outChannelData.CreateNewCopy());
            return newNode;
        }

        public void OnBeforeSerialize(Dictionary<GTNodeData, string> nodeToGuid)
        {
            List<SerializableNodeData> serializedNodes = GetSerializedNodes();
            connectedGuids.Clear();

            foreach (var node in serializedNodes)
            {
                if (node != null && node.NodePtr != null && nodeToGuid.TryGetValue(node.NodePtr, out var nodeId))
                    connectedGuids.Add(nodeId);
                else if (node == null || node.NodePtr == null)
                    connectedGuids.Add("");
            }
        }

        public void OnAfterDeserialize(Dictionary<string, GTNodeData> guidToNode)
        {
            List<SerializableNodeData> serializedNodes = GetSerializedNodes();
            int serializedNodeCount = serializedNodes.Count;

            for(int i = 0; i < serializedNodeCount; i++)
            {
                if (connectedGuids[i] != "")
                {
                    if (guidToNode.TryGetValue(connectedGuids[i], out var target))
                        serializedNodes[i].NodePtr = target;
                }
                else
                {   if (serializedNodes[i] == null)
                        serializedNodes[i] = new();
                }
            }
        }

        public GTNodeData()
        {
            id = "";
            name = "";
            connectedPorts = new List<GTNodeConnection>();
            nodeType = GTNodeType.SingleChoice;
            isStartingNode = false;
            groupID = "";
            position = new Vector2();
        }

        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            List<SerializableNodeData> allRefs = GetSerializedNodes();
            foreach (var node in allRefs)
            {
                foreach (var accessor in node.GetAllReferences())
                    yield return accessor;
            }
        }
    }
}
