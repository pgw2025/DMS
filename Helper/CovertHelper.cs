using System.Reflection;

namespace PMSWPF.Helper;

public class CovertHelper
{
    public static List<TTarget> ConvertList<TSource, TTarget>(List<TSource> sourceList)
    {
        var targetList = new List<TTarget>();
        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);

        // 获取源类型和目标类型的公共属性
        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceObject in sourceList)
        {
            var targetObject = Activator.CreateInstance<TTarget>();
            foreach (var targetProperty in targetProperties)
            {
                var sourceProperty = sourceProperties.FirstOrDefault(p =>
                    p.Name == targetProperty.Name && p.PropertyType == targetProperty.PropertyType);
                if (sourceProperty != null)
                {
                    var value = sourceProperty.GetValue(sourceObject);
                    targetProperty.SetValue(targetObject, value);
                }
            }

            targetList.Add(targetObject);
        }

        return targetList;
    }
}