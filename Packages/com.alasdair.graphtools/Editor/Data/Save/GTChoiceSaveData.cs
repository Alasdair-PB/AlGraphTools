using System;
using UnityEngine;

namespace GT.Data.Save
{
    [Serializable]
    public class GTChoiceSaveData<TData> where TData : GTData
    {
        [field: SerializeField] public TData Data { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}