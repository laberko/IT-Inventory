namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _34 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SupportRequests", "Comment", c => c.String());
            AddColumn("dbo.SupportRequests", "Mark", c => c.Int(nullable: false));
            AddColumn("dbo.SupportRequests", "FeedBack", c => c.String());
            AddColumn("dbo.SupportRequests", "CreationTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.SupportRequests", "StartTime", c => c.DateTime());
            AddColumn("dbo.SupportRequests", "FinishTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SupportRequests", "FinishTime");
            DropColumn("dbo.SupportRequests", "StartTime");
            DropColumn("dbo.SupportRequests", "CreationTime");
            DropColumn("dbo.SupportRequests", "FeedBack");
            DropColumn("dbo.SupportRequests", "Mark");
            DropColumn("dbo.SupportRequests", "Comment");
        }
    }
}
