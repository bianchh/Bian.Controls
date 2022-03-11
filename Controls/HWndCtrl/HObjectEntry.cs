using HalconDotNet;
using System.Collections;

namespace Bian.Controls.HWndCtrl
{
    internal class HObjectEntry
    {
        /// <summary>
        /// HObject 的键值集合
        /// </summary>
        public Hashtable gContext;

        /// <summary>
        /// halcon 图像对象
        /// </summary>
        public HObject HObj;

        public HWndMessage Message;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="obj">图像对象</param>
        /// <param name="gc">显示该对象之前应用的图形状态的Hashlist。</param>
        public HObjectEntry(HObject obj, Hashtable gc)
        {
            gContext = gc;
            if (obj != null && obj.IsInitialized())
            {
                if (obj is HImage image)
                {
                    HObj = image.Clone();
                }
                else
                {
                    HObj = obj.Clone();
                }
            }
        }

        public HObjectEntry(HWndMessage message)
        {
            this.Message = message;
        }

        /// <summary>
        /// 清除实体
        /// </summary>
        public void Clear()
        {
            gContext.Clear();
            HObj?.Dispose();
        }
    }
}
