using GT.Data;
using GT.Data.Save;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// Brainstroming starting node- >
// Shouldn't use attributes as this is a runtime method
// templated classes could work, but may stop it from being serialized


public class DialogueGraph : GTGraph<SequenceAct> {

    public SequenceAct GetStartingAct() => startisNode.GetConnectedAct();

    [field: SerializeField] private GTPerformanceConnection startisNode;

    private void SaveStartingNode(Dictionary<GTNodeData, GTNodeData> graphToObjectMap)
    {
        foreach (var (getter, setter) in startisNode.GetAllReferences())
        {
            var oldRef = getter();
            if (oldRef != null && graphToObjectMap.ContainsKey(oldRef))
                setter(graphToObjectMap[oldRef]);
        }
    }

    private void LoadStartingNode(Dictionary<GTNodeData, GTNodeData> graphToObjectMap)
    {
        var reverseMap = graphToObjectMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        foreach (var (getter, setter) in startisNode.GetAllReferences())
        {
            var currentValue = getter();
            if (currentValue != null && reverseMap.TryGetValue(currentValue, out var key))
                setter(key);
        }
    }
}
