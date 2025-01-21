using System;
using UnityEngine;

namespace GT.Data.Save
{
    [Serializable]
    public class GTChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}