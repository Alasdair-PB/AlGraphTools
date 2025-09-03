using UnityEngine;
using GT.Data;
using System;
using GT.Elements;


[Serializable] // Would be serilizable node data?
public class DialogueTableGTData : GTNodeData
{
    // Refactor csv file import
    [field: SerializeField] public string dataTest { get; set; }
}

[Serializable]
public class DialogueGTData : GTNodeData
{
    // In port
    // This should be a connection not a node reference?
    public DialogueTableGTData dialogueTable { get; set; }
}


[Serializable]
public class TableConnection : GTNodeConnection {}