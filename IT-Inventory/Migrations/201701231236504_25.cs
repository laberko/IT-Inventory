namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _25 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PrinterItems", "Printer_Id", "dbo.Printers");
            DropForeignKey("dbo.PrinterItems", "Item_Id", "dbo.Items");
            DropForeignKey("dbo.Printers", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.Printers", "Office_Id", "dbo.Offices");
            DropIndex("dbo.Printers", new[] { "Department_Id" });
            DropIndex("dbo.Printers", new[] { "Office_Id" });
            DropIndex("dbo.PrinterItems", new[] { "Printer_Id" });
            DropIndex("dbo.PrinterItems", new[] { "Item_Id" });
            DropTable("dbo.Printers");
            DropTable("dbo.PrinterItems");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PrinterItems",
                c => new
                    {
                        Printer_Id = c.Int(nullable: false),
                        Item_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Printer_Id, t.Item_Id });
            
            CreateTable(
                "dbo.Printers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Ip = c.String(),
                        Place = c.String(),
                        Department_Id = c.Int(),
                        Office_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.PrinterItems", "Item_Id");
            CreateIndex("dbo.PrinterItems", "Printer_Id");
            CreateIndex("dbo.Printers", "Office_Id");
            CreateIndex("dbo.Printers", "Department_Id");
            AddForeignKey("dbo.Printers", "Office_Id", "dbo.Offices", "Id");
            AddForeignKey("dbo.Printers", "Department_Id", "dbo.Departments", "Id");
            AddForeignKey("dbo.PrinterItems", "Item_Id", "dbo.Items", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PrinterItems", "Printer_Id", "dbo.Printers", "Id", cascadeDelete: true);
        }
    }
}
