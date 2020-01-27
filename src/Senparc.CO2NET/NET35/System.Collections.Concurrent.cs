/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：System.Collections.Concurrent.cs
    文件功能描述：为 .net 3.5 提供对 System.Collections.Concurrent 命名空间下部分类的兼容

    创建标识：Senparc - 20190521

----------------------------------------------------------------*/

#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Concurrent
{
    public class ConcurrentDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
    }
}
#endif