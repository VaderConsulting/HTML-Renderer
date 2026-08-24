using HtmlRenderer.Entities;

namespace HtmlRenderer
{
    /// <summary>
    /// Represents an AnonymousBlockBox which contains only blank spaces
    /// </summary>
    public sealed class CssAnonymousSpaceBlockBox : CssAnonymousBlockBox
    {
        public CssAnonymousSpaceBlockBox(CssBox parent)
            : base(parent)
        {
            Display = CssConstants.None;
        }

        public CssAnonymousSpaceBlockBox(CssBox parent, CssBox insertBefore)
            : base(parent, insertBefore)
        {
            Display = CssConstants.None;
        }
    }
}