using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Bian.Controls
{
    [DefaultEvent("SwitchChanged")]
    public class SlideSwitch : CheckBox
    {
        public event EventHandler SwitchChanged;
        /// <summary>
        /// 构造函数
        /// </summary>
        public SlideSwitch()
        {
            AutoSize = false;
            Paint += On_Paint;
            CheckedChanged += On_CheckChanged;
        }

        #region --属性
        public bool IsOpen
        {
            get { return base.Checked; }
            set { base.Checked = value; }
        }

        private bool _showState = true;
        public bool ShowState
        {
            get { return _showState; }
            set
            {
                _showState = value;
                base.Invalidate();
            }
        }

        #endregion

        #region --事件--
        private void On_Paint(object sender, PaintEventArgs e)
        {
            SlideSwitchButton(sender, e);
        }

        private void On_CheckChanged(object sender, EventArgs e)
        {
            SwitchChanged?.Invoke(this, new EventArgs());
        }
        #endregion

        #region --方法--
        private void SlideSwitchButton(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            Rectangle RoundRect = new Rectangle(0, 0, base.Width, base.Height);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Font f = new Font("Consolas", 12, FontStyle.Bold);

            if (base.Checked)
            {
                Color color = Color.FromArgb(55, 197, 90);
                Brush b1 = new SolidBrush(color);
                FillRoundRectangle(g, b1, RoundRect, Height / 2);

                using (Pen pen = new Pen(Color.FromArgb(255, 255, 255)))
                {
                    FillRoundRectangle(g, Brushes.White, new Rectangle(Width - Height + 2, 2, Height - 4, Height - 4), (Height - 4) / 2);
                }
                if (_showState)
                {
                    SizeF size = GetFontSize(ref f, "ON");
                    g.DrawString("ON", f, Brushes.White, new PointF(((Width - Height - size.Width) / 2) * 1.13f, (Height - size.Height) / 2));
                }
            }
            else
            {
                using (Pen pen = new Pen(Color.FromArgb(255, 255, 255)))
                {
                    FillRoundRectangle(g, Brushes.Silver, RoundRect, Height / 2);
                    FillRoundRectangle(g, Brushes.White, new Rectangle(2, 2, Height - 4, Height - 4), (Height - 4) / 2);
                }
                if (_showState)
                {
                    SizeF size = GetFontSize(ref f, "OFF");
                    g.DrawString("OFF", f, Brushes.Black, new PointF(((Width - Height - size.Width) / 2) * 0.9f + Height, (Height - size.Height) / 2));
                }
            }

        }

        /// <summary>
        /// 填充圆角矩形
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        /// <param name="cornerRadius"></param>
        private static void FillRoundRectangle(Graphics g, Brush brush, Rectangle rect, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
        /// <summary>
        /// 根据字体、文本计算文本的像素尺寸
        /// </summary>
        /// <param name="P_String"></param>
        /// <returns></returns>
        /// 
        private SizeF GetTextBounds(Font font, string text)
        {
            Bitmap bmp = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SizeF size = g.MeasureString(text, font);
                return size;
            }
        }

        private SizeF GetFontSize(ref Font font, string text)
        {
            float fontSize = 1f;
            SizeF sizef;
            do
            {
                font = new Font(font.FontFamily, fontSize, font.Style);
                sizef = GetTextBounds(font, text);
                fontSize += 0.1f;
            }
            while (sizef.Width < (Width - Height) * 2 / 3 && sizef.Height < Height * 2 / 3);
            return sizef;
        }

        #endregion
    }
}
