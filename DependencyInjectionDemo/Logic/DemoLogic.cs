namespace DependencyInjectionDemo.Logic
{
    public class DemoLogic : IDemoLogic
    {
        private readonly ILogger<DemoLogic> _logger;
        public int Value1 { get; set; }

        public int Value2 { get; set; }

        public DemoLogic(ILogger<DemoLogic> logger)
        {
            _logger = logger;
            Value1 = Random.Shared.Next(1, 1001);
            Value2 = Random.Shared.Next(1, 1001);
        }
    }
}