using UnityEngine;
using GT.Data;
using System;

[Serializable]
public class DialogueGTData : INodeData
{
    [field: SerializeField] public string Text { get; set; }
}
