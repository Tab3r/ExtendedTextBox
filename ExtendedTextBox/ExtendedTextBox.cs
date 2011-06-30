using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Inclam
{
    public enum TYPE_DATA
        {
            CUSTOM,
            INTEGER,
            DECIMAL,
            STRING
        }

    public class ExtendedTextBox : TextBox
    {
        
        TYPE_DATA _typedata = TYPE_DATA.CUSTOM;

        private string _regexDecimal;
        private string _regexInteger;
        private string _regexString;
        private string _regexCustom;

        public ExtendedTextBox()
        {
            //InitializeComponent();

            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);

            string separator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            this._regexDecimal = "^[+-]?[0-9]+[" + separator + "]?[0-9]*?$";
            this._regexInteger = "^[+-]?[0-9]+$";
            this._regexString = @"\w+";
            this._regexCustom = "^.+$";

            // Launch first
            this.OnTextChanged(null);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        public bool isValidText
        {
            get
            {
                string regex = string.Empty;

                switch (this._typedata)
                {
                    case TYPE_DATA.CUSTOM:
                        regex = this._regexCustom;
                        break;
                    case TYPE_DATA.DECIMAL:
                        regex = this._regexDecimal;
                        break;
                    case TYPE_DATA.INTEGER:
                        regex = this._regexInteger;
                        break;
                    case TYPE_DATA.STRING:
                        regex = this._regexString;
                        break;
                }

                return System.Text.RegularExpressions.Regex.IsMatch(this.Text, regex);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (this.isValidText)
                this.BackColor = Color.LightGreen;
            else
                this.BackColor = Color.LightSalmon;
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set type of data")]
        [DisplayName("Type of data")]
        public TYPE_DATA TypeOfData
        {
            get
            {
                return this._typedata;
            }
            set
            {
                this._typedata = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set type of data")]
        [DisplayName("Custom Regex")]
        public string CustomRegex
        {
            get
            {
                return this._regexCustom;
            }
            set
            {
                this._regexCustom = value;
            }
        }
    }
}
