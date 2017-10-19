namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _71 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "CpuFixed", c => c.String());
            AddColumn("dbo.Computers", "RamFixed", c => c.Int(nullable: false));
            AddColumn("dbo.Computers", "HddFixed", c => c.String());
            AddColumn("dbo.Computers", "MotherBoardFixed", c => c.String());
            AddColumn("dbo.Computers", "VideoAdapterFixed", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Computers", "VideoAdapterFixed");
            DropColumn("dbo.Computers", "MotherBoardFixed");
            DropColumn("dbo.Computers", "HddFixed");
            DropColumn("dbo.Computers", "RamFixed");
            DropColumn("dbo.Computers", "CpuFixed");
        }
    }
}
