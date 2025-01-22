using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    public class GTGraph : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<GTGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<GTNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<GTGroupSaveData>();
            Nodes = new List<GTNodeSaveData>();
        }
    }
}