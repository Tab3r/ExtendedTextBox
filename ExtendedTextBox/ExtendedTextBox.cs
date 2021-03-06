﻿/*
Copyright (C) 2011 by INCLAM S.A. (http://www.inclam.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Inclam.Controls
{
    /// <summary>
    /// Type of data that can be validated
    /// </summary>
    public enum TYPE_DATA
    {
        /// <summary>
        /// Custom, it needs a custom Regex
        /// </summary>
        CUSTOM,
        /// <summary>
        /// Integer without decimal
        /// </summary>
        INTEGER,
        /// <summary>
        /// Decimal, that will be parsed with "float.Parse" or "double.Parse"
        /// </summary>
        DECIMAL,
        /// <summary>
        /// Decimal, more restrictive only "X" or "XXX.XXX" is allowed
        /// </summary>
        DECIMAL_RESTRICTIVE,
        /// <summary>
        /// Percent number
        /// </summary>
        PERCENT,
        /// <summary>
        /// String
        /// </summary>
        STRING,
        /// <summary>
        /// Email
        /// </summary>
        EMAIL,
        /// <summary>
        /// URL
        /// </summary>
        URL
    }

    public delegate bool dTextValidation(string text);

    public class ExtendedTextBox : TextBox
    {

        #region Variables

        // Type of data to validate
        TYPE_DATA _typedata = TYPE_DATA.CUSTOM;

        // Regex
        private string _regexDecimal;
        private string _regexDecimalMoreRestrictive;
        private string _regexInteger;
        private string _regexString;
        private string _regexCustom;
        private string _regexEmail;
        private string _regexURL;
        private string _regexPercent;

        // Limits
        private double _minDecimalValue;
        private double _maxDecimalValue;
        private int _maxNumberDecimalDigits;
        private int _minLengthStringValue;
        private int _maxLengthStringValue;

        // Textbox Propierties
        private bool _isValid;
        private Color _backcolorOK;
        private Color _backcolorERROR;
        private bool _emptyIsValid;

        #endregion Variables

        #region Delegates

        // Delegates
        //  Useful for testing more complex regexs checks
        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set a delegate to function to other validate")]
        [DisplayName("Other validations")]
        public event dTextValidation OtherTextValidation;

        #endregion Delegates

        #region Constructor

        public ExtendedTextBox()
        {
            //InitializeComponent();

            // Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);

            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);

            // Initialize the regex expresions
            string separator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;                // Its ok: "0.1", ".1" or "0."
            this._regexDecimal = "^[+-]?[0-9]+[" + separator + "]?[0-9]*?$|^[+-]?[0-9]*[" + separator + "]?[0-9]+?$";   // Its ok: "numberSEPARATORnumber" or "number"
            this._regexDecimalMoreRestrictive = "^[+-]?[0-9]+[" + separator + "]?[0-9]+?$";
            this._regexInteger = "^[+-]?[0-9]+$";
            this._regexString = @"\w+";
            this._regexEmail = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"; // From here: http://www.cambiaresearch.com/c4/bf974b23-484b-41c3-b331-0bd8121d5177/Parsing-Email-Addresses-with-Regular-Expressions.aspx
            this._regexURL = @"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))"; // http://daringfireball.net/2010/07/improved_regex_for_matching_urls
            this._regexCustom = "^.+$"; // use this website to check: http://derekslager.com/blog/posts/2007/09/a-better-dotnet-regular-expression-tester.ashx
            this._regexPercent = "^[+-]?[0-9]+[" + separator + "]?[0-9]*?%?$|^[+-]?[0-9]*[" + separator + "]?[0-9]+?%?$";
            
            // Initialize max/min value for integer/decimal
            this._minDecimalValue = float.NaN;
            this._maxDecimalValue = float.NaN;
            this._maxNumberDecimalDigits = -1;

            // Initialize max/min lenght for string
            this._minLengthStringValue = -1;
            this._maxLengthStringValue = -1;

            // Colors
            this._backcolorOK = Color.LightGreen;
            this._backcolorERROR = Color.LightSalmon;

            // Delegate to other validation
            this.OtherTextValidation = null;

            // Launch first
            this.OnTextChanged(null);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message for flickering
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        #endregion Constructor

        #region Propierties

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set if empty value is valid")]
        [DisplayName("Empty is valid")]
        public bool EmptyIsValid
        {
            get { return this._emptyIsValid; }
            set { this._emptyIsValid = value; }
        }

        [Browsable(false)]
        [Description("Get is the text is valid for the type of data")]
        public bool IsValid
        {
            get
            {
                return this._isValid;
            }
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

                //  the "traditional way" of showing number ...
                if (this._typedata == TYPE_DATA.DECIMAL || this._typedata == TYPE_DATA.DECIMAL_RESTRICTIVE ||
                    this._typedata == TYPE_DATA.INTEGER || this._typedata == TYPE_DATA.PERCENT)
                    this.TextAlign = HorizontalAlignment.Right;
                //  the "traditional way" of showing number ...
                else if (this._typedata == TYPE_DATA.CUSTOM || this._typedata == TYPE_DATA.URL ||
                         this._typedata == TYPE_DATA.EMAIL)
                    this.TextAlign = HorizontalAlignment.Left;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set backcolor of textbox when is ok")]
        [DisplayName("Custom BackColor OK")]
        public Color BackColorOk
        {
            get
            {
                return this._backcolorOK;
            }
            set
            {
                this._backcolorOK = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set backcolor of textbox when is wrong")]
        [DisplayName("Custom BackColor ERROR")]
        public Color BackColorERROR
        {
            get
            {
                return this._backcolorERROR;
            }
            set
            {
                this._backcolorERROR = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set minimum value for a number. Only used with type DECIMAL/INTEGER. The string \"NeuN\" means no limit.")]
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
        [Description("Get or set maximum value for a number. Only used with type DECIMAL/INTEGER. The string \"NeuN\" means no limit.")]
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
        [Description("Get or set maximum number of decimal digits in number value. Only used with type DECIMAL. The -1 means infinite.")]
        [DisplayName("Max. Number Decimal Digits")]
        public int MaxNumberDecimalDigits
        {
            get
            {
                return this._maxNumberDecimalDigits;
            }
            set
            {
                this._maxNumberDecimalDigits = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set minimum length for a string. Only used with type STRING. The value -1 means no restrictions.")]
        [DisplayName("Min. String Length")]
        public int MinStringLength
        {
            get
            {
                return this._minLengthStringValue;
            }
            set
            {
                // -1 is always valid
                if (value != -1)
                {
                    // Check for correct value
                    if (value < 0)
                        return;
                    // Don't allow min length upper to max length
                    if (this._maxLengthStringValue < value)
                        return;
                }
                this._minLengthStringValue = value;
            }
        }

        [Browsable(true)]
        [Category("Extension")]
        [Description("Get or set maximum length for a string. Only used with type STRING. The value -1 means no restrictions.")]
        [DisplayName("Max. String Length")]
        public int MaxStringLength
        {
            get
            {
                return this._maxLengthStringValue;
            }
            set
            {
                // -1 is always valid
                if (value != -1)
                {
                    // Check for correct value
                    if (value < 0)
                        return;
                    // Don't allow max length lower to min length
                    if (this._minLengthStringValue > value)
                        return;
                }
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
                // This isn't a perfect solution but...
                if (this.isValidRegex(value))
                {
                    this._regexCustom = value;
                }
            }
        }

        #endregion Propierties

        #region Events

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.isValidText)
            {
                this._isValid = true;
                this.BackColor = this._backcolorOK;

                // Check max number of decimal digits
                if ((this._typedata ==  TYPE_DATA.DECIMAL || this._typedata == TYPE_DATA.DECIMAL_RESTRICTIVE) 
                     && this._maxNumberDecimalDigits > -1)
                {
                    // I found the idea here: http://stackoverflow.com/questions/2453951/c-double-tostring-formatting-with-two-decimal-places-but-no-rounding
                    double tmp = double.Parse(this.Text);
                    string sFormat = "{0:0.";
                    for (int i = 0; i < this._maxNumberDecimalDigits; i++)
                        sFormat += "#";
                    sFormat += "}";
                    string s = string.Format(System.Globalization.CultureInfo.CurrentCulture, sFormat, tmp);
                    this.Text = s;
                }

            }
            else
            {
                this._isValid = false;
                this.BackColor = this._backcolorERROR;
            }

            base.OnTextChanged(e);
        }

        #endregion Events

        #region Private functions

        /// <summary>
        /// <para>Check function with Regex and optional checks.</para>
        /// </summary>
        public bool isValidText
        {
            get
            {
                // First check: Is empty string?
                // -----------------------------
                if (this.Text == string.Empty)
                {
                    if (this._emptyIsValid)
                        return true;
                    else
                        return false;
                }

                // Second check: Is valid?
                // -----------------------
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
                    case TYPE_DATA.DECIMAL_RESTRICTIVE:
                        regex = this._regexDecimalMoreRestrictive;
                        break;
                    case TYPE_DATA.INTEGER:
                        regex = this._regexInteger;
                        break;
                    case TYPE_DATA.PERCENT:
                        regex = this._regexPercent;
                        break;
                    case TYPE_DATA.STRING:
                        regex = this._regexString;
                        break;
                    case TYPE_DATA.EMAIL:
                        regex = this._regexEmail;
                        break;
                    case TYPE_DATA.URL:
                        regex = this._regexURL;
                        break;
                }

                ok &= System.Text.RegularExpressions.Regex.IsMatch(this.Text, regex);

                // Regex mark as not valid -> exits with false
                if (!ok)
                    return ok;

                // Third check: Limits
                // -------------------
                switch (this._typedata)
                {
                    case TYPE_DATA.DECIMAL:
                    case TYPE_DATA.DECIMAL_RESTRICTIVE:
                    case TYPE_DATA.INTEGER:
                        double aux = double.Parse(this.Text);
                        if (this._maxDecimalValue != double.NaN && this._minDecimalValue != double.NaN)
                        {
                            if (aux > this._maxDecimalValue || aux < this._minDecimalValue)
                                ok = false;
                        }
                        else if (this._maxDecimalValue == double.NaN && this._minDecimalValue != double.NaN)
                        {
                            if (aux < this._minDecimalValue)
                                ok = false;
                        }
                        else if (this._maxDecimalValue != double.NaN && this._minDecimalValue == double.NaN)
                        {
                            if (aux > this._maxDecimalValue)
                                ok = false;
                        }
                        break;
                    case TYPE_DATA.PERCENT:
                        aux = double.Parse(this.Text.Replace("%", ""));
                        if (this._maxDecimalValue != double.NaN && this._minDecimalValue != double.NaN)
                        {
                            if (aux > this._maxDecimalValue || aux < this._minDecimalValue)
                                ok = false;
                        }
                        else if (this._maxDecimalValue == double.NaN && this._minDecimalValue != double.NaN)
                        {
                            if (aux < this._minDecimalValue)
                                ok = false;
                        }
                        else if (this._maxDecimalValue != double.NaN && this._minDecimalValue == double.NaN)
                        {
                            if (aux > this._maxDecimalValue)
                                ok = false;
                        }
                        break;
                    case TYPE_DATA.STRING:
                        aux = this.Text.Length;
                        // Checks, but considering the limiters are null (-1)
                        if (this._maxLengthStringValue != -1 && this._minLengthStringValue != -1)
                        {
                            if (aux > this._maxLengthStringValue || aux < this._minLengthStringValue)
                                ok = false;
                        }
                        else if (this._maxLengthStringValue == -1 && this._minLengthStringValue != -1)
                        {
                            if (aux < this._minLengthStringValue)
                                ok = false;
                        }
                        else if (this._maxLengthStringValue != -1 && this._minLengthStringValue == -1)
                        {
                            if (aux > this._maxLengthStringValue)
                                ok = false;
                        }
                        break;
                }


                // Fourth check: Delegate
                // ----------------------
                if (this.OtherTextValidation != null)
                {
                    ok &= this.OtherTextValidation(this.Text);
                }

                return ok;
            }
        }

        /// <summary>
        /// <para>Check patter for Regex object</para>
        /// </summary>
        /// <param name="pattern">String pattern</param>
        /// <returns>If it is valid</returns>
        public bool isValidRegex(string pattern)
        {
            bool isValid = true;

            // This is not a pattern
            if ((pattern == null) || (pattern == string.Empty) || (pattern.Trim() == string.Empty))
                return false;

            // do the trick... :)
            try
            {
                System.Text.RegularExpressions.Regex.Match("", pattern);
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        #endregion Private functions
    }
}
