using System;
using UnityEngine;

namespace GT.Data.Save
{
    [Serializable]
    public class GTGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}