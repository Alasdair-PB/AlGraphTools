using UnityEngine;
using GT.Data;
using System;

[Serializable]
public class DialogueGTData : GTData
{
    [field: SerializeField] public string Text { get; set; }
}
