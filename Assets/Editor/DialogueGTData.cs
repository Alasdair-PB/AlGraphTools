using UnityEngine;
using GT.Data;
using System;
using GT.Elements;


[Serializable] // Would be serilizable node data?
public class DialogueTableGTData : GTNodeData
{
    [field: SerializeField] public string dataTest { get; set; }
    // To be changed to csv file import
}

[Serializable]
public class DialogueGTData : GTNodeData
{
    // In port
    // This should be a connection not a node reference?
    public DialogueTableGTData dialogueTable { get; set; }
}
