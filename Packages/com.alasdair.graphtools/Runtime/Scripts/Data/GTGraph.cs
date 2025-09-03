using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    // To refactor for templated nodeData
    public class GTGraph : ScriptableObject, ISerializationCallbackReceiver
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<GTGroupSaveData> Groups { get; set; }
        [field: SerializeReference] public List<GTNodeData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public virtual void CreateNodeList()
        {
            Nodes = new List<GTNodeData>();
        }

        public GTGraph()
        {
            Groups = new List<GTGroupSaveData>();
            CreateNodeList();
        }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            Groups = new List<GTGroupSaveData>();
            CreateNodeList();
        }

        public GTNodeData GetStartingNode()
        {
            return Nodes.Count > 0 ? Nodes[0] : null;
        }

        public void OnBeforeSerialize()
        {
            var nodeToGuid = new Dictionary<GTNodeData, string>();
            foreach (var n in Nodes)
                nodeToGuid[n] = n.Guid;

            foreach (var n in Nodes)
                n.OnBeforeSerialize(nodeToGuid);
        }

        public void OnAfterDeserialize()
        {
            var guidToNode = new Dictionary<string, GTNodeData>();
            foreach (var n in Nodes)
                guidToNode[n.Guid] = n;

            foreach (var n in Nodes)
                n.OnAfterDeserialize(guidToNode);
        }

    }
}