using GT.Data;
using GT.Data.Save;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DialogueGraph : GTGraph {

    public SequenceAct GetStartingAct() => startingNode.GetConnectedAct();

    [field: SerializeField] private GTPerformanceConnection startingNode;

    private void SaveStartingNode(Dictionary<GTNodeData, GTNodeData> graphToObjectMap)
    {
        foreach (var (getter, setter) in startingNode.GetAllReferences())
        {
            var oldRef = getter();
            if (oldRef != null && graphToObjectMap.ContainsKey(oldRef))
                setter(graphToObjectMap[oldRef]);
        }
    }

    private void LoadStartingNode(Dictionary<GTNodeData, GTNodeData> graphToObjectMap)
    {
        var reverseMap = graphToObjectMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        foreach (var (getter, setter) in startingNode.GetAllReferences())
        {
            var currentValue = getter();
            if (currentValue != null && reverseMap.TryGetValue(currentValue, out var key))
                setter(key);
        }
    }
}
