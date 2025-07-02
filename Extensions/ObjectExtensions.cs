using System.Collections;
using System.Reflection;

namespace PMSWPF.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// 复制一个新的对象并返回
    /// </summary>
    /// <param name="source">源对象</param>
    /// <typeparam name="T">新的对象类型</typeparam>
    /// <returns>新的对象</returns>
    public static T CopyTo<T>(this Object source) where T : new()
    {
        T t = new T();
        CopyTo(source, t);
        return t;
    }

    /// <summary>
    /// 将可读写的公共属性值从源对象复制到目标对象。
    /// 属性名称和类型必须匹配，或者它们是具有匹配元素名称的泛型List类型。
    /// 对嵌套对象和列表执行深拷贝。
    /// </summary>
    /// <param name="tsource">源对象。</param>
    /// <param name="ttarget">目标对象。</param>
    public static void CopyTo(this object tsource, object ttarget)
    {
        // 1. 基本的空值检查，提高健壮性
        if (tsource == null)
        {
            // Console.WriteLine("源对象为空。无法复制。");
            throw new ArgumentNullException("源对象为空。无法复制。");
            return;
        }

        if (ttarget == null)
        {
            // Console.WriteLine("目标对象为空。无法复制。");
            throw new ArgumentNullException("目标对象为空。无法复制。");
            return;
        }

        Type sourceType = tsource.GetType();
        Type targetType = ttarget.GetType();

        // 2. 缓存源类型的公共实例属性，避免在循环中重复获取
        PropertyInfo[] sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo sourceProperty in sourceProperties)
        {
            // 确保源属性可读
            if (!sourceProperty.CanRead)
            {
                continue;
            }

            // 在目标类型中查找对应的属性
            PropertyInfo targetProperty =
                targetType.GetProperty(sourceProperty.Name, BindingFlags.Public | BindingFlags.Instance);

            // 确保目标属性存在且可写
            if (targetProperty != null && targetProperty.CanWrite)
            {
                object sourceValue = sourceProperty.GetValue(tsource);
                // 判断源属性和目标属性是否是泛型列表
                bool isSourceList = sourceProperty.PropertyType.IsGenericType &&
                                    sourceProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                bool isTargetList = targetProperty.PropertyType.IsGenericType &&
                                    targetProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

                // 场景 1: 属性类型完全相同
                if (targetProperty.PropertyType == sourceProperty.PropertyType)
                {
                    targetProperty.SetValue(ttarget, sourceValue);
                }
                // 场景 2: 属性类型不同，但可能是泛型 List<T> 类型
                else if (isTargetList && isSourceList)
                {
                    CopyGenericList(ttarget, sourceProperty, targetProperty, sourceValue);
                }
                // 场景 3: 属性类型不同，但是属性名称一样
                else
                {
                    var sObj = sourceProperty.GetValue(tsource);
                    if (sObj == null)
                    {
                        continue;
                    }
                    var tObj = targetProperty.GetValue(ttarget);
                    if (tObj == null)
                    {
                        tObj=Activator.CreateInstance(targetProperty.PropertyType);
                    }
                    
                    CopyTo(sObj,tObj);
                    targetProperty.SetValue(ttarget,tObj);
                }
            }
        }
    }

    /// <summary>
    /// 复制泛型列表，
    /// </summary>
    /// <param name="ttarget"></param>
    /// <param name="sourceProperty"></param>
    /// <param name="targetProperty"></param>
    /// <param name="sourceValue"></param>
    private static void CopyGenericList(object ttarget, PropertyInfo sourceProperty, PropertyInfo targetProperty,
        object? sourceValue)
    {
        // 获取源列表的元素类型
        Type sourceListItemType = sourceProperty.PropertyType.GetGenericArguments()[0];
        // 获取目标列表的元素类型
        Type targetListItemType = targetProperty.PropertyType.GetGenericArguments()[0];

        // 检查列表元素类型名称是否匹配（用于潜在的深拷贝）
        if (sourceProperty.Name == targetProperty.Name)
        {
            // 如果源列表为空，则将目标属性设置为 null
            if (sourceValue == null)
            {
                targetProperty.SetValue(ttarget, null);
                return;
            }

            // 将源值强制转换为 IEnumerable 以便遍历
            var sourceList = (IEnumerable)sourceValue;

            // 创建一个目标泛型列表的新实例
            // 假设 CreateGenericList 助手方法可用，或按注释创建
            // var targetList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetListItemType));
            IList targetList = CreateGenericList(targetListItemType);

            foreach (var item in sourceList)
            {
                // 确保列表项不为空，然后尝试深拷贝
                if (item == null)
                {
                    // 如果源列表项为 null，则直接添加到目标列表
                    targetList.Add(null);
                    continue;
                }

                // 创建目标列表元素类型的一个实例
                var targetDataItem = Activator.CreateInstance(targetListItemType);

                // 递归调用 CopyTo，对嵌套对象进行深拷贝
                CopyTo(item, targetDataItem);

                // 将复制后的项添加到目标列表
                targetList.Add(targetDataItem);
            }

            // 将填充好的目标列表设置给目标对象的属性
            targetProperty.SetValue(ttarget, targetList);
        }
        else
        {
            // 列表类型匹配，但元素类型名称不匹配。此处跳过，或可实现更复杂的转换。
            Console.WriteLine(
                $"由于列表元素类型不匹配，跳过属性 '{sourceProperty.Name}'：源 '{sourceListItemType.Name}'，目标 '{targetListItemType.Name}'");
        }
    }

    /// <summary>
    /// 辅助方法，用于动态创建泛型 List<T>。
    /// </summary>
    /// <param name="itemType">列表应包含的元素类型。</param>
    /// <returns>List<itemType> 的 IList 实例。</returns>
    private static IList CreateGenericList(Type itemType)
    {
        Type listType = typeof(List<>).MakeGenericType(itemType);
        return (IList)Activator.CreateInstance(listType);
    }
}