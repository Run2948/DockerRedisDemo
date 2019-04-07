# Docker �� ASP.NET Core Web Ӧ�� --- Redis ����ƪ

## �����
#### 1.��Docker�����а�װRedis������
* 1.����[Docker Hub](https://hub.docker.com/)���������� redis
![](./docs/1.png)
* 2.���������� [redis](https://hub.docker.com/_/redis) ��������ҳ�棬�����ҵ�<b>`How to use this image`</b>�ڵ�
![](./docs/2.png)
* 3.��cmd���ڣ�ִ��<b>`docker run --name asp-redis -it -p 6379:6379 redis`</b>��������ȡ����װredis����
![](./docs/3.png)
* 4.��ʱredis����װ��������ɣ����Ǵ��µ�cmd���ڣ�ִ��<b>`docker ps -a`</b>���鿴�������е�docker����
![](./docs/4.png)
* 5.ִ��<b>`docker exec -it asp-redis redis-cli`</b>��ʹ��`redis-cli`���ӵ�redis�����
![](./docs/5.png)
* 6.ִ��һЩ����redis�����������redis����������
![](./docs/6.png)

## ��Ŀ�
#### 1.�½�һ��ASP.NET Core Web Ӧ��
* 7.�½�һ��ASP.NET Core Mvc ģ����Ŀ RedisDemo
![](./docs/7.png)
* 8.Ϊ��ǰ��Ŀ���Redis�������������:`Microsoft.Extensions.Caching.Redis`*
![](./docs/8.png)
* 9.��`Startup:ConfigureServices`������ע��Redis��������:
```csharp
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetConnectionString("RedisConnection")));
```
* ����`appsetting.json`��Redis�����ַ���`RedisConnection`������:
```json
{
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379,password="
  }
}
```
* 10 ����Ŀ��ʹ�����Ӷ������Redis���ݿ�
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

## ����չʾ

#### 1.ʹ��Redisʵ��ҳ���������ͳ��
![](./docs/example1-1.png)
![](./docs/example1-2.png)

#### 2.ʹ��Redisʵ�ֲ�Ʒ��������ͳ��
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
    var element = $"<div><strong>��Ʒ:{product.Name}(�����{viewCount}��)</strong><img src='{product.Url}' alt='{product.Name}' width='50' height='50'/></div>";
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


#### 3.Redis�ֲ�ʽ�����ʵ��
* Startup:ConfigureServices ����
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
            options.InstanceName = "RedisDemoInstance:";// ���ջ��Ϊ redis key ��ǰ׺
        });
    }

    ...
}
```
* Controller:Redis�ֲ�ʽ��ʵ��
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





