using GT.Enumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT.Data.Save
{
    [Serializable]
    public class GTOutNodeSaveData : GTNodeSaveData
    {

        public List<GTNextNodeData> outChannels;
    }
}