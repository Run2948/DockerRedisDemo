# Docker 和 ASP.NET Core Web 应用 --- Redis 入门篇

## 环境搭建
#### 1.在Docker容器中安装Redis服务镜像
* 1.进入[Docker Hub](https://hub.docker.com/)官网，搜索 redis
![](./docs/1.png)
* 2.点击搜索结果 [redis](https://hub.docker.com/_/redis) 进入详情页面，下拉找到<b>`How to use this image`</b>节点
![](./docs/2.png)
* 3.打开cmd窗口，执行<b>`docker run --name asp-redis -it -p 6379:6379 redis`</b>命令来拉取并安装redis服务
![](./docs/3.png)
* 4.此时redis服务安装并启动完成，我们打开新的cmd窗口，执行<b>`docker ps -a`</b>来查看正在运行的docker镜像
![](./docs/4.png)
* 5.执行<b>`docker exec -it asp-redis redis-cli`</b>，使用`redis-cli`连接到redis服务端
![](./docs/5.png)
* 6.执行一些基本redis操作命令，测试redis服务的正常搭建
![](./docs/6.png)

## 项目搭建
#### 1.新建一个ASP.NET Core Web 应用
* 7.新建一个ASP.NET Core Mvc 模板项目 RedisDemo
![](./docs/7.png)
* 8.为当前项目添加Redis操作的相关依赖:`Microsoft.Extensions.Caching.Redis`*
![](./docs/8.png)
* 9.在`Startup:ConfigureServices`中依赖注入Redis连接配置:
```csharp
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("RedisConnection")));
```
* 其中`appsetting.json`中Redis连接字符串`RedisConnection`的内容:
```json
{
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379,password="
  }
}
```
* 10 在项目中使用连接对象操作Redis数据库
```csharp
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
```

## 案例展示

#### 1.使用Redis实现页面访问量的统计
![](./docs/example1-1.png)
![](./docs/example1-2.png)

#### 2.使用Redis实现产品访问量的统计
```csharp
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
```


#### 3.Redis分布式缓存的实现
* Startup:ConfigureServices 配置
```csharp
public class Startup
{
    ...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        services.AddDistributedRedisCache(options =>
        {
            options.Configuration = Configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "RedisDemoInstance:";// 最终会成为 redis key 的前缀
        });
    }

    ...
}
```
* Controller:Redis分布式的实现
```csharp
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
```





