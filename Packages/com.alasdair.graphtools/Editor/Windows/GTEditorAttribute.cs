using System;
using System.Collections.Generic;
using System.Linq;

[AttributeUsage(AttributeTargets.Class)]
public class GTEditorAttribute : Attribute
{
    public Type GraphType { get; }
    public GTEditorAttribute(Type graphType) => GraphType = graphType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NodeForDataAttribute : Attribute
{
    public Type DataType { get; }

    public NodeForDataAttribute(Type dataType)
    {
        DataType = dataType;
    }
}