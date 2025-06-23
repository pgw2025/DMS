namespace PMSWPF.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    ///     对象转换，将source对象上的所有属性的值，都转换到target对象上
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public static void CopyTo<T>(this object source, T target)
    {
        var sourceType = source.GetType();
        var targetType = target.GetType();
        var sourceProperties = sourceType.GetProperties();
        foreach (var sourceProperty in sourceProperties)
        {
            var targetProperty = targetType.GetProperty(sourceProperty.Name);
            if (targetProperty != null && targetProperty.CanWrite && sourceProperty.CanRead &&
                targetProperty.PropertyType == sourceProperty.PropertyType)
            {
                var value = sourceProperty.GetValue(source, null);
                targetProperty.SetValue(target, value, null);
            }
        }
    }

    /// <summary>
    ///     创建一个泛型对象，将source对象上的所有属性的值，都转换到新创建对象上
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    public static T NewTo<T>(this object source) where T : new()
    {
        var target = new T();
        var sourceType = source.GetType();
        var targetType = target.GetType();
        var sourceProperties = sourceType.GetProperties();
        foreach (var sourceProperty in sourceProperties)
        {
            var targetProperty = targetType.GetProperty(sourceProperty.Name);
            if (targetProperty != null && targetProperty.CanWrite && sourceProperty.CanRead &&
                targetProperty.PropertyType == sourceProperty.PropertyType)
            {
                var value = sourceProperty.GetValue(source, null);
                targetProperty.SetValue(target, value, null);
            }
        }

        return target;
    }
}