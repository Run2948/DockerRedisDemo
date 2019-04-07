using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RedisDemo.Data;
using RedisDemo.Models;
using StackExchange.Redis;

namespace RedisDemo.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _db;

        private const string RecentViewedProducts = "recentViewedProducts";

        public ProductsController(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _db = redis.GetDatabase();
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            var vms = products.Select(x => new ProductViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                ViewCount = _db.StringGet($"products:{x.Id}:views")
            }).OrderByDescending(v => v.ViewCount);
            return View(vms);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var key = $"products:{id}:views";
            await _db.StringIncrementAsync(key);
            var viewCount = await _db.StringGetAsync(key);
            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Url = product.Url,
                ViewCount = viewCount
            };

            // add to redis list
            var element = $"<div><strong>产品:{product.Name}(已浏览{viewCount}次)</strong><img src='{product.Url}' alt='{product.Name}' width='50' height='50'/></div>";
            await _db.ListLeftPushAsync(RecentViewedProducts,element);

            // add to redis set
            var username = User.Identity.Name ?? "Anonymous";
            await _db.SetAddAsync("products:uniquevisitors",username);

            // add to redis sorted set
            //await _db.SortedSetAddAsync("products:views:leaderboard",element,(double)viewCount);
            await _db.SortedSetIncrementAsync("products:views:leaderboard",element,1d);

            return View(vm);
        }

        // GET: Products/Recent
        public async Task<IActionResult> Recent()
        {
            var vms = await _db.ListRangeAsync(RecentViewedProducts,0,4);
            return View(vms);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Url")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Url")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
