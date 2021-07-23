using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Everyday.Common
{
    public class ObjectComparer
    {
        public static PropertyCompareResult[] Compare<T>(T oldObject, T newObject, string propName = "")
        {
            List<PropertyCompareResult> results = new List<PropertyCompareResult>();
            var type = oldObject.GetType();

            if (IsEnumerable(type))
            {
                if (oldObject is IDictionary oldDic)
                {
                    var newDic = newObject as IDictionary;
                    if (!Enumerable.SequenceEqual(oldDic.Keys.Cast<object>(), newDic.Keys.Cast<object>()))
                        results.Add(new PropertyCompareResult(propName, oldDic, newDic));
                    else foreach (var key in oldDic.Keys)
                            results.AddRange(Compare(oldDic[key], newDic[key], $"{propName}[{key}]"));
                }
                else
                {
                    IEnumerable<object> oldArr = oldObject as IEnumerable<object>;
                    IEnumerable<object> newArr = newObject as IEnumerable<object>;

                    if (oldArr.Count() != newArr.Count())
                        results.Add(new PropertyCompareResult(propName, oldArr, newArr));
                    else for (int i = 0; i < oldArr.Count(); i++)
                            results.AddRange(Compare(oldArr.ElementAt(i), newArr.ElementAt(i), $"{propName}[{i}]"));
                }
            }
            else if (HasPropertiesToFetch(type))
            {
                foreach (PropertyInfo propInfo in type.GetProperties())
                {
                    var nextPropName = propName != "" ? $"{propName}.{propInfo.Name}" : propInfo.Name;
                    results.AddRange(Compare(propInfo.GetValue(oldObject), propInfo.GetValue(newObject), nextPropName));
                }
            }
            else
            {
                // End of node
                if (!Equals(oldObject, newObject))
                    results.Add(new PropertyCompareResult(propName, oldObject, newObject));
            }


            return results.ToArray();
        }

        private static bool IsEnumerable(Type t) => (t.IsArray || typeof(ICollection).IsAssignableFrom(t)) && t.FullName != "System.String";

        private static bool HasPropertiesToFetch(Type t) => t.IsClass && !t.IsValueType && !t.IsPrimitive && t.FullName != "System.String";
    }

    public class PropertyCompareResult : ValueObject
    {
        public string Name { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }

        public PropertyCompareResult(string name, object oldValue, object newValue)
        {
            Name = name;
            OldValue = oldValue;
            NewValue = newValue;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return OldValue;
            yield return NewValue;
        }
    }
}
