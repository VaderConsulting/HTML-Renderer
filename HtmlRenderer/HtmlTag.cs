using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlRenderer.Entities;
using HtmlRenderer.Parse;

namespace HtmlRenderer
{
    public sealed class HtmlTag
    {
        #region Fields

        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();
        private readonly string _tagName;
        private readonly bool _isClosing;

        #endregion

        /// <summary>
        /// init
        /// </summary>
        /// <param name="tag"></param>
        public HtmlTag(string tag)
        {
            tag = tag.Substring(1, tag.Length - (tag.Length > 2 && tag[tag.Length - 2] == '/' ? 3 : 2));

            int spaceIndex = tag.IndexOf(" ");

            //Extract tag name
            if (spaceIndex < 0)
            {
                _tagName = tag;
            }
            else
            {
                _tagName = tag.Substring(0, spaceIndex);
            }

            //Check if is end tag
            if (_tagName.StartsWith("/"))
            {
                _isClosing = true;
                _tagName = _tagName.Substring(1);
            }

            _tagName = _tagName.ToLower();

            //Extract attributes
            MatchCollection atts = RegexParserHelper.Match(RegexParserHelper.HmlTagAttributes, tag);

            foreach (Match att in atts)
            {
                if (!att.Value.Contains(@"="))
                {
                    if (!Attributes.ContainsKey(att.Value))
                        Attributes.Add(att.Value.ToLower(), string.Empty);
                }
                else
                {
                    //Extract attribute and value
                    string[] chunks = new string[2];
                    chunks[0] = att.Value.Substring(0, att.Value.IndexOf('='));
                    chunks[1] = att.Value.Substring(att.Value.IndexOf('=') + 1);

                    string attname = chunks[0].Trim();
                    string attvalue = chunks[1].Trim();

                    if (attvalue.StartsWith("\"") && attvalue.EndsWith("\"") && attvalue.Length > 2)
                    {
                        attvalue = attvalue.Substring(1, attvalue.Length - 2);
                    }

                    if (!Attributes.ContainsKey(attname))
                        Attributes.Add(attname, attvalue);
                }
            }
        }


        #region Props

        /// <summary>
        /// Gets the dictionary of attributes in the tag
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }


        /// <summary>
        /// Gets the name of this tag
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
        }

        /// <summary>
        /// Gets if the tag is actually a closing tag
        /// </summary>
        public bool IsClosing
        {
            get { return _isClosing; }
        }

        /// <summary>
        /// Gets if the tag is single placed; in other words it doesn't need a closing tag; 
        /// e.g. &lt;br&gt;
        /// </summary>
        public bool IsSingle
        {
            get
            {
                return TagName.StartsWith("!")
                    || (new List<string>(
                            new string[]{
                             "area", "base", "basefont", "br", "col",
                             "frame", "hr", "img", "input", "isindex",
                             "link", "meta", "param"
                            }
                        )).Contains(TagName)
                    ;
            }
        }

        internal void TranslateAttributes(CssBox box)
        {
            string t = TagName.ToUpper();

            foreach (string att in Attributes.Keys)
            {
                string value = Attributes[att];

                switch (att)
                {
                    case HtmlConstants.Align:
                        if (value == HtmlConstants.Left || value == HtmlConstants.Center || value == HtmlConstants.Right || value == HtmlConstants.Justify)
                            box.TextAlign = value;
                        else
                            box.VerticalAlign = value;
                        break;
                    case HtmlConstants.Background:
                            box.BackgroundImage = value;
                        break;
                    case HtmlConstants.Bgcolor:
                        box.BackgroundColor = value;
                        break;
                    case HtmlConstants.Border:
                        box.BorderWidth = TranslateLength(value);
                        
                        if (t == HtmlConstants.Table)
                        {
                            ApplyTableBorder(box, value);
                        }
                        else
                        {
                            box.BorderStyle = CssConstants.Solid;
                        }
                        break;
                    case HtmlConstants.Bordercolor:
                        box.BorderColor = value;
                        break;
                    case HtmlConstants.Cellspacing:
                        box.BorderSpacing = TranslateLength(value);
                        break;
                    case HtmlConstants.Cellpadding:
                        ApplyTablePadding(box, value);
                        break;
                    case HtmlConstants.Color:
                        box.Color = value;
                        break;
                    case HtmlConstants.Dir:
                        box.Direction = value;
                        break;
                    case HtmlConstants.Face:
                        box.FontFamily = value;
                        break;
                    case HtmlConstants.Height:
                        box.Height = TranslateLength(value);
                        break;
                    case HtmlConstants.Hspace:
                        box.MarginRight = box.MarginLeft = TranslateLength(value);
                        break;
                    case HtmlConstants.Nowrap:
                        box.WhiteSpace = CssConstants.Nowrap;
                        break;
                    case HtmlConstants.Size:
                        if (t == HtmlConstants.Hr)
                            box.Height = TranslateLength(value);
                        break;
                    case HtmlConstants.Valign:
                        box.VerticalAlign = value;
                        break;
                    case HtmlConstants.Vspace:
                        box.MarginTop = box.MarginBottom = TranslateLength(value);
                        break;
                    case HtmlConstants.Width:
                        box.Width = TranslateLength(value);
                        break;

                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts an HTML length into a Css length
        /// </summary>
        /// <param name="htmlLength"></param>
        /// <returns></returns>
        private string TranslateLength(string htmlLength)
        {
            CssLength len = new CssLength(htmlLength);

            if (len.HasError)
            {
                return htmlLength + "px";
            }

            return htmlLength;
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="border"></param>
        private void ApplyTableBorder(CssBox table, string border)
        {
            foreach (CssBox box in table.Boxes)
            {
                foreach (CssBox cell in box.Boxes)
                {
                    cell.BorderWidth = TranslateLength(border);
                }
            }
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="border"></param>
        private void ApplyTablePadding(CssBox table, string padding)
        {
            foreach (CssBox box in table.Boxes)
            {
                foreach (CssBox cell in box.Boxes)
                {
                    cell.Padding = TranslateLength(padding);

                }
            }
        }

        /// <summary>
        /// Gets a boolean indicating if the attribute list has the specified attribute
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool HasAttribute(string attribute)
        {
            return Attributes.ContainsKey(attribute);
        }

        public override string ToString()
        {
            return string.Format("<{1}{0}>", TagName, IsClosing ? "/" : string.Empty);
        }

        #endregion
    }
}
