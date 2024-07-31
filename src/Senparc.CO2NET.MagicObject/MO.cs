#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc
  
    文件名：Enums.cs
    文件功能描述：枚举配置文件
    
    
    创建标识：MO - 20240730
 
    修改标识：Senparc - 20240731
    修改描述：v0.0.2 将 MO.OriginObject 和 MO.Object 属性设为 public

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Senparc.CO2NET.MagicObject
{
    /// <summary>
    /// MagicObject 对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MO<T>
        //where T:class
    {
        public T OriginalObject { get; set; }
        public T Object { get; set; }
        private Dictionary<string, PropertyChangeResult<object>> _changes;
        private T _snapshot;

        public event EventHandler<string> PropertyChanged;

        public MO(T obj)
        {
            OriginalObject = Clone(obj);
            Object = obj;
            _changes = new Dictionary<string, PropertyChangeResult<object>>();
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression">需要设置属性的对象</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public MO<T> Set<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        {
            MemberExpression memberExpression = null;
            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else if (expression.Body is MemberExpression)
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression != null && memberExpression.Member is PropertyInfo propertyInfo)
            {
                var originalValue = (TValue)propertyInfo.GetValue(OriginalObject);
                var newValue = value;
                var propertyName = propertyInfo.Name;

                if (!Equals(originalValue, newValue))
                {
                    propertyInfo.SetValue(Object, newValue);
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
        /// 获取属性值
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression">需要获取的属性</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public PropertyChangeResult<TValue> Get<TValue>(Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                var hasSnapshot = _snapshot != null;

                var originalValue = (TValue)propertyInfo.GetValue(OriginalObject);

                TValue? snapshptValue = hasSnapshot ? (TValue?)propertyInfo.GetValue(_snapshot) : default;
                var newValue = (TValue)propertyInfo.GetValue(Object);

                return new PropertyChangeResult<TValue>
                {
                    OldValue = originalValue,
                    SnapshotValue = snapshptValue,
                    NewValue = newValue,
                    IsChanged = /*hasSnapshot ? Equals(snapshptValue, newValue) :*/ !Equals(originalValue, newValue),
                    HasShapshot = hasSnapshot
                };
            }
            else
            {
                throw new ArgumentException("表达式必须是一个属性访问表达式", nameof(expression));
            }
        }

        /// <summary>
        /// 获取所有的修改
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, PropertyChangeResult<object>> GetChanges()
        {
            return _changes;
        }

        /// <summary>
        /// 使用旧对象的副本覆盖当前对象（注意将丢失对原有对象的引用，请慎重使用）
        /// <para>如果不希望丢失对原有对象的引用，请使用 <see cref="RevertChanges"/> 方法</para>
        /// </summary>
        public void Reset()
        {
            Object = Clone(OriginalObject);
            _changes.Clear();
        }

        /// <summary>
        /// 检查是否具有修改
        /// </summary>
        /// <returns></returns>
        public bool HasChanges()
        {
            return _changes.Count > 0;
        }

        /// <summary>
        /// 批量设置属性值
        /// </summary>
        /// <param name="properties"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetProperties(Dictionary<Expression<Func<T, object>>, object> properties)
        {
            foreach (var property in properties)
            {
                if (property.Key.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpression)
                {
                    Set(Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpression, typeof(object)), property.Key.Parameters), property.Value);
                }
                else if (property.Key.Body is MemberExpression memberExpression2)
                {
                    Set(Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpression2, typeof(object)), property.Key.Parameters), property.Value);
                }
                else
                {
                    throw new ArgumentException("表达式必须是一个属性访问表达式", nameof(property.Key));
                }
            }
        }


        /// <summary>
        /// 获取快照
        /// </summary>
        public void TakeSnapshot()
        {
            _snapshot = Clone(Object);
        }

        /// <summary>
        /// 将快照还原到对象
        /// </summary>
        /// <param name="keepObjectInstance">是否保持当前对象引用，如果为否，将使用 Clone 的旧对象</param>
        public void RestoreSnapshot(bool keepObjectInstance = true)
        {
            if (_snapshot != null)
            {
                if (keepObjectInstance)
                {
                    RevertProperties(Object, _snapshot);
                }
                else
                {
                    Object = Clone(_snapshot);
                }
                _changes.Clear();
            }
        }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private T Clone(T source)
        {
            var cloneMethod = source.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)cloneMethod.Invoke(source, null);
        }

        private void RevertProperties(T target,T originalObject)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var originalValue = property.GetValue(originalObject);
                    property.SetValue(Object, originalValue);
                }
            }
        }

        /// <summary>
        /// 在当前引用对象基础上，还原所有修改（不会改变引用对象）
        /// </summary>
        public void RevertChanges()
        {
            RevertProperties(Object, OriginalObject);
            _changes.Clear();
        }
    }
}
