/* ==============================================================================
* 命名空间：RedisDemo.ViewComponents 
* 类 名 称：CounterViewComponent
* 创 建 者：Qing
* 创建时间：2019/04/07 14:42:28
* CLR 版本：4.0.30319.42000
* 保存的文件名：CounterViewComponent
* 文件版本：V1.0.0.0
*
* 功能描述：N/A 
*
* 修改历史：
*
*
* ==============================================================================
*         CopyRight @ 班纳工作室 2019. All rights reserved
* ==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisDemo.ViewComponents
{
    public class CounterViewComponent : ViewComponent
    {
        private readonly IDatabase _db;

        public CounterViewComponent(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var controller = RouteData.Values["controller"].ToString();
            var action = RouteData.Values["action"].ToString();
            if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
            {
                var pageId = $"{controller}-{action}";
                await _db.StringIncrementAsync(pageId);
                var count = await _db.StringGetAsync(pageId);
                return View("_Default",pageId + ":" + count);
            }

            throw new Exception("Page Not Found...");
        }
    }
}
