using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    // Use attributes to describe ports?


    public abstract class GTGraph<StartingNode> : ScriptableObject where StartingNode : GTNodeData
    {
        [field: SerializeField] public string FileName { get; set; } // Move to graph view
        [field: SerializeField] public List<GTGroupSaveData> Groups { get; set; }
        [field: SerializeReference] public List<GTNodeData> Nodes { get; set; } 
        [field:SerializeReference] public StartingNode startingNode { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        // Make this abstract and don't keep nodes here?
        // Could go thorugh property fields and get all node data
        public virtual List<GTNodeData> GetNodes() => Nodes;

        // Public create Property window -> expose graph variables to edit in the graph view.

        public GTGraph()
        {
            Groups = new List<GTGroupSaveData>();
            Nodes = new List<GTNodeData>();
        }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            Groups = new List<GTGroupSaveData>();
            Nodes = new List<GTNodeData>();
        }

    }
}