namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _17 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Items", "Printer_Id", "dbo.Printers");
            DropForeignKey("dbo.Printers", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.Printers", "Office_Id", "dbo.Offices");
            DropIndex("dbo.Printers", new[] { "Department_Id" });
            DropIndex("dbo.Printers", new[] { "Office_Id" });
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Items", "Printer_Id", c => c.Int());
            CreateIndex("dbo.Printers", "Office_Id");
            CreateIndex("dbo.Printers", "Department_Id");
            CreateIndex("dbo.Items", "Printer_Id");
            AddForeignKey("dbo.Printers", "Office_Id", "dbo.Offices", "Id");
            AddForeignKey("dbo.Printers", "Department_Id", "dbo.Departments", "Id");
            AddForeignKey("dbo.Items", "Printer_Id", "dbo.Printers", "Id");
        }
    }
}
