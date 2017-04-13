using System.Data.Entity;

namespace IT_Inventory.Models
{
    public class InventoryModel : DbContext
    {
        public InventoryModel() : base("name=InventoryConnection")
        {
        }
        public DbSet<Item> Items { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<ItemAttribute> ItemAttributes { get; set; }
        public DbSet<ItemAttributeValue> ItemAttributeValues { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
        public DbSet<Computer> Computers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Item>()
                .HasMany(v => v.AttributeValues)
                .WithOptional()
                .WillCascadeOnDelete(true);
        }
    }
}