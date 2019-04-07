using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisDemo.Data;

namespace RedisDemo.Controllers
{
    public class DistributedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public DistributedController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Distributed/Index
        public async Task<IActionResult> Index()
        {
            var key = "productList";

            var val = await _cache.GetAsync(key);
            if (val == null)
            {
                var obj = await _context.Products.ToListAsync();
                var str = JsonConvert.SerializeObject(obj);
                await _cache.SetAsync(key, Encoding.UTF8.GetBytes(str), new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30)));
                return View(obj);
            }
            else
            {
                var str = Encoding.UTF8.GetString(val);
                var obj = JsonConvert.DeserializeObject<List<Product>>(str);
                return View(obj);
            }
        }
    }
}