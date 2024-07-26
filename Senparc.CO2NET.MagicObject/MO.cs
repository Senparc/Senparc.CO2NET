using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Senparc.CO2NET.MagicObject
{
    /// <summary>  
    /// 表示属性值变更的结果。  
    /// </summary>  
    /// <typeparam name="TValue">属性值的类型。</typeparam>  
    public class PropertyChangeResult<TValue>
    {
        /// <summary>  
        /// 获取或设置属性的旧值。  
        /// </summary>  
        public TValue OldValue { get; set; }

        /// <summary>  
        /// 获取或设置属性的新值。  
        /// </summary>  
        public TValue NewValue { get; set; }

        /// <summary>  
        /// 获取或设置一个值，该值指示属性是否已更改。  
        /// </summary>  
        public bool IsChanged { get; set; }

        /// <summary>  
        /// 返回表示当前对象的字符串。  
        /// </summary>  
        /// <returns>表示当前对象的字符串。</returns>  
        public override string ToString()
        {
            return IsChanged ? $"{OldValue} -> {NewValue}" : NewValue?.ToString();
        }
    }

    /// <summary>  
    /// 跟踪对象属性值变更的类。  
    /// </summary>  
    /// <typeparam name="T">对象类型。</typeparam>  
    public class MO<T>
    {
        private T OriginalObject { get; set; }
        private T NewObject { get; set; }
        private Dictionary<string, PropertyChangeResult<object>> _changes;
        private T _snapshot;

        /// <summary>  
        /// 当属性值发生变化时触发的事件。  
        /// </summary>  
        public event EventHandler<string> PropertyChanged;

        /// <summary>  
        /// 初始化 <see cref="MO{T}"/> 类的新实例。  
        /// </summary>  
        /// <param name="obj">要跟踪的对象。</param>  
        public MO(T obj)
        {
            OriginalObject = obj;
            NewObject = Clone(obj);
            _changes = new Dictionary<string, PropertyChangeResult<object>>();
        }

        /// <summary>  
        /// 设置对象的属性值，并记录变更。  
        /// </summary>  
        /// <typeparam name="TValue">属性值的类型。</typeparam>  
        /// <param name="expression">用于指定属性的表达式。</param>  
        /// <param name="value">要设置的值。</param>  
        /// <returns>当前 <see cref="MO{T}"/> 实例。</returns>  
        /// <exception cref="ArgumentException">如果表达式不是一个属性访问表达式。</exception>  
        public MO<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            if (expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                var originalValue = (TValue)propertyInfo.GetValue(OriginalObject);
                var newValue = value;
                var propertyName = propertyInfo.Name;

                if (!Equals(originalValue, newValue))
                {
                    propertyInfo.SetValue(NewObject, newValue);
                    var changeResult = new PropertyChangeResult<object>
                    {
                        OldValue = originalValue,
                        NewValue = newValue,
                        IsChanged = true
                    };
                    _changes[propertyName] = changeResult;
                    PropertyChanged?.Invoke(this, propertyName);
                }
            }
            else
            {
                throw new ArgumentException("表达式必须是一个属性访问表达式", nameof(expression));
            }

            return this;
        }

        /// <summary>  
        /// 获取属性的变更结果。  
        /// </summary>  
        /// <typeparam name="TValue">属性值的类型。</typeparam>  
        /// <param name="expression">用于指定属性的表达式。</param>  
        /// <returns>表示属性变更结果的 <see cref="PropertyChangeResult{TValue}"/> 对象。</returns>  
        /// <exception cref="ArgumentException">如果表达式不是一个属性访问表达式。</exception>  
        public PropertyChangeResult<TValue> Get<TValue>(Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                var originalValue = (TValue)propertyInfo.GetValue(OriginalObject);
                var newValue = (TValue)propertyInfo.GetValue(NewObject);

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

        /// <summary>  
        /// 获取所有属性的变更记录。  
        /// </summary>  
        /// <returns>包含所有属性变更记录的字典。</returns>  
        public Dictionary<string, PropertyChangeResult<object>> GetChanges()
        {
            return _changes;
        }

        /// <summary>  
        /// 重置对象的状态，将 <see cref="NewObject"/> 重置为 <see cref="OriginalObject"/> 的状态，并清除变更记录。  
        /// </summary>  
        public void Reset()
        {
            NewObject = Clone(OriginalObject);
            _changes.Clear();
        }

        /// <summary>  
        /// 检测对象是否有任何属性发生变化。  
        /// </summary>  
        /// <returns>如果有属性发生变化，则为 <c>true</c>；否则为 <c>false</c>。</returns>  
        public bool HasChanges()
        {
            return _changes.Count > 0;
        }

        /// <summary>  
        /// 批量设置对象的属性值。  
        /// </summary>  
        /// <param name="properties">包含要设置的属性和值的字典。</param>  
        public void SetProperties(Dictionary<Expression<Func<T, object>>, object> properties)
        {
            foreach (var property in properties)
            {
                Set(property.Key, property.Value);
            }
        }

        /// <summary>  
        /// 创建对象的快照。  
        /// </summary>  
        public void TakeSnapshot()
        {
            _snapshot = Clone(NewObject);
        }

        /// <summary>  
        /// 恢复到之前创建的快照。  
        /// </summary>  
        public void RestoreSnapshot()
        {
            if (_snapshot != null)
            {
                NewObject = Clone(_snapshot);
                _changes.Clear();
            }
        }

        /// <summary>  
        /// 创建对象的浅表副本。  
        /// </summary>  
        /// <param name="source">要克隆的对象。</param>  
        /// <returns>对象的浅表副本。</returns>  
        private T Clone(T source)
        {
            var cloneMethod = source.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)cloneMethod.Invoke(source, null);
        }
    }
}
