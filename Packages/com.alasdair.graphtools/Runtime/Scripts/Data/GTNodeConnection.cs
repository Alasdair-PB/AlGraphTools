using GT.Data;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public abstract class GTNodeConnection
{
    [field: SerializeReference] protected GTNodeData _connectedNode;
    public virtual GTNodeConnection CreateNewCopy()
    {
        GTNodeConnection copy = (GTNodeConnection)Activator.CreateInstance(this.GetType());
        copy._connectedNode = _connectedNode;
        return copy;
    }
    public void SetNodePtr(GTNodeData ptr) => _connectedNode = ptr;
    public void ClearNodeReferences() =>_connectedNode = null;
    public  GTNodeData GetNodeData() => _connectedNode;
    public bool HasNodeReference() => (_connectedNode != null);
    public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
    {
        yield return (() => _connectedNode, value => _connectedNode = value);
    }
}


// To remove in refactor with single and multiple choice node
[Serializable]
public class GTPortConnection<T> : GTNodeConnection
{
    [field: SerializeField] private Func<T> _getter;

    public void SetGetter(Func<T> getter) => _getter = getter;

    public GTPortConnection(Func<T> getter, GTNodeData connectedNode = null)
    {
        _getter = getter;
        _connectedNode = connectedNode;
    }

    //public object GetValue() => _getter();
    public T GetTypedValue() => _getter();

    public override GTNodeConnection CreateNewCopy()
    {
        return new GTPortConnection<T>(_getter, _connectedNode);
    }
}
