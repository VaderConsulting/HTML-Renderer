namespace HtmlRenderer
{
    /// <summary>
    /// Represents an anonymous inline box which contains nothing but blank spaces
    /// </summary>
    public sealed class CssAnonymousSpaceBox : CssAnonymousBox
    {
        public CssAnonymousSpaceBox(CssBox parentBox)
            : base(parentBox)
        {}
    }
}