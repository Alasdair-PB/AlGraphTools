using UnityEngine;
using GT.Data;
using System;
using GT.Elements;

[Serializable]
public class TableConnection : GTNodeConnection { }

[Serializable] // Would be serilizable node data?
public class DialogueTableGTData : GTNodeData
{
    // Refactor csv file import
    [field: SerializeField] public string dataTest { get; set; }

    public string GetTableInfo()
    {
        return dataTest; 
    }
}

[Serializable]
public class DialogueGTData : GTNodeData
{
    public TableConnection dialogueTable { get; set; }
    public SerializableNodeData dialogueTableRef { get; set; }
    public bool repeatDialogue;

    public void GetDialogue()
    {

    }

    public void OnPerformanceStart()
    {
        // Setup get table info etc
        // SetUp events and listeners
    }

    public void OnPerformanceUpdate(float deltaTime)
    {
        // Trigger UIEvent
        // Draw textbox to screen

        // If time > x progress e.t.c
    }

    public void OnParticipantInteract()
    {
        // Finish this Performance
        // Start linked performance or end.

    }
}
