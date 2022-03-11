using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bian.Controls.HWndCtrl
{
    public enum ROIType
    {
        /// <summary>
        /// 直线
        /// </summary>
        Line = 10,
        /// <summary>
        /// 圆
        /// </summary>
        Circle,
        /// <summary>
        /// 圆弧
        /// </summary>
        CircleArc,
        /// <summary>
        /// 矩形
        /// </summary>
        Rectangle1,
        /// <summary>
        /// 带角度矩形
        /// </summary>
        Rectangle2,
        /// <summary>
        /// 读码使用的矩形
        /// </summary>
        Rectangle1WithCode
    }

    public enum ROIOperation
    {
        /// <summary>
        /// ROI求和模式
        /// </summary>
        Positive = 21,
        /// <summary>
        /// ROI求差模式
        /// </summary>
        Negative,
        /// <summary>
        /// ROI模式为无
        /// </summary>
        None
    }

    public enum ViewMessage
    {
        UpdateROI = 50,

        ChangeROISign,

        MovingROI,

        DeletedActROI,

        DelectedAllROIs,

        ActivatedROI,

        MouseMove,

        CreateROI,

        UpdateImage,

        ErrReadimgImage,

        ErrDefiningGC
    }

    public enum ShowMode
    {
        /// <summary>
        /// 包含ROI显示
        /// </summary>
        IncludeROI = 1,
        /// <summary>
        /// 不包含ROI显示
        /// </summary>
        ExcludeROI
    }

    public enum ResultShow
    {
        原图,
        处理后
    }
}
