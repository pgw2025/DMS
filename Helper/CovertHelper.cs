using System.Reflection;

namespace PMSWPF.Helper;

public class CovertHelper
{
    public static List<TTarget> ConvertList<TSource, TTarget>(List<TSource> sourceList)
    {
        List<TTarget> targetList = new List<TTarget>();
        Type sourceType = typeof(TSource);
        Type targetType = typeof(TTarget);

        // 获取源类型和目标类型的公共属性
        PropertyInfo[] sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo[] targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (TSource sourceObject in sourceList)
        {
            TTarget targetObject = Activator.CreateInstance<TTarget>();
            foreach (PropertyInfo targetProperty in targetProperties)
            {
                PropertyInfo sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == targetProperty.Name && p.PropertyType == targetProperty.PropertyType);
                if (sourceProperty!= null)
                {
                    object value = sourceProperty.GetValue(sourceObject);
                    targetProperty.SetValue(targetObject, value);
                }
            }
            targetList.Add(targetObject);
        }
        return targetList;
    }
}