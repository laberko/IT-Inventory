namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _26 : DbMigration
    {
        public override void Up()
        {
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departments", t => t.Department_Id)
                .ForeignKey("dbo.Offices", t => t.Office_Id)
                .Index(t => t.Department_Id)
                .Index(t => t.Office_Id);
            
            CreateTable(
                "dbo.PrinterItems",
                c => new
                    {
                        Printer_Id = c.Int(nullable: false),
                        Item_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Printer_Id, t.Item_Id })
                .ForeignKey("dbo.Printers", t => t.Printer_Id, cascadeDelete: true)
                .ForeignKey("dbo.Items", t => t.Item_Id, cascadeDelete: true)
                .Index(t => t.Printer_Id)
                .Index(t => t.Item_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Printers", "Office_Id", "dbo.Offices");
            DropForeignKey("dbo.Printers", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.PrinterItems", "Item_Id", "dbo.Items");
            DropForeignKey("dbo.PrinterItems", "Printer_Id", "dbo.Printers");
            DropIndex("dbo.PrinterItems", new[] { "Item_Id" });
            DropIndex("dbo.PrinterItems", new[] { "Printer_Id" });
            DropIndex("dbo.Printers", new[] { "Office_Id" });
            DropIndex("dbo.Printers", new[] { "Department_Id" });
            DropTable("dbo.PrinterItems");
            DropTable("dbo.Printers");
        }
    }
}
