using HalconDotNet;
using System.Collections;

namespace Bian.Controls.HWndCtrl
{
    public class GraphicsContext
    {
        //halcon 窗体显示设置参数
        public const string GC_COLOR = "Color";
        public const string GC_COLORED = "Colored";
        public const string GC_LINEWIDTH = "LineWidth";
        public const string GC_DRAWMODE = "DrawMode";
        public const string GC_SHAPE = "Shape";
        public const string GC_LUT = "Lut";
        public const string GC_PAINT = "Paint";
        public const string GC_LINESTYLE = "LineStyle";

        /// <summary>
        /// 图像窗体的设置属性
        /// </summary>
        private Hashtable graphicsSetting;

        /// <summary>
        /// 图像窗口的当前显示属性
        /// </summary>
        public Hashtable stateOfSetting;

        public IEnumerator iterator;

        /// <summary>
        /// 构造函数
        /// </summary>
        public GraphicsContext()
        {
            graphicsSetting = new Hashtable(10, 0.2f);
            stateOfSetting = new Hashtable(10, 0.2f);
        }

        public GraphicsContext(Hashtable setting)
        {
            graphicsSetting = setting;
            stateOfSetting = new Hashtable(10, 0.2f);
        }

        public void ApplyContext(HWindow window, Hashtable cContext)
        {
            string key = "";
            string valS = "";
            int valI = -1;
            HTuple valH = null;

            //获取窗体显示属性的枚举
            iterator = cContext.Keys.GetEnumerator();
            if (!window.IsInitialized())
            {
                return;
            }
            if (window == null)
            {
                return;
            }
            try
            {
                while (iterator.MoveNext())
                {
                    //当前元素
                    key = (string)iterator.Current;
                    //当前属性与设置属性相同就跳过当前一次循环
                    if (stateOfSetting.Contains(key)
                        && stateOfSetting[key] == cContext[key])
                    {
                        continue;
                    }

                    switch (key)
                    {
                        case GC_COLOR:
                            valS = (string)cContext[key];
                            if (stateOfSetting.Contains(GC_COLORED))
                            {
                                stateOfSetting.Remove(GC_COLORED);
                            }
                            if (stateOfSetting.ContainsKey(key))
                            {
                                break;
                            }
                            window.SetColor(valS);
                            break;
                        case GC_COLORED:
                            valI = (int)cContext[key];
                            if (stateOfSetting.Contains(GC_COLOR))
                            {
                                stateOfSetting.Remove(GC_COLOR);
                            }
                            if (stateOfSetting.ContainsKey(key))
                            {
                                break;
                            }
                            window.SetColored(12);
                            break;
                        case GC_DRAWMODE:
                            valS = (string)cContext[key];
                            window.SetDraw(valS);
                            break;
                        case GC_LINEWIDTH:
                            valI = (int)cContext[key];
                            window.SetLineWidth(valI);
                            break;
                        case GC_LUT:
                            valS = (string)cContext[key];
                            window.SetLut(valS);
                            break;
                        case GC_SHAPE:
                            valS = (string)cContext[key];
                            window.SetShape(valH);
                            break;
                        case GC_LINESTYLE:
                            valH = (HTuple)cContext[key];
                            window.SetLineStyle(valH);
                            break;
                        default:
                            break;
                    }

                    if (valI != -1)
                    {
                        if (stateOfSetting.Contains(key))
                        {
                            stateOfSetting[key] = valI;
                        }
                        else
                        {
                            stateOfSetting.Add(key, valI);
                        }

                        valI = -1;
                    }
                    else if (valS != "")
                    {
                        if (stateOfSetting.Contains(key))
                        {
                            stateOfSetting[key] = valS;
                        }
                        else
                        {
                            stateOfSetting.Add(key, valS);
                        }

                        valS = "";
                    }
                    else if (valH != null)
                    {
                        if (stateOfSetting.Contains(key))
                        {
                            stateOfSetting[key] = valH;
                        }
                        else
                        {
                            stateOfSetting.Add(key, valH);
                        }

                        valH = null;
                    }
                }
            }
            catch (HOperatorException ex)
            {
                return;
            }
        }

        public void SetColorAttribute(string val)
        {
            if (graphicsSetting.ContainsKey(GC_COLORED))
            {
                graphicsSetting.Remove(GC_COLORED);
            }

            addValue(GC_COLOR, val);
        }

        public void SetColoredAttribute(int val)
        {
            if (graphicsSetting.ContainsKey(GC_COLOR))
            {
                graphicsSetting.Remove(GC_COLOR);
            }

            addValue(GC_COLORED, val);
        }

        public void SetDrawModeAttribute(string val)
        {
            addValue(GC_DRAWMODE, val);
        }

        public void SetLineWidthAttribute(int val)
        {
            addValue(GC_LINEWIDTH, val);
        }

        public void SetLutAttribute(string val)
        {
            addValue(GC_LUT, val);
        }

        public void SetPaintAttribute(string val)
        {
            addValue(GC_PAINT, val);
        }

        public void SetShapeAttribute(string val)
        {
            addValue(GC_SHAPE, val);
        }

        public void SetLineStyleAttribute(HTuple val)
        {
            addValue(GC_LINESTYLE, val);
        }

        /// <summary> 
        /// Adds a value to the hashlist 'graphicalSettings' for the 
        /// graphical mode described by the parameter 'key' 
        /// </summary>
        /// <param name="key"> 
        /// A graphical mode defined by the constant GC_* 
        /// </param>
        /// <param name="val"> 
        /// Defines the value as an int for this graphical
        /// mode 'key' 
        /// </param>
        private void addValue(string key, int val)
        {
            if (graphicsSetting.ContainsKey(key))
            {
                graphicsSetting[key] = val;
            }
            else
            {
                graphicsSetting.Add(key, val);
            }
        }

        /// <summary>
        /// Adds a value to the hashlist 'graphicalSettings' for the 
        /// graphical mode, described by the parameter 'key'
        /// </summary>
        /// <param name="key"> 
        /// A graphical mode defined by the constant GC_* 
        /// </param>
        /// <param name="val"> 
        /// Defines the value as a string for this 
        /// graphical mode 'key' 
        /// </param>
        private void addValue(string key, string val)
        {
            if (graphicsSetting.ContainsKey(key))
            {
                graphicsSetting[key] = val;
            }
            else
            {
                graphicsSetting.Add(key, val);
            }
        }

        /// <summary> 
        /// Adds a value to the hashlist 'graphicalSettings' for the 
        /// graphical mode, described by the parameter 'key'
        /// </summary>
        /// <param name="key">
        /// A graphical mode defined by the constant GC_* 
        /// </param>
        /// <param name="val"> 
        /// Defines the value as a HTuple for this 
        /// graphical mode 'key' 
        /// </param>
        private void addValue(string key, HTuple val)
        {
            if (graphicsSetting.ContainsKey(key))
            {
                graphicsSetting[key] = val;
            }
            else
            {
                graphicsSetting.Add(key, val);
            }
        }

        /// <summary>
        /// 清除属性设置
        /// </summary>
        public void Clear()
        {
            graphicsSetting.Clear();
        }


        public GraphicsContext Copy()
        {
            return new GraphicsContext((Hashtable)this.graphicsSetting.Clone());
        }

        public object GetGraphicsAttribute(string key)
        {
            if (graphicsSetting.ContainsKey(key))
            {
                return graphicsSetting[key];
            }

            return null;
        }

        public Hashtable CopyContextList()
        {
            return (Hashtable)graphicsSetting.Clone();
        }
    }
}
