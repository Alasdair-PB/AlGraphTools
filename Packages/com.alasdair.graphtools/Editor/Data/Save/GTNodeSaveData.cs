using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    using Enumerations;

    [Serializable]
    public class GTNodeSaveData<TData> where TData : GTData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public TData Data { get; set; }
        [field: SerializeField] public List<TData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public GTNodeType NodeType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}