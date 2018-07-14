using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.Redis
{
    public interface IRedisObjectCacheStrategy : IBaseCacheLock, IBaseObjectCacheStrategy
    {
        ConnectionMultiplexer Client { get; set; }
    }
}
