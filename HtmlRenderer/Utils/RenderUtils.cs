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
using System.Drawing.Drawing2D;
using HtmlRenderer.Dom;
using HtmlRenderer.Entities;

namespace HtmlRenderer.Utils
{
    /// <summary>
    /// General utility methods for paint operations.
    /// </summary>
    internal static class RenderUtils
    {
        /// <summary>
        /// Clip the region the graphics will draw on by the overflow style of the containing block.<br/>
        /// Recursivly travel up the tree to find containing block that has overflow style set to hidden. if not
        /// block found there will be no clipping and null will be returned.
        /// </summary>
        /// <param name="g">the graphics to clip</param>
        /// <param name="box">the box that is rendered to get containing blocks</param>
        /// <returns>the prev region if clipped, otherwise null</returns>
        public static Region ClipGraphicsByOverflow(Graphics g, CssBox box)
        {
            var containingBlock = box.ContainingBlock;
            while (true)
            {
                if (containingBlock.Overflow == CssConstants.Hidden)
                {
                    var prevClip = g.Clip;
                    var rect = box.ContainingBlock.ClientRectangle;
                    rect.X -= 2; // atodo: find better way to fix it
                    rect.Width += 2;
                    rect.Offset(box.HtmlContainer.ScrollOffset);
                    rect.Intersect(prevClip.GetBounds(g));
                    g.SetClip(rect);
                    return prevClip;
                }
                else
                {
                    var cBlock = containingBlock.ContainingBlock;
                    if(cBlock == containingBlock)
                        return null;
                    containingBlock = cBlock;
                }
            }
        }

        /// <summary>
        /// Return original clip region to the graphics object.<br/>
        /// Should be used with <see cref="ClipGraphicsByOverflow"/> return value to return clip back to original.
        /// </summary>
        /// <param name="g">the graphics to clip</param>
        /// <param name="prevClip">the region to set on the graphics (null - ignore)</param>
        public static void ReturnClip(Graphics g, Region prevClip)
        {
            if (prevClip != null)
            {
                g.SetClip(prevClip, CombineMode.Replace);
            }
        }
    }
}
