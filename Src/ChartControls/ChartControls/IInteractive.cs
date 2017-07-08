#region License
// Copyright (c) 2015 Wayne Gu
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

#if USINGCANVAS
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
#else
using System.Windows.Controls;
using System.Windows.Media;
#endif


namespace ChartControls
{
    public sealed class InteractiveConst
    {
        public const int nodeIndexInTarget = -1;
        public const int nodeIndexOutofTarget = -2;
    }

    /// <summary>
    /// 交互接口。
    /// </summary>
    public interface IInteractive
    {
        /// <summary>
        /// 是否有提示内容。
        /// </summary>
        bool HasTooltip
        {
            get;
            set;
        }

        /// <summary>
        /// 提示内容。
        /// </summary>
        object ToolTip
        {
            get;
            set;
        }

#if USINGCANVAS
        PopupMenu ContextMenu
        {
            get;
            set;
        }
#else
        /// <summary>
        /// 菜单。
        /// </summary>
        ContextMenu ContextMenu
        {
            get;
            set;
        }
#endif
        /// <summary>
        /// 判断点是否在区域内。
        /// </summary>
        bool IsPointInRegion(Point point);

        /// <summary>
        /// 是否可以选择。
        /// </summary>
        bool CanSelect
        {
            get;
            set;
        }

        /// <summary>
        /// 选择。
        /// </summary>
        bool IsSelected
        {
            get;
            set;
        }

        /// <summary>
        /// 是否可以改变。
        /// </summary>
        bool CanChange
        {
            get;
            set;
        }

        /// <summary>
        /// 取点所在的节点索引。
        /// </summary>
        int GetNodeIndex(Point point);
        
        /// <summary>
        /// 更新节点的坐标。
        /// </summary>
        /// <param name="nodeIndex">节点索引。</param>
        /// <param name="newPosition">新坐标。</param>
        void UpdateNodePosition(int nodeIndex, Point newPosition);

        /// <summary>
        /// 转换图形。
        /// </summary>
        /// <param name="transform"></param>
        void TranformPosition(Transform transform);
    }
}
