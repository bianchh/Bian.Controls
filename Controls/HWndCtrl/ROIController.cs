using HalconDotNet;
using System;
using System.Collections.Generic;

namespace Bian.Controls.HWndCtrl
{
    public delegate void FuncROIDelegate();

    public class ROIController
    {
        #region 属性
        [NonSerialized]
        private ROI roiSeed;
        private ROIOperation stateROIOperation;
        [NonSerialized]
        private double currX, currY;
        [NonSerialized]
        private int activeROIidx;
        [NonSerialized]
        private int deletedIdx;

        public List<ROI> ROIList = new List<ROI>();

        [NonSerialized]
        private HRegion modelROI;
        private string activeCol = "green";
        private string activeHdlCol = "red";
        private string inactiveCol = "yellow";
        private string serachRegionCol = "blue";

        [NonSerialized]
        public HWndCtrl viewController;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ROIController()
        {
            stateROIOperation = ROIOperation.Positive;
            activeROIidx = -1;
            modelROI = new HRegion();
            deletedIdx = -1;
            currX = currY = -1;
        }

        /// <summary>
        /// 当前活动的ROI序列
        /// </summary>
        public int ActiveRoiIdx
        {
            get
            {
                return activeROIidx;
            }
            set
            {
                activeROIidx = value;
                TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.UpdateROI));
            }
        }

        public event EventHandler<ViewEventArgs> ROINotifyEvent;
        public void TriggerROINotifyEvent(ViewEventArgs e)
        {
            if (ROINotifyEvent != null)
            {
                ROINotifyEvent(this, e);
            }
        }

        public event EventHandler<List<ROI>> ROIChangeEvent;
        public void TriggerROIChangeEvent(List<ROI> e)
        {
            if (ROIChangeEvent != null)
            {
                ROIChangeEvent(this, e);
            }
        }

        /// <summary>
        /// 将HWndCtrl注册到此ROIController实例
        /// </summary>
        /// <param name="view"></param>
        public void SetViewController(HWndCtrl view)
        {
            viewController = view;
        }

        /// <summary>
        /// 获取ModelROI对象
        /// </summary>
        /// <returns></returns>
        public HRegion GetModelRegion()
        {
            return modelROI;
        }


        public ROI GetActiveROI()
        {
            if (activeROIidx != -1)
            {
                return ROIList[activeROIidx];
            }

            return null;
        }


        public int GetDelROIIdx()
        {
            return deletedIdx;
        }

        /// <summary>
        /// 为了创建一个新的ROI对象，应用程序类初始化一个“种子”ROI实例并将其传递给ROIController。
        /// ROIController现在通过操纵这个新的ROI实例进行响应。
        /// </summary>
        /// <param name="r"></param>
        public void SetROIShape(ROI r)
        {
            roiSeed = r;
            roiSeed.OperatorFlag = stateROIOperation;

            roiSeed.ImageWidth = viewController.ImageWidth;
            roiSeed.CreateROI(viewController.ImageWidth / 2, viewController.ImageHeight / 2);
            ROIList.Add(roiSeed);
            roiSeed = null;

            viewController?.Repaint();
            TriggerROIChangeEvent(ROIList);
        }

        public void AddROI(ROIRectangle1 r)
        {
            ROIList.Add(r);
            viewController?.Repaint();
        }

        public void SetROIShapeNoOperator(ROI r)
        {
            roiSeed = r;
            roiSeed.OperatorFlag = ROIOperation.None;
            //只能有一个无标志的roi作为搜索框
            for (int i = 0; i < ROIList.Count; i++)
            {
                if (ROIList[i].OperatorFlag == ROIOperation.None)
                {
                    ROIList.RemoveAt(i);
                }
            }
        }

        public void SetROISign(ROIOperation mode)
        {
            stateROIOperation = mode;

            if (activeROIidx != -1)
            {
                ROIList[activeROIidx].OperatorFlag = stateROIOperation;

                if (viewController != null)
                {
                    viewController.Repaint();
                }

                TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.ChangeROISign));
            }
        }

        public void SetROIList(List<ROI> roiList)
        {
            ROIList = roiList;
            foreach (ROI roi in ROIList)
            {
                roi.ReCreateROI();
            }
        }

        public void RemoveActive()
        {
            if (activeROIidx != -1)
            {
                ROIList.RemoveAt(activeROIidx);
                deletedIdx = activeROIidx;
                activeROIidx = -1;

                if (viewController != null)
                {
                    viewController.Repaint();
                }

                TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.DeletedActROI));
                TriggerROIChangeEvent(ROIList);
            }
        }

        public void RemoveROI(int index)
        {
            if (index < 0 || ROIList.Count < index)
            {
                return;
            }

            ROIList.RemoveAt(index);
            activeROIidx = -1;

            if (viewController != null)
            {
                viewController.Repaint();
            }

            TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.DeletedActROI));
        }

        public bool DefineModeROI()
        {
            HRegion tmpAdd, tmpDiff, tmp;
            double row, col;

            if (stateROIOperation == ROIOperation.None)    //ROI搜索模式
            {
                return true;
            }

            tmpAdd = new HRegion();
            tmpDiff = new HRegion();
            tmpAdd.GenEmptyRegion();
            tmpDiff.GenEmptyRegion();

            for (int i = 0; i < ROIList.Count; i++)
            {
                switch (ROIList[i].OperatorFlag)
                {
                    case ROIOperation.Positive:
                        tmp = ROIList[i].GetRegion();
                        tmpAdd = tmp.Union2(tmpAdd);    //把所有求和模式的ROI Region联合在一起
                        break;
                    case ROIOperation.Negative:
                        tmp = ROIList[i].GetRegion();
                        tmpDiff = tmp.Union2(tmpDiff);  //把所有求差模式的ROI Region联合在一起
                        break;
                    case ROIOperation.None:
                        break;
                    default:
                        break;
                }
            }

            modelROI = null;

            if (tmpAdd.AreaCenter(out row, out col) > 0)
            {
                tmp = tmpAdd.Difference(tmpDiff);
                if (tmp.AreaCenter(out row, out col) > 0)   //如果tmpAdd > tmpDiff
                {
                    modelROI = tmp;                     //把差值赋给modelROI
                }
            }
            //防止positive和negative的ROI组合都没有
            if (modelROI == null || ROIList.Count == 0)
            {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            ROIList.Clear();
            activeROIidx = -1;
            modelROI = null;
            roiSeed = null;
            TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.DelectedAllROIs));
        }

        public void ResetROI()
        {
            activeROIidx = -1;
            roiSeed = null;
        }

        public void SetDrawColor(string aColor, string aHdlColor, string inaColor)
        {
            if (aColor != "")
            {
                activeCol = aColor;
            }
            if (aHdlColor != "")
            {
                activeHdlCol = aHdlColor;
            }
            if (inactiveCol != "")
            {
                inactiveCol = inaColor;
            }
        }

        public void PaintData(HWindow window, int imageWidth, double txtScale)
        {
            window.SetDraw("margin");
            window.SetLineWidth(1);

            for (int i = 0; i < ROIList.Count; i++)
            {
                window.SetLineStyle(ROIList[i].FlagLineStyle);
                ROIList[i].ImageWidth = imageWidth;
                ROIList[i].TxtScale = txtScale;

                if (ROIList[i].OperatorFlag == ROIOperation.None)
                {
                    window.SetColor(serachRegionCol);   //采用搜索区域颜色
                }
                else
                {
                    window.SetColor(inactiveCol);
                }
                ROIList[i].Draw(window);
            }

            if (activeROIidx != -1)
            {
                window.SetColor(activeCol);
                window.SetLineStyle(ROIList[activeROIidx].FlagLineStyle);
                ROIList[activeROIidx].Draw(window);

                window.SetColor(activeHdlCol);
                ROIList[activeROIidx].DisplayActive(window);
            }
        }


        public int MouseDownAction(double imgX, double imgY)
        {
            int idxROI = -1;
            double max = 10000, dist = 0;

            //判断是否为新建ROI
            if (roiSeed != null)   //
            {
                roiSeed.ImageWidth = viewController.ImageWidth;
                roiSeed.CreateROI(imgX, imgY);
                ROIList.Add(roiSeed);
                roiSeed = null;
                activeROIidx = ROIList.Count - 1;
                if (viewController != null)
                {
                    viewController.Repaint();
                }
                TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.CreateROI));
            }
            else if (ROIList.Count > 0)
            {
                activeROIidx = -1;

                for (int i = 0; i < ROIList.Count; i++)
                {
                    dist = ROIList[i].DistToClosestHandle(imgX, imgY);
                    double epsilon = ROIList[i].GetHandleWidth() + 2.0;
                    if ((dist < max) && (dist < epsilon))
                    {
                        max = dist;
                        idxROI = i;
                    }
                }

                if (idxROI >= 0)
                {
                    activeROIidx = idxROI;
                    TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.ActivatedROI));
                    TriggerROIChangeEvent(ROIList);
                }

                if (viewController != null)
                {
                    viewController.Repaint();
                }
            }

            return activeROIidx;
        }

        public void MouseMoveAction(double newX, double newY)
        {
            if ((newX == currX) && (newY == currY))
            {
                return;
            }

            ROIList[activeROIidx].MoveByHandle(newX, newY);
            if (viewController != null)
            {
                viewController.Repaint();
            }

            currX = newX;
            currY = newY;

            TriggerROINotifyEvent(new ViewEventArgs(ViewMessage.MovingROI));
            TriggerROIChangeEvent(ROIList);
        }
    }
}
