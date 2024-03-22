namespace DependencyInjectionDemo.Logic
{
    public class BetterDemoLogic : IDemoLogic
    {
        public int Value1 { get; set; }

        public int Value2 { get; set; }

        public BetterDemoLogic()
        {
            Value1 = 25;
            Value2 = 50;
        }
    }
}