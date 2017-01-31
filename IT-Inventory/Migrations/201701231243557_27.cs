namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _27 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PrinterItems", newName: "ItemPrinters");
            DropPrimaryKey("dbo.ItemPrinters");
            AddPrimaryKey("dbo.ItemPrinters", new[] { "Item_Id", "Printer_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.ItemPrinters");
            AddPrimaryKey("dbo.ItemPrinters", new[] { "Printer_Id", "Item_Id" });
            RenameTable(name: "dbo.ItemPrinters", newName: "PrinterItems");
        }
    }
}
