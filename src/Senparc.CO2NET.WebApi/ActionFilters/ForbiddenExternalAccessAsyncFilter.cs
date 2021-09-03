using Microsoft.AspNetCore.Mvc.Filters;
using Senparc.CO2NET.WebApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Senparc.CO2NET.WebApi.ActionFilters
{
    /// <summary>
    /// 外部访问屏蔽特性
    /// </summary>
    public class ForbiddenExternalAccessAsyncFilter : IAsyncActionFilter
    {
        private readonly bool _forbiddenExternalAccess;

        public ForbiddenExternalAccessAsyncFilter()
        {
            _forbiddenExternalAccess = Register.ForbiddenExternalAccess;
        }

        public ForbiddenExternalAccessAsyncFilter(bool forbiddenExternalAccess)
        {
            this._forbiddenExternalAccess = forbiddenExternalAccess;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_forbiddenExternalAccess && !context.HttpContext.Request.IsLocal())
            {
                throw new ForbiddenExternalAccessException();
            }
            await next();
        }
    }

    // 二选一

    //public class ForbiddenExternalAccessFilter : IActionFilter
    //{
    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        if (!Register.ForbiddenExternalAccess && context.HttpContext.Request.IsLocal())
    //        {
    //            throw new ForbiddenExternalAccessException();
    //        }
    //    }

    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        if (!Register.ForbiddenExternalAccess && context.HttpContext.Request.IsLocal())
    //        {
    //            throw new ForbiddenExternalAccessException();
    //        }
    //    }
    //}
}
