using HalconDotNet;

namespace Bian.Controls.HWndCtrl
{
    public class ROI
    {
        private int imageWidth;

        /// <summary>
        /// 图形窗体上控制框的个数
        /// </summary>
        protected int NumHandles;

        /// <summary>
        /// 图像窗体上活跃控制框的序号
        /// </summary>
        protected int activeHandleIdx;

        /// <summary>
        /// ROI类型
        /// </summary>
        public ROIType ROIType { get; protected set; }

        protected string info;

        /// <summary>
        /// ROI线显示方式
        /// </summary>
        public HTuple FlagLineStyle { get; set; }

        public double TxtScale { get; set; }

        protected ROIOperation operatorFlag;
        /// <summary>
        /// ROI 定义为 'positive' 或者 'negative' 的标记
        /// </summary>
        public ROIOperation OperatorFlag
        {
            get
            {
                return operatorFlag;
            }
            set
            {
                operatorFlag = value;

                switch (operatorFlag)
                {
                    case ROIOperation.Positive:
                        FlagLineStyle = posOperation;
                        break;
                }
            }
        }

        /// <summary>
        /// 要显示ROI的图像宽度
        /// </summary>
        public int ImageWidth
        {
            get
            {
                if (imageWidth == 0)
                {
                    imageWidth = 500;
                }
                return imageWidth;
            }
            set
            {
                imageWidth = value;
            }
        }

        /// <summary>
        /// "+"方式的直接直线
        /// </summary>
        protected HTuple posOperation = new HTuple();

        /// <summary>
        /// "-"方式的虚线
        /// </summary>
        protected HTuple negOperation = new HTuple(new int[] { 8, 8 });

        /// <summary>
        /// 重新绘制ROI
        /// </summary>
        public virtual void ReCreateROI() { }

        /// <summary>
        /// 在指定位置创建ROI
        /// </summary>
        /// <param name="midX">列坐标</param>
        /// <param name="midY">行坐标</param>
        public virtual void CreateROI(double midX, double midY) { }

        /// <summary>
        /// 在支持的窗体上绘制ROI
        /// </summary>
        /// <param name="window"></param>
        public virtual void Draw(HWindow window) { }

        /// <summary>
        /// 求出鼠标坐标与ROI的最近控制点的距离
        /// </summary>
        /// <param name="x">鼠标坐标X</param>
        /// <param name="y">鼠标坐标Y</param>
        /// <returns>鼠标与ROI的控制框的最近距离值</returns>
        public virtual double DistToClosestHandle(double x, double y)
        {
            return 0.0;
        }

        /// <summary>
        /// 将活动句柄绘制到提供的窗体上
        /// </summary>
        /// <param name="window"></param>
        public virtual void DisplayActive(HWindow window) { }

        public virtual void MoveByHandle(double x, double y) { }

        /// <summary>
        /// 获取ROI描述的region
        /// </summary>
        /// <returns></returns>
        public virtual HRegion GetRegion()
        {
            return null;
        }

        public virtual double GetDistanceFromStartPoint(double row, double col)
        {
            return 0.0;
        }

        /// <summary>
        /// 获取ROI的描述点坐标信息
        /// </summary>
        /// <returns></returns>
        public virtual HTuple GetModelData()
        {
            return null;
        }

        /// <summary>
        /// ROI定义的句柄数
        /// </summary>
        /// <returns>句柄数</returns>
        public int GetNumHandles()
        {
            return NumHandles;
        }

        /// <summary>
        /// 获取活跃 ROI 的句柄
        /// </summary>
        /// <returns>活跃句柄的序号</returns>
        public int GetActHandleIdx()
        {
            return activeHandleIdx;
        }

        public int GetHandleWidth()
        {
            int dat = imageWidth / 100;

            if (dat < 3)
            {
                dat = 3;
            }
            else if (dat > 20)
            {
                dat = 20;
            }

            return dat;
        }
    }
}
