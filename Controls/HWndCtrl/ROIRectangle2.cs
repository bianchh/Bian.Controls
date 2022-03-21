using HalconDotNet;
using System;

namespace Bian.Controls.HWndCtrl
{
    /// <summary>
    /// This class demonstrates one of the possible implementations for a 
    /// (simple) rectangularly shaped ROI. To create this rectangle we use 
    /// a center point (midR, midC), an orientation 'phi' and the half 
    /// edge lengths 'length1' and 'length2', similar to the HALCON 
    /// operator gen_rectangle2(). 
    /// The class ROIRectangle2 inherits from the base class ROI and 
    /// implements (besides other auxiliary methods) all virtual methods 
    /// defined in ROI.cs.
    /// </summary>
    public class ROIRectangle2 : ROI
    {

        /// <summary>Half length of the rectangle side, perpendicular to phi</summary>
        public double Length1 { get; set; }

        /// <summary>Half length of the rectangle side, in direction of phi</summary>
        public double Length2 { get; set; }

        /// <summary>Row coordinate of midpoint of the rectangle</summary>
        public double MidR { get; set; }

        /// <summary>Column coordinate of midpoint of the rectangle</summary>
        public double MidC { get; set; }

        /// <summary>Orientation of rectangle defined in radians.</summary>
        public double Phi { get; set; }

        //auxiliary variables
        HTuple rowsInit;
        HTuple colsInit;
        HTuple rows;
        HTuple cols;

        HHomMat2D hom2D, tmp;

        /// <summary>Constructor</summary>
        public ROIRectangle2()
        {
            NumHandles = 6; // 4 corners +  1 midpoint + 1 rotationpoint			
            activeHandleIdx = 4;

            this.ROIType = ROIType.Rectangle2;
        }
        public ROIRectangle2(string info)
        {
            NumHandles = 6; // 4 corners +  1 midpoint + 1 rotationpoint			
            activeHandleIdx = 4;

            this.ROIType = ROIType.Rectangle2;
            this.info = info;
        }
        /// <summary>Creates a new ROI instance at the mouse position</summary>
        /// <param name="midX">
        /// x (=column) coordinate for interactive ROI
        /// </param>
        /// <param name="midY">
        /// y (=row) coordinate for interactive ROI
        /// </param>
        public override void CreateROI(double midX, double midY)
        {

            int width = GetHandleWidth();

            MidR = midY;
            MidC = midX;

            Length1 = width * 10;
            Length2 = width * 5;

            Phi = 0.0;

            rowsInit = new HTuple(new double[] {-1.0, -1.0, 1.0,
                                                   1.0,  0.0, 0.0 });
            colsInit = new HTuple(new double[] {-1.0, 1.0,  1.0,
                                                  -1.0, 0.0, 0.6 });
            //order        ul ,  ur,   lr,  ll,   mp, arrowMidpoint
            hom2D = new HHomMat2D();
            tmp = new HHomMat2D();

            updateHandlePos();
        }

        public override void ReCreateROI()
        {
            base.ReCreateROI();
            rowsInit = new HTuple(new double[] { -1.0, -1.0, 1.0, 1.0, 0.0, 0.0 });
            colsInit = new HTuple(new double[] { -1.0, 1.0, 1.0, -1.0, 0.0, 0.6 });
            //order  ul, ur, lr, ll, mp, arrowMidpoint
            hom2D = new HHomMat2D();
            tmp = new HHomMat2D();
            updateHandlePos();
        }

        /// <summary>Paints the ROI into the supplied window</summary>
        /// <param name="window">HALCON window</param>
        public override void Draw(HalconDotNet.HWindow window)
        {

            int width = GetHandleWidth();
            window.DispRectangle2(MidR, MidC, -Phi, Length1, Length2);
            for (int i = 0; i < NumHandles; i++)
                window.DispRectangle2(rows[i].D, cols[i].D, -Phi, width, width);

            window.DispArrow(MidR, MidC, MidR + (Math.Sin(Phi) * Length1 * 1.2),
                MidC + (Math.Cos(Phi) * Length1 * 1.2), width / 3.0);
            if (info != null && info.Length > 0)
            {
#if NativeCode
            Wrapper.ShowUnit.ShowText(window, info,(int) rows[0].D, (int)cols[0].D, (int)(20.0*TxtScale), "green","image");
#else
                HWndMessage message = new HWndMessage(info, (int)rows[0].D, (int)cols[0].D, 20, "green");
                message.DispMessage(window, "image", TxtScale);
#endif
            }

        }

        /// <summary> 
        /// Returns the distance of the ROI handle being
        /// closest to the image point(x,y)
        /// </summary>
        /// <param name="x">x (=column) coordinate</param>
        /// <param name="y">y (=row) coordinate</param>
        /// <returns> 
        /// Distance of the closest ROI handle.
        /// </returns>
        public override double DistToClosestHandle(double x, double y)
        {
            double max = 10000;
            double[] val = new double[NumHandles];


            for (int i = 0; i < NumHandles; i++)
                val[i] = HMisc.DistancePp(y, x, rows[i].D, cols[i].D);

            for (int i = 0; i < NumHandles; i++)
            {
                if (val[i] < max)
                {
                    max = val[i];
                    activeHandleIdx = i;
                }
            }
            return val[activeHandleIdx];
        }

        /// <summary> 
        /// Paints the active handle of the ROI object into the supplied window
        /// </summary>
        /// <param name="window">HALCON window</param>
        public override void DisplayActive(HalconDotNet.HWindow window)
        {

            int width = GetHandleWidth();
            window.DispRectangle2(rows[activeHandleIdx].D,
                                  cols[activeHandleIdx].D,
                                  -Phi, width, width);

            if (activeHandleIdx == 5)
                window.DispArrow(MidR, MidC,
                                 MidR + (Math.Sin(Phi) * Length1 * 1.2),
                                 MidC + (Math.Cos(Phi) * Length1 * 1.2),
                                 width / 3.0);
        }


        /// <summary>Gets the HALCON region described by the ROI</summary>
        public override HRegion GetRegion()
        {
            HRegion region = new HRegion();
            region.GenRectangle2(MidR, MidC, -Phi, Length1, Length2);
            return region;
        }

        /// <summary>
        /// Gets the model information described by 
        /// the interactive ROI
        /// </summary> 
        public override HTuple GetModelData()
        {
            return new HTuple(new double[] { MidR, MidC, Phi, Length1, Length2 });
        }

        /// <summary> 
        /// Recalculates the shape of the ROI instance. Translation is 
        /// performed at the active handle of the ROI object 
        /// for the image coordinate (x,y)
        /// </summary>
        /// <param name="newX">x mouse coordinate</param>
        /// <param name="newY">y mouse coordinate</param>
        public override void MoveByHandle(double newX, double newY)
        {
            double vX, vY;//, x=0, y=0;
            HTuple l2, l1;
            switch (activeHandleIdx)
            {
                case 0:

                    HOperatorSet.DistancePl(newY, newX, rows[1], cols[1], rows[2], cols[2], out l1);
                    HOperatorSet.DistancePl(newY, newX, rows[3], cols[3], rows[2], cols[2], out l2);
                    Length1 = Math.Abs(l1.D) / 2.0;
                    Length2 = Math.Abs(l2.D) / 2.0;
                    MidR = (newY + rows[2].D) / 2.0;
                    MidC = (newX + cols[2].D) / 2.0;
                    break;
                case 1:

                    HOperatorSet.DistancePl(newY, newX, rows[2], cols[2], rows[3], cols[3], out l1);
                    HOperatorSet.DistancePl(newY, newX, rows[3], cols[3], rows[0], cols[0], out l2);
                    Length2 = Math.Abs(l1.D) / 2.0;
                    Length1 = Math.Abs(l2.D) / 2.0;
                    MidR = (newY + rows[3].D) / 2.0;
                    MidC = (newX + cols[3].D) / 2.0;
                    break;
                case 2:
                    HOperatorSet.DistancePl(newY, newX, rows[0], cols[0], rows[1], cols[1], out l1);
                    HOperatorSet.DistancePl(newY, newX, rows[3], cols[3], rows[0], cols[0], out l2);
                    Length2 = Math.Abs(l1.D) / 2.0;
                    Length1 = Math.Abs(l2.D) / 2.0;
                    MidR = (newY + rows[0].D) / 2.0;
                    MidC = (newX + cols[0].D) / 2.0;
                    break;
                case 3:

                    HOperatorSet.DistancePl(newY, newX, rows[0], cols[0], rows[1], cols[1], out l1);
                    HOperatorSet.DistancePl(newY, newX, rows[1], cols[1], rows[2], cols[2], out l2);
                    Length2 = Math.Abs(l1.D) / 2.0;
                    Length1 = Math.Abs(l2.D) / 2.0;
                    MidR = (newY + rows[1].D) / 2.0;
                    MidC = (newX + cols[1].D) / 2.0;
                    break;
                //               tmp = hom2D.HomMat2dInvert();
                //x = tmp.AffineTransPoint2d(newX, newY, out y);

                //length2 = Math.Abs(y);
                //length1 = Math.Abs(x);
                //               checkForRange(x, y);
                //               updateHandlePos();
                //               break;

                case 4://平移
                    MidC = newX;
                    MidR = newY;
                    // updateHandlePos();
                    break;
                case 5://旋转
                    vY = newY - rows[4].D;
                    vX = newX - cols[4].D;
                    Phi = Math.Atan2(vY, vX);

                    break;
            }
            updateHandlePos();
        }//end of method
        private void updatehandlePosCorner()
        {

        }

        /// <summary>
        /// Auxiliary method to recalculate the contour points of 
        /// the rectangle by transforming the initial row and 
        /// column coordinates (rowsInit, colsInit) by the updated
        /// homography hom2D
        /// </summary>
        private void updateHandlePos()
        {
            hom2D.HomMat2dIdentity();
            //平移
            hom2D = hom2D.HomMat2dTranslate(MidC, MidR);
            //旋转
            hom2D = hom2D.HomMat2dRotateLocal(Phi);
            //缩放
            tmp = hom2D.HomMat2dScaleLocal(Length1, Length2);
            // tmp=hom2D
            cols = tmp.AffineTransPoint2d(colsInit, rowsInit, out rows);
        }


        /* This auxiliary method checks the half lengths 
		 * (length1, length2) using the coordinates (x,y) of the four 
		 * rectangle corners (handles 0 to 3) to avoid 'bending' of 
		 * the rectangular ROI at its midpoint, when it comes to a
		 * 'collapse' of the rectangle for length1=length2=0.
		 * */
        private void checkForRange(double x, double y)
        {
            switch (activeHandleIdx)
            {
                case 0:
                    if ((x < 0) && (y < 0))
                        return;
                    if (x >= 0) Length1 = 0.01;
                    if (y >= 0) Length2 = 0.01;
                    break;
                case 1:
                    if ((x > 0) && (y < 0))
                        return;
                    if (x <= 0) Length1 = 0.01;
                    if (y >= 0) Length2 = 0.01;
                    break;
                case 2:
                    if ((x > 0) && (y > 0))
                        return;
                    if (x <= 0) Length1 = 0.01;
                    if (y <= 0) Length2 = 0.01;
                    break;
                case 3:
                    if ((x < 0) && (y > 0))
                        return;
                    if (x >= 0) Length1 = 0.01;
                    if (y <= 0) Length2 = 0.01;
                    break;
                default:
                    break;
            }
        }
    }//end of class
}
