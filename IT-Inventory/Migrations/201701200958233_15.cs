namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _15 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemAttributeValues", "ParentItem_Id", "dbo.Items");
            AddColumn("dbo.ItemAttributeValues", "Item_Id", c => c.Int());
            CreateIndex("dbo.ItemAttributeValues", "Item_Id");
            AddForeignKey("dbo.ItemAttributeValues", "Item_Id", "dbo.Items", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItemAttributeValues", "Item_Id", "dbo.Items");
            DropIndex("dbo.ItemAttributeValues", new[] { "Item_Id" });
            DropColumn("dbo.ItemAttributeValues", "Item_Id");
            AddForeignKey("dbo.ItemAttributeValues", "ParentItem_Id", "dbo.Items", "Id");
        }
    }
}
