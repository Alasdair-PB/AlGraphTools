using System;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public class GTNextNodeData<TData> where TData : GTData
    {
        [field: SerializeField] public TData Data { get; set; }
        [field: SerializeField] public GTNodeData NextNode { get; set; }
    }
}
