using System.Linq.Expressions;
using System.Reflection;

namespace Senparc.CO2NET.MagicObject
{
    public class PropertyChangeResult<TValue>
    {
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }
        public bool IsChanged { get; set; }

        public override string ToString()
        {
            return IsChanged ? $"{OldValue} -> {NewValue}" : NewValue?.ToString();
        }
    }

    public class MO<T>
    {
        private T OriginalObject { get; set; }
        private T NewObject { get; set; }

        public MO(T obj)
        {
            OriginalObject = obj;
            NewObject = obj;
        }

        public PropertyChangeResult<TValue> Get<TValue>(Expression<Func<T, TValue>> expression)
        {
            // 解析表达式，获取属性信息  
            if (expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                // 获取属性值  
                var originalValue = (TValue)propertyInfo.GetValue(OriginalObject);
                var newValue = (TValue)propertyInfo.GetValue(NewObject);

                // 创建并返回结果对象  
                return new PropertyChangeResult<TValue>
                {
                    OldValue = originalValue,
                    NewValue = newValue,
                    IsChanged = !Equals(originalValue, newValue)
                };
            }
            else
            {
                throw new ArgumentException("表达式必须是一个属性访问表达式", nameof(expression));
            }
        }

    }
}
