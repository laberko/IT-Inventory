namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cartridges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Color = c.String(),
                        Quantity = c.Int(nullable: false),
                        Printer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Printers", t => t.Printer_Id)
                .Index(t => t.Printer_Id);
            
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        WhoGave_Id = c.Int(),
                        WhoTook_Id = c.Int(),
                        Cartridge_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.People", t => t.WhoGave_Id)
                .ForeignKey("dbo.People", t => t.WhoTook_Id)
                .ForeignKey("dbo.Cartridges", t => t.Cartridge_Id)
                .Index(t => t.WhoGave_Id)
                .Index(t => t.WhoTook_Id)
                .Index(t => t.Cartridge_Id);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(),
                        IsItUser = c.Boolean(nullable: false),
                        Department_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departments", t => t.Department_Id)
                .Index(t => t.Department_Id);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Office_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Offices", t => t.Office_Id)
                .Index(t => t.Office_Id);
            
            CreateTable(
                "dbo.Offices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Printers", "Office_Id", "dbo.Offices");
            DropForeignKey("dbo.Printers", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.Cartridges", "Printer_Id", "dbo.Printers");
            DropForeignKey("dbo.Histories", "Cartridge_Id", "dbo.Cartridges");
            DropForeignKey("dbo.Histories", "WhoTook_Id", "dbo.People");
            DropForeignKey("dbo.Histories", "WhoGave_Id", "dbo.People");
            DropForeignKey("dbo.People", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.Departments", "Office_Id", "dbo.Offices");
            DropIndex("dbo.Printers", new[] { "Office_Id" });
            DropIndex("dbo.Printers", new[] { "Department_Id" });
            DropIndex("dbo.Departments", new[] { "Office_Id" });
            DropIndex("dbo.People", new[] { "Department_Id" });
            DropIndex("dbo.Histories", new[] { "Cartridge_Id" });
            DropIndex("dbo.Histories", new[] { "WhoTook_Id" });
            DropIndex("dbo.Histories", new[] { "WhoGave_Id" });
            DropIndex("dbo.Cartridges", new[] { "Printer_Id" });
            DropTable("dbo.Printers");
            DropTable("dbo.Offices");
            DropTable("dbo.Departments");
            DropTable("dbo.People");
            DropTable("dbo.Histories");
            DropTable("dbo.Cartridges");
        }
    }
}
