using UnityEngine;
using GT.Data;
using System;


[Serializable]
public class DialogueTableGTData : GTNodeData
{
    [field: SerializeField] public string dataTest { get; set; }
    // To be changed to csv file import
}

[Serializable]
public class DialogueGTData : GTNodeData
{
    // In port
    public DialogueTableGTData dialogueTable { get; set; }
}
