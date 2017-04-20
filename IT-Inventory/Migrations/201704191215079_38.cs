namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _38 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "CpuInvent", c => c.String());
            AddColumn("dbo.Computers", "RamInvent", c => c.Int(nullable: false));
            AddColumn("dbo.Computers", "MotherBoardInvent", c => c.String());
            AddColumn("dbo.Computers", "VideoAdapterInvent", c => c.String());
            AddColumn("dbo.Computers", "SoftwareInvent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Computers", "SoftwareInvent");
            DropColumn("dbo.Computers", "VideoAdapterInvent");
            DropColumn("dbo.Computers", "MotherBoardInvent");
            DropColumn("dbo.Computers", "RamInvent");
            DropColumn("dbo.Computers", "CpuInvent");
        }
    }
}
