using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    public class GTGroup : Group
    {
        public string id { get; set; }
        public string oldTitle { get; set; }

        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public GTGroup(string groupTitle, Vector2 position)
        {
            id = Guid.NewGuid().ToString();

            title = groupTitle;
            oldTitle = groupTitle;

            SetPosition(new Rect(position, Vector2.zero));

            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}