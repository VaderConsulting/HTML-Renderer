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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using HtmlRenderer.Entities;

namespace HtmlRenderer
{
    /// <summary>
    /// Provides HTML rendering on the tooltips
    /// </summary>
    public class HtmlToolTip : ToolTip
    {
        #region Fields and Consts

        /// <summary>
        /// 
        /// </summary>
        private HtmlContainer _htmlContainer;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public HtmlToolTip()
        {
            OwnerDraw = true;

            Popup += OnToolTipPopup;
            Draw += OnToolTipDraw;
            Disposed += OnToolTipDisposed;
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when an error occured during html rendering.<br/>
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;


        #region Private methods

        private void OnToolTipPopup(object sender, PopupEventArgs e)
        {
            string text = GetToolTip(e.AssociatedControl);
            string font = string.Format(NumberFormatInfo.InvariantInfo, "font: {0}pt {1}", e.AssociatedControl.Font.Size, e.AssociatedControl.Font.FontFamily.Name);

            //Create fragment container
            _htmlContainer = new HtmlContainer();
            _htmlContainer.AvoidGeometryAntialias = true;
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
            _htmlContainer.SetHtml("<div><table class=htmltooltipbackground cellspacing=5 cellpadding=0 style=\"" + font + "\"><tr><td style=border:0px>" + text + "</td></tr></table></div>");

            //Measure bounds of the container
            using (Graphics g = e.AssociatedControl.CreateGraphics())
            {
                _htmlContainer.PerformLayout(g);
            }

            //Set the size of the tooltip
            e.ToolTipSize = new Size((int) Math.Round(_htmlContainer.ActualSize.Width, MidpointRounding.AwayFromZero), (int) Math.Round(_htmlContainer.ActualSize.Height, MidpointRounding.AwayFromZero));
        }

        private void OnToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (_htmlContainer != null)
            {
                _htmlContainer.PerformPaint(e.Graphics);
            }
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            if (LinkClicked != null)
            {
                LinkClicked(this, e);
            }
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            if (RenderError != null)
            {
                RenderError(this, e);
            }
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        private void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            if (StylesheetLoad != null)
            {
                StylesheetLoad(this, e);
            }
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            if (ImageLoad != null)
            {
                ImageLoad(this, e);
            }
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        private void OnRefresh(object sender, HtmlRefreshEventArgs e)
        {
//            if (e.Layout)
//            {
//                if (InvokeRequired)
//                    Invoke(new MethodInvoker(PerformLayout));
//                else
//                    PerformLayout();
//            }
//            if (InvokeRequired)
//                Invoke(new MethodInvoker(Invalidate));
//            else
//                Invalidate();
        }

        /// <summary>
        /// Unsubscribe from events and dispose of <see cref="_htmlContainer"/>.
        /// </summary>
        private void OnToolTipDisposed(object sender, EventArgs eventArgs)
        {
            Popup -= OnToolTipPopup;
            Draw -= OnToolTipDraw;
            Disposed -= OnToolTipDisposed;

            if(_htmlContainer != null)
            {
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
        }

        #endregion
    }
}
