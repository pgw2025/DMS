
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DMS.WPF.Helper
{
    public static class VisualTreeFinder
    {
        /// <summary>
        /// (按名称查找) 在可视化树中查找指定名称的特定类型 T 的子元素。
        /// </summary>
        /// <typeparam name="T">要查找的元素的类型，必须是 FrameworkElement。</typeparam>
        /// <param name="parent">要开始查找的父元素。</param>
        /// <param name="name">要查找的元素的 Name 属性。</param>
        /// <returns>找到的第一个匹配的元素，如果未找到则返回 null。</returns>
        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // 检查当前子元素是否是我们要找的目标
                if (child is T frameworkElement && frameworkElement.Name == name)
                {
                    return frameworkElement;
                }

                // 如果不是，则递归进入子元素的子级中查找
                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                {
                    // 如果在子级中找到了，立即返回结果，停止继续搜索
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// (非泛型) 查找指定元素的所有可视化子元素。
        /// </summary>
        /// <param name="parent">要开始查找的父元素。</param>
        /// <returns>一个包含所有找到的子元素的 IEnumerable<DependencyObject> 集合。</returns>
        public static IEnumerable<DependencyObject> FindAllVisualChildren(DependencyObject parent)
        {
            if (parent == null)
                yield break;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                // 先返回直接子元素
                yield return child;

                // 然后递归查找并返回其所有子元素
                foreach (DependencyObject nestedChild in FindAllVisualChildren(child))
                {
                    yield return nestedChild;
                }
            }
        }

        /// <summary>
        /// (泛型) 查找指定元素的可视化子元素中所有符合条件的特定类型 T 的元素。
        /// </summary>
        /// <typeparam name="T">要查找的子元素的类型，必须是 DependencyObject。</typeparam>
        /// <param name="parent">要开始查找的父元素。</param>
        /// <returns>一个包含所有找到的子元素的 IEnumerable<T> 集合。</returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            // 复用 FindAllVisualChildren 的逻辑，避免代码重复
            foreach (var child in FindAllVisualChildren(parent))
            {
                if (child is T typedChild)
                {
                    yield return typedChild;
                }
            }
        }
    }
}
