using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GT.Data
{
    public interface INodeData { }

    [Serializable]
    public class GTNodeConnection 
    {
        public GTNodeConnection()
        {
            nodeData = new();
        }

        [field: SerializeReference] public SerializableNodeData nodeData { get; set; }

        public GTNodeConnection CreateNewCopy()
        {
            GTNodeConnection newConnection = (GTNodeConnection)Activator.CreateInstance(this.GetType());
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is ICloneable cloneable)
                    prop.SetValue(newConnection, cloneable.Clone());
                else if (value is List<SerializableNodeData> connections)
                    prop.SetValue(newConnection, connections.Select(c => c.CreateNewCopy()).ToList());
                else if (value is SerializableNodeData conn)
                    prop.SetValue(newConnection, conn.CreateNewCopy());
                else
                    prop.SetValue(newConnection, value);
            }
            return newConnection;
        }

        // Get all references (Ptrs) to allow remapping between graph and SO data
        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            if (nodeData != null)
            {
                foreach (var accessor in nodeData.GetAllReferences())
                    yield return accessor;
            }
        }
    }
}