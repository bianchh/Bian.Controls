﻿using HalconDotNet;
using System;

namespace Bian.Controls.HWndCtrl
{
    /// <summary>
    /// This class implements an ROI shaped as a circular
    /// arc. ROICircularArc inherits from the base class ROI and 
    /// implements (besides other auxiliary methods) all virtual methods 
    /// defined in ROI.cs.
    /// </summary>
    public class ROICircularArc : ROI
    {
        //handles
        private double midR, midC;       // 0. handle: midpoint
        private double sizeR, sizeC;     // 1. handle        
        private double startR, startC;   // 2. handle
        private double extentR, extentC; // 3. handle

        //model data to specify the arc
        public double Radius { get; set; }
        public double StartPhi { set; get; }
        public double ExtentPhi { set; get; } // -2*PI <= x <= 2*PI
        [NonSerialized]
        //display attributes
        private HXLDCont contour;
        [NonSerialized]
        private HXLDCont arrowHandleXLD;
        [NonSerialized]
        private HXLDCont startRect2XLD;
        private string circDir;
        private double TwoPI;
        private double PI;


        public ROICircularArc()
        {
            NumHandles = 4;         // midpoint handle + three handles on the arc
            activeHandleIdx = 0;
            contour = new HXLDCont();
            circDir = "";

            TwoPI = 2 * Math.PI;
            PI = Math.PI;

            arrowHandleXLD = new HXLDCont();
            arrowHandleXLD.GenEmptyObj();

            startRect2XLD = new HXLDCont();
            startRect2XLD.GenEmptyObj();

            this.ROIType = ROIType.CircleArc;
        }

        public override void ReCreateROI()
        {
            determineArcHandles();
            updateArrowHandle();
            updateStartRect2XLDHandle();
        }
        /// <summary>Creates a new ROI instance at the mouse position</summary>
        public override void CreateROI(double midX, double midY)
        {
            midR = midY;
            midC = midX;


            int width = GetHandleWidth();

            Radius = width * 10.0;

            sizeR = midR;
            sizeC = midC - Radius;

            StartPhi = PI * 0.25;
            ExtentPhi = PI * 1.5;
            circDir = "positive";

            determineArcHandles();
            updateArrowHandle();
            updateStartRect2XLDHandle();
        }

        /// <summary>Paints the ROI into the supplied window</summary>
        /// <param name="window">HALCON window</param>
        public override void Draw(HalconDotNet.HWindow window)
        {
            if (contour == null)
                contour = new HXLDCont();
            contour.Dispose();
            contour.GenCircleContourXld(midR, midC, Radius, StartPhi,
                                        (StartPhi + ExtentPhi), circDir, 1.0);
            window.DispObj(contour);

            int width = GetHandleWidth();
            window.DispRectangle2(sizeR, sizeC, 0, width, width);
            window.DispRectangle2(midR, midC, 0, width, width);

            window.DispObj(startRect2XLD);
            window.DispObj(arrowHandleXLD);
        }

        /// <summary> 
        /// Returns the distance of the ROI handle being
        /// closest to the image point(x,y)
        /// </summary>
        public override double DistToClosestHandle(double x, double y)
        {
            double max = 10000;
            double[] val = new double[NumHandles];

            val[0] = HMisc.DistancePp(y, x, midR, midC);       // midpoint 
            val[1] = HMisc.DistancePp(y, x, sizeR, sizeC);     // border handle 
            val[2] = HMisc.DistancePp(y, x, startR, startC);   // border handle 
            val[3] = HMisc.DistancePp(y, x, extentR, extentC); // border handle 

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
        /// Paints the active handle of the ROI object into the supplied window 
        /// </summary>
        public override void DisplayActive(HWindow window)
        {
            int width = GetHandleWidth();
            switch (activeHandleIdx)
            {
                case 0:
                    window.DispRectangle2(midR, midC, 0, width, width);
                    break;
                case 1:
                    window.DispRectangle2(sizeR, sizeC, 0, width, width);
                    break;
                case 2:
                    //window.DispRectangle2(startR, startC, startPhi, 10, 2);
                    window.DispObj(startRect2XLD);
                    break;
                case 3:
                    window.DispObj(arrowHandleXLD);
                    break;
            }
        }

        /// <summary> 
        /// Recalculates the shape of the ROI. Translation is 
        /// performed at the active handle of the ROI object 
        /// for the image coordinate (x,y)
        /// </summary>
        public override void MoveByHandle(double newX, double newY)
        {
            HTuple distance;
            double dirX, dirY, prior, next, valMax, valMin;

            switch (activeHandleIdx)
            {
                case 0: // midpoint 
                    dirY = midR - newY;
                    dirX = midC - newX;

                    midR = newY;
                    midC = newX;

                    sizeR -= dirY;
                    sizeC -= dirX;

                    determineArcHandles();
                    break;

                case 1: // handle at circle border                  
                    sizeR = newY;
                    sizeC = newX;

                    HOperatorSet.DistancePp(new HTuple(sizeR), new HTuple(sizeC),
                                            new HTuple(midR), new HTuple(midC), out distance);
                    Radius = distance[0].D;
                    determineArcHandles();
                    break;

                case 2: // start handle for arc                
                    dirY = newY - midR;
                    dirX = newX - midC;

                    StartPhi = Math.Atan2(-dirY, dirX);

                    if (StartPhi < 0)
                        StartPhi = PI + (StartPhi + PI);

                    setStartHandle();
                    prior = ExtentPhi;
                    ExtentPhi = HMisc.AngleLl(midR, midC, startR, startC, midR, midC, extentR, extentC);

                    if (ExtentPhi < 0 && prior > PI * 0.8)
                        ExtentPhi = (PI + ExtentPhi) + PI;
                    else if (ExtentPhi > 0 && prior < -PI * 0.7)
                        ExtentPhi = -PI - (PI - ExtentPhi);

                    break;

                case 3: // end handle for arc
                    dirY = newY - midR;
                    dirX = newX - midC;

                    prior = ExtentPhi;
                    next = Math.Atan2(-dirY, dirX);

                    if (next < 0)
                        next = PI + (next + PI);

                    if (circDir == "positive" && StartPhi >= next)
                        ExtentPhi = (next + TwoPI) - StartPhi;
                    else if (circDir == "positive" && next > StartPhi)
                        ExtentPhi = next - StartPhi;
                    else if (circDir == "negative" && StartPhi >= next)
                        ExtentPhi = -1.0 * (StartPhi - next);
                    else if (circDir == "negative" && next > StartPhi)
                        ExtentPhi = -1.0 * (StartPhi + TwoPI - next);

                    valMax = Math.Max(Math.Abs(prior), Math.Abs(ExtentPhi));
                    valMin = Math.Min(Math.Abs(prior), Math.Abs(ExtentPhi));

                    if ((valMax - valMin) >= PI)
                        ExtentPhi = (circDir == "positive") ? -1.0 * valMin : valMin;

                    setExtentHandle();
                    break;
            }

            circDir = (ExtentPhi < 0) ? "negative" : "positive";
            updateArrowHandle();

            updateStartRect2XLDHandle();
        }

        /// <summary>Gets the HALCON region described by the ROI</summary>
        public override HRegion GetRegion()
        {
            HRegion region;
            contour.Dispose();
            contour.GenCircleContourXld(midR, midC, Radius, StartPhi, (StartPhi + ExtentPhi), circDir, 1.0);
            region = new HRegion(contour);
            return region;
        }

        /// <summary>
        /// 圆弧数据 行/列/半径/角度1/角度2
        /// </summary> 
        public override HTuple GetModelData()
        {
            return new HTuple(new double[] { midR, midC, Radius, StartPhi, ExtentPhi });
        }

        /// <summary>
        /// Auxiliary method to determine the positions of the second and
        /// third handle.
        /// </summary>
        private void determineArcHandles()
        {
            setStartHandle();
            setExtentHandle();
        }

        /// <summary> 
        /// Auxiliary method to recalculate the start handle for the arc 
        /// </summary>
        private void setStartHandle()
        {
            startR = midR - Radius * Math.Sin(StartPhi);
            startC = midC + Radius * Math.Cos(StartPhi);
        }

        /// <summary>
        /// Auxiliary method to recalculate the extent handle for the arc
        /// </summary>
        private void setExtentHandle()
        {
            extentR = midR - Radius * Math.Sin(StartPhi + ExtentPhi);
            extentC = midC + Radius * Math.Cos(StartPhi + ExtentPhi);
        }

        /// <summary>
        /// Auxiliary method to display an arrow at the extent arc position
        /// </summary>
        private void updateArrowHandle()
        {
            double row1, col1, row2, col2;
            double rowP1, colP1, rowP2, colP2;
            double length, dr, dc, halfHW, sign, angleRad;

            int width = GetHandleWidth();
            double headLength = width;
            double headWidth = width;
            if (arrowHandleXLD == null)
                arrowHandleXLD = new HXLDCont();
            arrowHandleXLD.Dispose();
            arrowHandleXLD.GenEmptyObj();

            row2 = extentR;
            col2 = extentC;
            angleRad = (StartPhi + ExtentPhi) + Math.PI * 0.5;

            sign = (circDir == "negative") ? -1.0 : 1.0;
            row1 = row2 + sign * Math.Sin(angleRad) * 20;
            col1 = col2 - sign * Math.Cos(angleRad) * 20;

            length = HMisc.DistancePp(row1, col1, row2, col2);
            if (length == 0)
                length = -1;

            dr = (row2 - row1) / length;
            dc = (col2 - col1) / length;

            halfHW = headWidth / 2.0;
            rowP1 = row1 + (length - headLength) * dr + halfHW * dc;
            rowP2 = row1 + (length - headLength) * dr - halfHW * dc;
            colP1 = col1 + (length - headLength) * dc - halfHW * dr;
            colP2 = col1 + (length - headLength) * dc + halfHW * dr;

            if (length == -1)
                arrowHandleXLD.GenContourPolygonXld(row1, col1);
            else
                arrowHandleXLD.GenContourPolygonXld(new HTuple(new double[] { row1, row2, rowP1, row2, rowP2, row2 }),
                    new HTuple(new double[] { col1, col2, colP1, col2, colP2, col2 }));
        }
        /// <summary>
        /// 更新起始点的矩形框
        /// </summary>
        private void updateStartRect2XLDHandle()
        {
            if (startRect2XLD == null)
                startRect2XLD = new HXLDCont();
            startRect2XLD.Dispose();
            startRect2XLD.GenEmptyObj();
            int width = GetHandleWidth();
            startRect2XLD.GenRectangle2ContourXld(startR, startC, StartPhi, width, width / 5);

        }
    }//end of class
}
