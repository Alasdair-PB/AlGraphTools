using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public SerializableNodeData CreateNewCopy()
        {
            SerializableNodeData copy = new SerializableNodeData();
            copy.NodePtr = this.NodePtr;
            return copy; 
        }
    }

    [Serializable]
    public class SerializableNode 
    {
        public SerializableNode() { NodePtr = null; }

        [NonSerialized] public GTNodeData NodePtr = null;
        public GTNodeData NodeData => NodePtr;

        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            yield return (() => NodePtr, value => NodePtr = value);
            //yield return (() => anotherRef, value => anotherRef = value);
        }

        public SerializableNodeData CreateNewCopy()
        {
            SerializableNodeData copy = new SerializableNodeData();
            copy.NodePtr = this.NodePtr;
            return copy;
        }
    }

    [Serializable]
    public abstract class GTNodeData
    {
        [SerializeField] private string nodeGuid = System.Guid.NewGuid().ToString();
        public string Guid => nodeGuid;
        [field: SerializeField] public string name { get; set; } // To Remove
        [field: SerializeReference] public List<GTNodeConnection> connectedPorts { get; set; }
        [SerializeField] private List<string> connectedGuids = new();
        [field: SerializeField] public string groupID { get; set; } // Investigate refactor to remove from this object
        [field: SerializeField] public Vector2 position { get; set; } // Investigate refactor to remove from this object

        // Refactor to make non-vritual by iterating through fields
        public virtual List<SerializableNodeData> GetSerializedNodes()
        {
            List<SerializableNodeData> serializedNodes = new List<SerializableNodeData>();
            foreach (var node in connectedPorts)
                serializedNodes.Add(node?.nodeData);
            return serializedNodes;


            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is SerializableNodeData)
                    serializedNodes.Add(((SerializableNodeData)value));
                else if (value is GTNodeConnection)
                    serializedNodes.Add((((GTNodeConnection)value)?.nodeData));
            }
        }

        public GTNodeData CreateNewCopy()
        {
            GTNodeData newNode = (GTNodeData)Activator.CreateInstance(this.GetType());
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is ICloneable cloneable)
                    prop.SetValue(newNode, cloneable.Clone());
                else if (value is List<GTNodeConnection> connections)
                    prop.SetValue(newNode, connections.Select(c => c.CreateNewCopy()).ToList());
                else if (value is GTNodeConnection conn)
                    prop.SetValue(newNode, conn.CreateNewCopy());
                else
                    prop.SetValue(newNode, value);
            }
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
            name = "";
            connectedPorts = new List<GTNodeConnection>();
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
