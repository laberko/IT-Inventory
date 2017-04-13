namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _30 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Computers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComputerName = c.String(),
                        Cpu = c.String(),
                        Ram = c.Int(nullable: false),
                        MotherBoard = c.String(),
                        VideoAdapter = c.String(),
                        Software = c.String(),
                        Owner_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.People", t => t.Owner_Id)
                .Index(t => t.Owner_Id);
            
            AddColumn("dbo.SupportRequests", "State", c => c.Int(nullable: false));
            AddColumn("dbo.SupportRequests", "SoftwareInstalled", c => c.String());
            AddColumn("dbo.SupportRequests", "SoftwareRepaired", c => c.String());
            AddColumn("dbo.SupportRequests", "SoftwareUpdated", c => c.String());
            AddColumn("dbo.SupportRequests", "SoftwareRemoved", c => c.String());
            AddColumn("dbo.SupportRequests", "HardwareInstalled", c => c.String());
            AddColumn("dbo.SupportRequests", "HardwareReplaced", c => c.String());
            AddColumn("dbo.SupportRequests", "OtherActions", c => c.String());
            AddColumn("dbo.SupportRequests", "FromComputer_Id", c => c.Int());
            AddColumn("dbo.SupportRequests", "Person_Id", c => c.Int());
            CreateIndex("dbo.SupportRequests", "FromComputer_Id");
            CreateIndex("dbo.SupportRequests", "Person_Id");
            AddForeignKey("dbo.SupportRequests", "FromComputer_Id", "dbo.Computers", "Id");
            AddForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People");
            DropForeignKey("dbo.SupportRequests", "FromComputer_Id", "dbo.Computers");
            DropForeignKey("dbo.Computers", "Owner_Id", "dbo.People");
            DropIndex("dbo.SupportRequests", new[] { "Person_Id" });
            DropIndex("dbo.SupportRequests", new[] { "FromComputer_Id" });
            DropIndex("dbo.Computers", new[] { "Owner_Id" });
            DropColumn("dbo.SupportRequests", "Person_Id");
            DropColumn("dbo.SupportRequests", "FromComputer_Id");
            DropColumn("dbo.SupportRequests", "OtherActions");
            DropColumn("dbo.SupportRequests", "HardwareReplaced");
            DropColumn("dbo.SupportRequests", "HardwareInstalled");
            DropColumn("dbo.SupportRequests", "SoftwareRemoved");
            DropColumn("dbo.SupportRequests", "SoftwareUpdated");
            DropColumn("dbo.SupportRequests", "SoftwareRepaired");
            DropColumn("dbo.SupportRequests", "SoftwareInstalled");
            DropColumn("dbo.SupportRequests", "State");
            DropTable("dbo.Computers");
        }
    }
}
