namespace GeekBurger.Products.Service
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; }
        public string ProductsPubQueue { get; set; }
        public string ProductsSubQueue { get; set; }
    }
}