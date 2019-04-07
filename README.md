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









