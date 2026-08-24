// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System.Drawing;
using HtmlRenderer.Entities;
using HtmlRenderer.Parse;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Dom
{
    /// <summary>
    /// CSS box for hr element.
    /// </summary>
    internal sealed class CssBoxHr : CssBox
    {
        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxHr(CssBox parent, HtmlTag tag) 
            : base(parent, tag)
        {
            Display = CssConstants.Block;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.
        /// </summary>
        /// <param name="g">Device context to use</param>
        protected override void PerformLayoutImp(Graphics g)
        {
            if (Display == CssConstants.None)
                return;

            RectanglesReset();

            var prevSibling = DomUtils.GetPreviousSibling(this);
            float left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
            float top = (prevSibling == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (prevSibling != null ? prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth : 0);
            Location = new PointF(left, top);
            ActualBottom = top;

            //width at 100% (or auto)
            float minwidth = GetMinimumWidth();
            float width = ContainingBlock.Size.Width
                          - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                          - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth
                          - ActualMarginLeft - ActualMarginRight - ActualBorderLeftWidth - ActualBorderRightWidth;

            //Check width if not auto
            if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width))
            {
                width = CssValueParser.ParseLength(Width, width, this);
            }

            if (width < minwidth || width >= 9999)
                width = minwidth;

            float height = ActualHeight;
            if(height < 1)
            {
                height = Size.Height + ActualBorderTopWidth + ActualBorderBottomWidth;
            }
            if (height < 1)
            {
                height = 2;
            }
            if(height<=2 && ActualBorderTopWidth < 1 && ActualBorderBottomWidth < 1)
            {
                BorderTopStyle = BorderBottomStyle = CssConstants.Solid;
                BorderTopWidth = "1px";
                BorderBottomWidth = "1px";
            }

            Size = new SizeF(width, height);

            ActualBottom = Location.Y + ActualPaddingTop + ActualPaddingBottom + height;
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected override void PaintImp(Graphics g)
        {
            var offset = HtmlContainer != null ? HtmlContainer.ScrollOffset : PointF.Empty;
            var rect = new RectangleF(Bounds.X + offset.X, Bounds.Y + offset.Y, Bounds.Width, Bounds.Height);

            if (rect.Height > 2 && CssUtils.IsColorVisible(ActualBackgroundColor))
            {
                g.FillRectangle(CssUtils.GetSolidBrush(ActualBackgroundColor), rect);
            }

            var b1 = CssUtils.GetSolidBrush(ActualBorderTopColor);
            DrawingUtils.DrawBorder(Border.Top, g, this, b1, rect, true, true);

            if (rect.Height > 1)
            {
                var b2 = CssUtils.GetSolidBrush(ActualBorderLeftColor);
                DrawingUtils.DrawBorder(Border.Left, g, this, b2, rect, true, true);

                var b3 = CssUtils.GetSolidBrush(ActualBorderRightColor);
                DrawingUtils.DrawBorder(Border.Right, g, this, b3, rect, true, true);

                var b4 = CssUtils.GetSolidBrush(ActualBorderBottomColor);
                DrawingUtils.DrawBorder(Border.Bottom, g, this, b4, rect, true, true);
            }
        }
    }
}