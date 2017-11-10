namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _73 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ItemPrinters", newName: "PrinterItems");
            DropPrimaryKey("dbo.PrinterItems");
            AddColumn("dbo.Departments", "Order", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PrinterItems", new[] { "Printer_Id", "Item_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.PrinterItems");
            DropColumn("dbo.Departments", "Order");
            AddPrimaryKey("dbo.PrinterItems", new[] { "Item_Id", "Printer_Id" });
            RenameTable(name: "dbo.PrinterItems", newName: "ItemPrinters");
        }
    }
}
