using StackExchange.Redis;

namespace EX_Parcial_VilchezGuardia_JF.Services
{
    public class RedisTestService
    {
        public void Run()
        {
            var muxer = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { { "redis-16383.c81.us-east-1-2.ec2.redns.redis-cloud.com", 16383 } },
                    User = "default",
                    Password = "GHjIHaeHgF64swTlRpdSombXu48b8PDN",
                    Ssl = true,
                    AbortOnConnectFail = false
                }
            );

            var db = muxer.GetDatabase();
            db.StringSet("foo", "bar");
            var result = db.StringGet("foo");
            Console.WriteLine(result); // >>> bar
        }
    }
}
