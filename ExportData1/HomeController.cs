using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ExportData1
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private List<User> users = new List<User>()
        {
            new User {Id = 1, Age = 1},
            new User {Id = 2, Age = 10}
        };
        
        public IActionResult ExportToCSV()
        {
            var builder = new StringBuilder();

            builder.AppendLine("id, Age");

            foreach (var user in users)
            {
                builder.AppendLine($"{user.Id},{user.Age}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
    }
}
