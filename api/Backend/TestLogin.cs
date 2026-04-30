using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    class TestLogin
    {
        static async Task Main(string[] args)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5200");

            // 准备登录数据
            var loginData = new {
                username = "admin",
                password = "123456",
                remember = false
            };

            var json = System.Text.Json.JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 发送登录请求
            var response = await client.PostAsync("/api/Auth/login", content);

            // 读取响应
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"状态码: {response.StatusCode}");
            Console.WriteLine($"响应内容: {responseContent}");
        }
    }
}