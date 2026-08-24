using System;
using HtmlRenderer.Entities;

namespace HtmlRenderer
{
    /// <summary>
    /// Represents an anonymous block box
    /// </summary>
    /// <remarks>
    /// To learn more about anonymous block boxes visit CSS spec:
    /// http://www.w3.org/TR/CSS21/visuren.html#anonymous-block-level
    /// </remarks>
    public class CssAnonymousBlockBox : CssBox
    {
        public CssAnonymousBlockBox(CssBox parent)
            : base(parent)
        {
            Display = CssConstants.Block;
        }

        public CssAnonymousBlockBox(CssBox parent, CssBox insertBefore)
            : this(parent)
        {
            int index = parent.Boxes.IndexOf(insertBefore);

            if (index < 0)
            {
                throw new Exception("insertBefore box doesn't exist on parent");
            }
            parent.Boxes.Remove(this);
            parent.Boxes.Insert(index, this);
        }
    }
}
