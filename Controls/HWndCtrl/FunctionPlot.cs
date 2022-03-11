﻿using HalconDotNet;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bian.Controls.HWndCtrl
{
    /// <summary>
    /// 绘制灰度图类
    /// </summary>
    public class FunctionPlot
    {
        #region --字段--
        // 绘制对象
        private Graphics gPanel, backBuffer;

        // 绘制设定
        private Pen pen, penCurve, penCursor;
        private SolidBrush brushCS, brushFuncPanel;
        private Font drawFont;
        private StringFormat format;
        private Bitmap functionMap;

        Control panel;

        // 区域参数设定
        private float panelWidth;
        private float panelHeight;
        private float margin;

        // 原点
        private float originX;
        private float originY;

        private PointF[] points;
        private HFunction1D func;

        // axis
        private int axisAdaption;
        private float axisXLength;
        private float axisYLength;
        private float scaleX, scaleY;

        // 显示模式
        public const int AXIS_RANGE_FIXED = 3;
        public const int AXIS_RANGE_INCREASING = 4;
        public const int AXIS_RANGE_ADAPTING = 5;

        int PreX, BorderRight, BorderTop;
        #endregion

        #region 属性
        public string XName { get; set; }
        public string YName { get; set; }
        #endregion

        public FunctionPlot(Control panel, bool useMouseHandle)
        {
            gPanel = panel.CreateGraphics();
            this.panel = panel;
            panelWidth = panel.Width - 32;
            panelHeight = panel.Height - 22;

            originX = 32;
            originY = panel.Size.Height - 22;
            margin = 5.0f;

            BorderRight = (int)(panelWidth + originX - margin);
            BorderTop = (int)panelHeight;

            PreX = 0;
            scaleX = scaleY = 0.0f;

            axisAdaption = AXIS_RANGE_FIXED;
            axisXLength = 10.0f;
            axisYLength = 10.0f;

            pen = new Pen(Color.DarkGray, 1);
            penCurve = new Pen(Color.Blue, 1);
            penCursor = new Pen(Color.LightSteelBlue, 1);
            penCursor.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            brushCS = new SolidBrush(Color.Black);
            brushFuncPanel = new SolidBrush(Color.White);
            drawFont = new Font("Arial", 6);
            format = new StringFormat();
            format.Alignment = StringAlignment.Far;

            functionMap = new Bitmap(panel.Size.Width, panel.Size.Height);
            backBuffer = Graphics.FromImage(functionMap);

            XName = "X";
            YName = "Y";
            ResetPlot();

            panel.Paint += new System.Windows.Forms.PaintEventHandler(this.paint);

            if (useMouseHandle)
            {
                panel.MouseMove += new MouseEventHandler(this.panel_MouseMove);
            }
        }

        public void SetLabel(string x, string y)
        {
            XName = x;
            YName = y;
        }

        /// <summary>
        /// 构造函数，默认不响应鼠标移动
        /// </summary>
        /// <param name="panel"></param>
        public FunctionPlot(Control panel) : this(panel, false)
        {

        }

        /// <summary>
        /// 改变坐标原点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetOrign(int x, int y)
        {
            float tmpX;

            if (x < 1 || y < 1)
            {
                return;
            }

            tmpX = originX;
            originX = x;
            originY = y;

            panelWidth += tmpX - originX;
            panelHeight = originY;
            BorderRight = (int)(panelWidth + originX - margin);
            BorderTop = (int)panelHeight;
        }

        /// <summary>
        /// 设置Y轴数据缩放模式
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="val"></param>
        public void SetAxisAdaption(int mode, float val)
        {
            switch (mode)
            {
                case AXIS_RANGE_FIXED:
                    axisAdaption = mode;
                    axisYLength = (val > 0) ? val : 255.0f;
                    break;
                default:
                    axisAdaption = mode;
                    break;
            }
        }

        /// <summary>
        /// 设置Y轴数据缩放模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetAxisAdaption(int mode)
        {
            SetAxisAdaption(mode, -1.0f);
        }

        /// <summary>
        /// 依据数组数据绘制曲线
        /// </summary>
        /// <param name="tuple"></param>
        public void DrawFunction(HTuple tuple)
        {
            HTuple val;
            int maxY, maxX;
            float stepOffset;

            if (tuple == null)
            {
                return;
            }
            if (tuple.Length == 0)
            {
                ResetPlot();
                return;
            }
            // 排序
            val = tuple.TupleSortIndex();
            // 获取最大值
            maxX = tuple.Length - 1;
            maxY = (int)tuple[val[val.Length - 1].I].D;

            axisXLength = maxX;

            switch (axisAdaption)
            {
                case AXIS_RANGE_ADAPTING:
                    axisYLength = maxY;
                    break;
                case AXIS_RANGE_INCREASING:
                    axisYLength = (maxY > axisYLength) ? maxY : axisYLength;
                    break;
            }

            //坐标区域背景
            backBuffer.Clear(System.Drawing.Color.WhiteSmoke);
            backBuffer.FillRectangle(brushFuncPanel, originX, 0, panelWidth, panelHeight);
            //绘制坐标轴及标签
            stepOffset = DrawXYLabels();
            //依据数据绘制灰度曲线
            DrawLineCurve(tuple, stepOffset);
            backBuffer.Flush();

            gPanel.DrawImageUnscaled(functionMap, 0, 0);
            gPanel.Flush();
        }

        /// <summary>
        /// 清除绘图区域并只显示坐标系
        /// </summary>
        public void ResetPlot()
        {
            backBuffer.Clear(Color.WhiteSmoke);
            backBuffer.FillRectangle(brushFuncPanel, originX, 0, panelWidth, panelHeight);
            func = null;
            DrawXYLabels();
            backBuffer.Flush();
            Repaint();
        }

        /// <summary>
        /// 重绘图像
        /// </summary>
        private void Repaint()
        {
            gPanel.DrawImageUnscaled(functionMap, 0, 0);
            gPanel.Flush();
        }

        /// <summary>
        /// 绘制函数点
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="stepOffset"></param>
        private void DrawLineCurve(HTuple tuple, float stepOffset)
        {
            int length;

            if (stepOffset > 1)
            {
                points = ScaleDispValue(tuple);
            }
            else
            {
                points = ScaleDispBlockValue(tuple);
            }

            length = points.Length;

            func = new HFunction1D(tuple);

            for (int i = 0; i < length - 1; i++)
            {
                backBuffer.DrawLine(penCurve, points[i], points[i + 1]);
            }
        }

        /// <summary>
        /// Scales the function to the dimension of the graphics object 
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private PointF[] ScaleDispValue(HTuple tuple)
        {
            PointF[] pVals;
            float xMax, yMax, yV, x, y;
            int length;

            xMax = axisXLength;
            yMax = axisYLength;

            scaleX = (xMax != 0.0f) ? ((panelWidth - margin) / xMax) : 0.0f;
            scaleY = (yMax != 0.0f) ? ((panelHeight - margin) / yMax) : 0.0f;

            length = tuple.Length;
            pVals = new PointF[length];

            for (int i = 0; i < length; i++)
            {
                yV = tuple[i].F;
                x = originX + i * scaleX;
                y = panelHeight - (yV * scaleY);
                pVals[i] = new PointF(x, y);
            }

            return pVals;
        }

        /// <summary>
        /// Scales the function to the dimension of the graphics object 
        /// (provided by the control). If the stepsize  for the x-axis is
        /// 1, the points are scaled in a block shape.
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private PointF[] ScaleDispBlockValue(HTuple tuple)
        {
            PointF[] pVals;
            float xMax, yMax, yV, x, y;
            int length, idx;

            yMax = axisXLength;
            xMax = axisYLength;

            scaleX = (xMax != 0.0f) ? ((panelWidth - margin) / xMax) : 0.0f;
            scaleY = (yMax != 0.0f) ? ((panelHeight - margin) / yMax) : 0.0f;

            length = tuple.Length;
            pVals = new PointF[length * 2];

            y = 0;
            idx = 0;

            for (int i = 0; i < length; i++)
            {
                yV = tuple[i].F;
                x = originX + i * scaleX - (scaleX / 2.0f);
                y = panelHeight - (yV * scaleY);
                pVals[idx] = new PointF(x, y);
                idx++;

                x = originX + i * scaleX + (scaleX / 2.0f);
                pVals[idx] = new PointF(x, y);
                idx++;
            }

            idx--;
            x = originX + (length - 1) * scaleX;
            pVals[idx] = new PointF(x, y);

            idx = 0;
            yV = tuple[idx].F;
            x = originX;
            y = panelHeight - (yV * scaleY);

            return pVals;

        }


        /// <summary>坐标轴和标签绘制</summary>
        /// <returns>x轴的步长</returns>
        private float DrawXYLabels()
        {
            float pX, pY, length, XStart, YStart;
            float YCoord, XCoord, XEnd, YEnd, offset, offsetString, offsetStep;
            float scale = 0.0f;

            offsetString = 5;
            XStart = originX;
            YStart = originY;

            //prepare scale data for x-axis
            pX = axisXLength;
            if (pX != 0.0)
                scale = (panelWidth - margin) / pX;

            if (scale > 10.0)
                offset = 1.0f;
            else if (scale > 2)
                offset = 10.0f;
            else if (scale > 0.2)
                offset = 100.0f;
            else
                offset = 1000.0f;


            /***************   draw X-Axis   ***************/
            XCoord = 0.0f;
            YCoord = YStart;
            XEnd = (scale * pX);

            backBuffer.DrawLine(pen, XStart, YStart, XStart + panelWidth - margin, YStart);
            backBuffer.DrawLine(pen, XStart + XCoord, YCoord, XStart + XCoord, YCoord + 6);
            backBuffer.DrawString(0 + "", drawFont, brushCS, XStart + XCoord + 4, YCoord + 8, format);
            backBuffer.DrawLine(pen, XStart + XEnd, YCoord, XStart + XEnd, YCoord + 6);
            backBuffer.DrawString(((int)pX + ""), drawFont, brushCS, XStart + XEnd + 4, YCoord + 8, format);

            length = (int)(pX / offset);
            length = (offset == 10) ? length - 1 : length;
            for (int i = 1; i <= length; i++)
            {
                XCoord = offset * i * scale;

                if ((XEnd - XCoord) < 20)
                    continue;

                backBuffer.DrawLine(pen, XStart + XCoord, YCoord, XStart + XCoord, YCoord + 6);
                backBuffer.DrawString(((int)(i * offset) + ""), drawFont, brushCS, XStart + XCoord + 5, YCoord + 8, format);
            }

            offsetStep = offset;

            //prepare scale data for y-axis
            pY = axisYLength;
            if (pY != 0.0)
                scale = ((panelHeight - margin) / pY);

            if (scale > 10.0)
                offset = 1.0f;
            else if (scale > 2)
                offset = 10.0f;
            else if (scale > 0.8)
                offset = 50.0f;
            else if (scale > 0.12)
                offset = 100.0f;
            else
                offset = 1000.0f;

            /***************    draw Y-Axis   ***************/
            XCoord = XStart;
            YCoord = 5.0f;
            YEnd = YStart - (scale * pY);

            backBuffer.DrawLine(pen, XStart, YStart, XStart, YStart - (panelHeight - margin));
            backBuffer.DrawLine(pen, XCoord, YStart, XCoord - 10, YStart);
            backBuffer.DrawString(0 + "", drawFont, brushCS, XCoord - 12, YStart - offsetString, format);
            backBuffer.DrawLine(pen, XCoord, YEnd, XCoord - 10, YEnd);
            backBuffer.DrawString(pY + "", drawFont, brushCS, XCoord - 12, YEnd - offsetString, format);

            length = (int)(pY / offset);
            length = (offset == 10) ? length - 1 : length;
            for (int i = 1; i <= length; i++)
            {
                YCoord = (YStart - offset * i * scale);

                if ((YCoord - YEnd) < 10)
                    continue;
                //y轴数据太多就不绘制
                if (length > 10 && i % 5 != 0)
                {
                    continue;
                }
                backBuffer.DrawLine(pen, XCoord, YCoord, XCoord - 10, YCoord);
                backBuffer.DrawString(((int)(i * offset) + ""), drawFont, brushCS, XCoord - 12, YCoord - offsetString, format);
            }

            return offsetStep;
        }


        /// <summary>
        /// Action call for the Mouse-Move event. For the x-coordinate
        /// supplied by the MouseEvent, the unscaled x and y coordinates
        /// of the plotted function are determined and displayed 
        /// on the control.
        /// </summary>
        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            int Xh, Xc;
            HTuple Ytup;
            float Yh, Yc;

            Xh = e.X;

            if (PreX == Xh || Xh < originX || Xh > BorderRight || func == null)
                return;

            PreX = Xh;

            Xc = (int)Math.Round((Xh - originX) / scaleX);
            Ytup = func.GetYValueFunct1d(new HTuple(Xc), "zero");

            Yc = (float)Ytup[0].D;
            Yh = panelHeight - (Yc * scaleY);

            BufferedGraphicsContext ctx = BufferedGraphicsManager.Current;
            BufferedGraphics bg = ctx.Allocate(gPanel, new Rectangle(new Point(0, 0), panel.Size));
            Graphics gOfbuff = bg.Graphics;
            Bitmap toDrawBeffor = new Bitmap(panel.Width, panel.Height);
            Graphics gOfToDrawBeffor = Graphics.FromImage(toDrawBeffor);
            // 自定义绘图
            gOfToDrawBeffor.DrawLine(Pens.Blue, 0, 0, 1000, 1000);

            gOfToDrawBeffor.DrawImageUnscaled(functionMap, 0, 0);
            gOfToDrawBeffor.DrawLine(penCursor, Xh, 0, Xh, BorderTop);
            gOfToDrawBeffor.DrawLine(penCursor, originX, Yh, BorderRight + margin, Yh);
            string xStr = string.Format("{0}={1}", XName, Xc);
            string yStr = string.Format("{0}={1}", YName, (int)Yc);
            gOfToDrawBeffor.DrawString(xStr, drawFont, brushCS, panelWidth - margin * 4, 10);
            gOfToDrawBeffor.DrawString(yStr, drawFont, brushCS, panelWidth - margin * 4, 20);

            //双缓存到panel上
            gOfbuff.Clear(panel.BackColor);
            gOfbuff.DrawImage(toDrawBeffor, 0, 0);
            bg.Render();
            toDrawBeffor.Dispose();
            gOfToDrawBeffor.Dispose();
            gOfbuff.Dispose();
            ctx.Dispose();
            bg.Dispose();
        }


        /// <summary>
        /// Action call for the Paint event of the control to trigger the
        /// repainting of the function plot. 
        /// </summary>
        private void paint(object sender, PaintEventArgs e)
        {
            Repaint();
        }
    }
}
