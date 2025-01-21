using System;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNextNodeData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public GTNodeData NextNode { get; set; }
    }
}
