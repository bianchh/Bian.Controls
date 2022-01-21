using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bian.Controls
{
    /// <summary>
    /// 带有 PlaceHolder 的 Textbox
    /// </summary>
    public class PlaceHolderTextBox : TextBox
    {
        /* 旧版本
        private bool _isPlaceHolder = true;
        private string _placeHolderText;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PlaceHolderTextBox()
        {
            GotFocus += RemovePlaceHolder;
            LostFocus += SetPlaceHolder;
        }

        #region --属性--
        public string PlaceHolderText
        {
            get { return _placeHolderText; }
            set
            {
                _placeHolderText = value;
                SetPlaceHolder();
            }
        }

        public new string Text
        {
            get
            {
                return _isPlaceHolder ? string.Empty : base.Text;
            }
            set
            {
                base.Text = value;
            }
        }
        #endregion


        private void SetPlaceHolder()
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                base.Text = PlaceHolderText;
                this.ForeColor = Color.Gray;
                this.Font = new Font(this.Font, FontStyle.Italic);
                _isPlaceHolder = true;
            }
        }

        private void RemovePlaceHolder()
        {
            if (_isPlaceHolder)
            {
                base.Text = "";
                this.ForeColor = SystemColors.WindowText;
                this.Font = new Font(this.Font, FontStyle.Regular);
                _isPlaceHolder = false;
            }
        }

        private void SetPlaceHolder(object sender, EventArgs e)
        {
            SetPlaceHolder();
        }

        private void RemovePlaceHolder(object sender, EventArgs e)
        {
            RemovePlaceHolder();
        }
        */

        private string _placeholderText;
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                base.Invalidate();
            }
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0xF || m.Msg == 0x133)
            {
                WmPaint(ref m);
            }
        }

        private void WmPaint(ref Message m)
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            if (!String.IsNullOrEmpty(this._placeholderText) && string.IsNullOrEmpty(this.Text))
            {
                g.DrawString(this._placeholderText, new Font(this.Font, FontStyle.Italic), new SolidBrush(Color.LightGray), 0, 0);
            }
        }
    }
}
