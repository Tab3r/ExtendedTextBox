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

        private double _minDecimalValue;
        private double _maxDecimalValue;

        private int _minLengthStringValue;
        private int _maxLengthStringValue;

        #region Constructor

        public ExtendedTextBox()
        {
            //InitializeComponent();

            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);

            // Initialize the regex expresions
            string separator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            this._regexDecimal = "^[+-]?[0-9]+[" + separator + "]?[0-9]*?$";
            this._regexInteger = "^[+-]?[0-9]+$";
            this._regexString = @"\w+";
            this._regexCustom = "^.+$";
            
            // Initialize max/min value for integer/decimal
            this._minDecimalValue = float.NaN;
            this._maxDecimalValue = float.NaN;

            // Initialize max/min lenght for string
            this._minLengthStringValue = -1;
            this._maxLengthStringValue = -1;

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

        #endregion Constructor

        #region Propierties

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
        [Description("Get or set minimum value for a number. Only used with type DECIMAL/INTEGER.")]
        [DisplayName("Min. Number Value")]
        public double MinNumberValue
        {
            get
            {
                return this._minDecimalValue;
            }
            set
            {
                if (this._maxDecimalValue < value)
                    return;
                this._minDecimalValue = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set maximum value for a number. Only used with type DECIMAL/INTEGER.")]
        [DisplayName("Max. Number Value")]
        public double MaxNumberValue
        {
            get
            {
                return this._maxDecimalValue;
            }
            set
            {
                if (this._minDecimalValue > value)
                    return;
                this._maxDecimalValue = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set minimum length for a string. Only used with type STRING.")]
        [DisplayName("Min. String Length")]
        public int MinStringLength
        {
            get
            {
                return this._minLengthStringValue;
            }
            set
            {
                // Check for correct value
                if (value < 0)
                    return;
                // Don't allow min length upper to max length
                if (this._maxLengthStringValue < value)
                    return;

                this._minLengthStringValue = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set maximum length for a string. Only used with type STRING.")]
        [DisplayName("Max. String Length")]
        public int MaxStringLength
        {
            get
            {
                return this._maxLengthStringValue;
            }
            set
            {
                // Check for correct value
                if (value < 0)
                    return;
                // Don't allow max length lower to min length
                if (this._minLengthStringValue > value)
                    return;

                this._maxLengthStringValue = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set the string used with Regex. Only used if CUSTOM type is in use")]
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

        #endregion Propierties

        #region Events

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (this.isValidText)
                this.BackColor = Color.LightGreen;
            else
                this.BackColor = Color.LightSalmon;
        }

        #endregion Events

        #region Private functions

        public bool isValidText
        {
            get
            {

                bool ok = true;

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

                ok &= System.Text.RegularExpressions.Regex.IsMatch(this.Text, regex);

                if (!ok)
                    return ok;

                switch (this._typedata)
                {
                    case TYPE_DATA.DECIMAL:
                        double aux = double.Parse(this.Text);
                        if (aux > this._maxDecimalValue || aux < this._minDecimalValue)
                            ok = false;
                        break;
                    case TYPE_DATA.INTEGER:
                        aux = double.Parse(this.Text);
                        if (aux > this._maxDecimalValue || aux < this._minDecimalValue)
                            ok = false;
                        break;
                    case TYPE_DATA.STRING:
                        aux = this.Text.Length;
                        if (aux > this._maxLengthStringValue || aux < this._minLengthStringValue)
                            ok = false;
                        break;
                }

                return ok;
            }
        }

        #endregion Private functions
    }
}
