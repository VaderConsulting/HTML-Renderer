namespace HtmlRenderer
{
    /// <summary>
    /// Represents an anonymous inline box
    /// </summary>
    /// <remarks>
    /// To learn more about anonymous inline boxes visit:
    /// http://www.w3.org/TR/CSS21/visuren.html#anonymous
    /// </remarks>
    public class CssAnonymousBox : CssBox
    {
        public CssAnonymousBox(CssBox parentBox)
            : base(parentBox)
        {}
    }
}
