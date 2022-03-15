using HalconDotNet;
using System;

namespace Bian.Controls.HWndCtrl
{
    /// <summary>
    /// 直线ROI
    /// </summary>
    public class ROILine : ROI
    {
        /// <summary>
        /// 起点行坐标
        /// </summary>
        public double StartRow { get; set; }
        /// <summary>
        /// 起点列坐标
        /// </summary>
        public double StartCol { get; set; }
        /// <summary>
        /// 终点行坐标
        /// </summary>
        public double EndRow { get; set; }
        /// <summary>
        /// 终点列坐标
        /// </summary>
        public double EndCol { get; set; }
        /// <summary>
        /// 中点行坐标
        /// </summary>
        public double MidRow { get; set; }
        /// <summary>
        /// 中点列坐标
        /// </summary>
        public double MidCol { get; set; }

        [NonSerialized]
        /// <summary>
        /// 直线上添加箭头显示
        /// </summary>
        private HXLDCont arrowHandleXLD;
        /// <summary>
        /// 直线ROI构造
        /// </summary>
        public ROILine()
        {
            //直线上的控制框个数
            NumHandles = 3;        // 端点2+中点1=3
            //默认激活的控制点为2--中点
            activeHandleIdx = 2;
            //直线箭头初始化
            arrowHandleXLD = new HXLDCont();
            arrowHandleXLD.GenEmptyObj();

            ROIType = ROIType.Line;
        }
        /// <summary>
        /// 依据起点与终点信息重构ROI
        /// </summary>
        /// <param name="roiDat">坐标信息</param>
        private void CreateROI(HTuple roiDat)
        {
            if (roiDat.Length == 4)
            {
                StartRow = roiDat[0].D;
                StartCol = roiDat[1].D;
                EndRow = roiDat[2].D;
                EndCol = roiDat[3].D;
                MidRow = (StartRow + EndRow) / 2.0;
                MidCol = (StartCol + EndCol) / 2.0;
                updateArrowHandle();
            }
        }

        public override void ReCreateROI()
        {
            updateArrowHandle();
        }
        /// <summary>在鼠标位置创建一个ROI</summary>
        public override void CreateROI(double midX, double midY)
        {
            int width = GetHandleWidth();

            //鼠标位置为直线中心
            MidRow = midY;
            MidCol = midX;

            StartRow = MidRow;
            StartCol = MidCol - width * 5;
            EndRow = MidRow;
            EndCol = MidCol + width * 5;

            updateArrowHandle();
        }

        /// <summary>绘制图形在窗口中</summary>
        public override void Draw(HWindow window)
        {
            //直线绘制
            window.DispLine(StartRow, StartCol, EndRow, EndCol);

            int width = GetHandleWidth();
            //直线起点的框
            window.DispRectangle2(StartRow, StartCol, 0, width, width);
            //直线终点的箭头
            window.DispObj(arrowHandleXLD);  //window.DispRectangle2( row2, col2, 0, 5, 5);
            //直线中点的框
            window.DispRectangle2(MidRow, MidCol, 0, width, width);

            //int r1, c1, r2, c2;
            //window.GetPart(out r1, out c1, out r2, out c2);
            //int width = r2 - r1;
            ////直线起点的框
            //window.DispRectangle2(startRow, startCol, 0, width / 50.0, width / 50.0);
            ////System.Diagnostics.Debug.WriteLine("{0}-{1}-{2}-{3}", r1, c1, r2, c2);
        }

        /// <summary>
        /// 求出鼠标坐标与ROI的最近控制点的距离
        /// </summary>
        /// <param name="x">鼠标坐标X</param>
        /// <param name="y">鼠标坐标Y</param>
        /// <returns>鼠标与ROI的控制框的最近距离值</returns>
        public override double DistToClosestHandle(double x, double y)
        {
            double max = 10000;
            double[] val = new double[NumHandles];

            val[0] = HMisc.DistancePp(y, x, StartRow, StartCol); // upper left 
            val[1] = HMisc.DistancePp(y, x, EndRow, EndCol); // upper right 
            val[2] = HMisc.DistancePp(y, x, MidRow, MidCol); // midpoint 

            for (int i = 0; i < NumHandles; i++)
            {
                if (val[i] < max)
                {
                    max = val[i];
                    activeHandleIdx = i;
                }
            }// end of for 

            return val[activeHandleIdx];
        }

        /// <summary> 
        /// 在窗体上显示激活的控制点
        /// </summary>
        public override void DisplayActive(HWindow window)
        {
            int width = GetHandleWidth();
            switch (activeHandleIdx)
            {
                case 0:
                    window.DispRectangle2(StartRow, StartCol, 0, width, width);
                    break;
                case 1:
                    window.DispObj(arrowHandleXLD); //window.DispRectangle2(row2, col2, 0, 5, 5);
                    break;
                case 2:
                    window.DispRectangle2(MidRow, MidCol, 0, width, width);
                    break;
            }
        }

        /// <summary>
        /// 获取region
        /// </summary>
        /// <returns>region图形</returns>
        public override HRegion GetRegion()
        {
            HRegion region = new HRegion();
            region.GenRegionLine(StartRow, StartCol, EndRow, EndCol);
            return region;
        }
        /// <summary>
        /// 获取坐标点到图像起始点的距离
        /// </summary>
        /// <param name="row">坐标行 Y</param>
        /// <param name="col">坐标列 X</param>
        /// <returns>距离值</returns>
        public override double GetDistanceFromStartPoint(double row, double col)
        {
            double distance = HMisc.DistancePp(row, col, StartRow, StartCol);
            return distance;
        }
        /// <summary>
        /// 获取region的坐标信息
        /// </summary> 
        public override HTuple GetModelData()
        {
            return new HTuple(new double[] { StartRow, StartCol, EndRow, EndCol });
        }

        /// <summary> 
        ///依据坐标点移动region
        /// </summary>
        public override void MoveByHandle(double newX, double newY)
        {
            double lenR, lenC;
            //起始点-终点-中点三种方式修改直线region
            switch (activeHandleIdx)
            {
                case 0: // first end point
                    StartRow = newY;
                    StartCol = newX;

                    MidRow = (StartRow + EndRow) / 2;
                    MidCol = (StartCol + EndCol) / 2;
                    break;
                case 1: // last end point
                    EndRow = newY;
                    EndCol = newX;

                    MidRow = (StartRow + EndRow) / 2;
                    MidCol = (StartCol + EndCol) / 2;
                    break;
                case 2: // midpoint 
                    lenR = StartRow - MidRow;
                    lenC = StartCol - MidCol;

                    MidRow = newY;
                    MidCol = newX;

                    StartRow = MidRow + lenR;
                    StartCol = MidCol + lenC;
                    EndRow = MidRow - lenR;
                    EndCol = MidCol - lenC;
                    break;
            }
            updateArrowHandle();
        }
        /// <summary> 
        /// 辅助的箭头显示方法
        /// </summary>
        private void updateArrowHandle()
        {
            double length, dr, dc, halfHW;
            double rrow1, ccol1, rowP1, colP1, rowP2, colP2;

            int width = GetHandleWidth();
            double headLength = width;
            double headWidth = width;

            if (arrowHandleXLD == null)
                arrowHandleXLD = new HXLDCont();
            arrowHandleXLD.Dispose();
            arrowHandleXLD.GenEmptyObj();

            //箭头起始点为直线长度的0.8位置
            rrow1 = StartRow + (EndRow - StartRow) * 0.8;
            ccol1 = StartCol + (EndCol - StartCol) * 0.8;
            //测量箭头起始点到直线终点的距离
            length = HMisc.DistancePp(rrow1, ccol1, EndRow, EndCol);
            //如果距离为0说明直线长度为0
            if (length == 0)
                length = -1;

            dr = (EndRow - rrow1) / length;
            dc = (EndCol - ccol1) / length;

            halfHW = headWidth / 2.0;
            rowP1 = rrow1 + (length - headLength) * dr + halfHW * dc;
            rowP2 = rrow1 + (length - headLength) * dr - halfHW * dc;
            colP1 = ccol1 + (length - headLength) * dc - halfHW * dr;
            colP2 = ccol1 + (length - headLength) * dc + halfHW * dr;

            if (length == -1)
                arrowHandleXLD.GenContourPolygonXld(rrow1, ccol1);
            else
                arrowHandleXLD.GenContourPolygonXld(new HTuple(new double[] { rrow1, EndRow, rowP1, EndRow, rowP2, EndRow }),
                                                    new HTuple(new double[] { ccol1, EndCol, colP1, EndCol, colP2, EndCol }));
        }

    }
}
