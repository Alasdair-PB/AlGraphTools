using System;
using UnityEngine;

namespace GT.Data.Save
{
    [Serializable]
    public class GTGroupSaveData
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [field: SerializeField] public Vector2 position { get; set; }
    }
}