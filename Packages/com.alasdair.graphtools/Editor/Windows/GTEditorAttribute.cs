using System;

[AttributeUsage(AttributeTargets.Class)]
public class GTEditorAttribute : Attribute
{
    public Type GraphType { get; }
    public GTEditorAttribute(Type graphType) => GraphType = graphType;
}