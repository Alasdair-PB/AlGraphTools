using UnityEngine;
using GT.Data;
using System;
using GT.Elements;

[Serializable]
public class TableConnection : GTNodeConnection {

    [NonSerialized] public ITableProvider connectedTable;

    // Refactor to allow different ports to be set to get Table Data from node data
    public string GetTableData()
    {
        if (nodeData is ITableProvider)
            connectedTable = (ITableProvider)nodeData;
        return connectedTable.GetTableData();
    }
}

[Serializable]

public class PerformerConnection : GTNodeConnection
{

    public string GetPerformer()
    {
        return "";
    }
}

public interface ITableProvider
{
    string GetTableData();
}

[Serializable]
public class DialogueTableGTData : GTNodeData, ITableProvider
{
    // Refactor csv file import
    [field: SerializeField] public string dataTest { get; set; }

    public string GetTableData()
    {
        return dataTest; 
    }
}

[Serializable]
public class DialogueGTData : GTNodeData
{

    public bool repeatDialogue;
    public Event OnPerformanceEnd;

    // To Investigate: node pointers out of connection list. 

    // What might be needed:
    //public TableConnection dialogueTable { get; set; }
    //public SerializableNodeData nextPerformer { get; set; }
    // OutChannel Performers: that subscribe to OnPerformanceEnd
    // OutChannel NextPeformance: if not null triggered after OnPerformanceEnd from
    // PlayerClickPerformer

    public void GetDialogue()
    {
        // Determine best way to get table from node.
        //var x = ((DialogueTableGTData)dialogueTable.nodeData.NodeData).dataTest;
        //var z = dialogueTable.GetTableData();
    }

    public void OnPerformanceStart()
    {
        // What occurs here: 
        // Set up side performers
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
