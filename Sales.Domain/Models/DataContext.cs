namespace Sales.Domain.Models
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Models;

    public class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection")
        {
        }

        public IQueryable<Product> Products { get; set; }
    }
}
