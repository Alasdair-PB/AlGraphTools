using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    using Enumerations;

    [Serializable]
    public class GTNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<GTChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public GTNodeType NodeType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}