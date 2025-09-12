using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GT.Data
{
    [Serializable]
    public abstract class GTNodeData
    {
        [SerializeField] private string nodeGuid = System.Guid.NewGuid().ToString();
        public string Guid => nodeGuid;

        [SerializeField] private List<string> connectedGuids = new();
        [field: SerializeField] public string Name { get; set; } 
        [field: SerializeField] public string GroupID { get; set; } = "";
        [field: SerializeField] public Vector2 Position { get; set; } = Vector2.zero;

        public IEnumerable<(Func<GTNodeData> Getter, Action<GTNodeData> Setter)> GetAllReferences()
        {
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is GTNodeConnection)
                {
                    foreach (var accessor in ((GTNodeConnection)value).GetAllReferences())
                        yield return accessor;
                }
            }

            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value is GTNodeConnection conn)
                {
                    foreach (var accessor in ((GTNodeConnection)value).GetAllReferences())
                        yield return accessor;
                }
            }
        }

        public GTNodeData CreateNewCopy()
        {
            GTNodeData newNode = (GTNodeData) Activator.CreateInstance(this.GetType());
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is ICloneable cloneable)
                    prop.SetValue(newNode, cloneable.Clone());
                else if (value is List<GTNodeConnection> connections)
                    prop.SetValue(newNode, connections.Select(c => c.CreateNewCopy()).ToList());
                else if (value is GTNodeConnection conn)
                    prop.SetValue(newNode, conn.CreateNewCopy());
                else
                    prop.SetValue(newNode, value);
            }

            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value is GTNodeConnection conn)
                    field.SetValue(newNode, conn.CreateNewCopy());
                else
                    field.SetValue(newNode, value);
            }
            return newNode;
        }


        /*
        public void OnBeforeSerialize(Dictionary<GTNodeData, string> nodeToGuid)
        {
            connectedGuids.Clear();
            GTNodeData newNode = (GTNodeData)Activator.CreateInstance(this.GetType());
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is GTNodeConnection)
                {
                    foreach (var (getter, _) in ((GTNodeConnection)value).GetAllReferences())
                    {
                       // var node = getter();
                        //connectedGuids.Add(node != null && nodeToGuid.TryGetValue(node, out var id) ? id : "");
                    }
                }
            }
            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value is GTNodeConnection conn)
                {
                    foreach (var (getter, _) in ((GTNodeConnection)value).GetAllReferences())
                    {
                        //var node = getter();
                        //connectedGuids.Add(node != null && nodeToGuid.TryGetValue(node, out var id) ? id : "");
                    }
                }
            }
        }

        public void OnAfterDeserialize(Dictionary<string, GTNodeData> guidToNode)
        {
            int i = 0;
            //connectedGuids = new List<string>();
            var props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                var value = prop.GetValue(this);

                if (value is GTNodeConnection)
                {
                    foreach (var (_, setter) in ((GTNodeConnection)value).GetAllReferences())
                    {
                        //var guid = connectedGuids[i++];
                        //setter(string.IsNullOrEmpty(guid) ? null : guidToNode[guid]);
                    }
                }
            }

            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value is GTNodeConnection conn)
                {
                    foreach (var (_, setter) in ((GTNodeConnection)value).GetAllReferences())
                    {
                        //var guid = connectedGuids[i++];
                        //setter(string.IsNullOrEmpty(guid) ? null : guidToNode[guid]);
                    }
                }
            }
        }*/
    }
}
