namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _7 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemAttributes", "Item_Id", "dbo.Items");
            DropIndex("dbo.ItemAttributes", new[] { "Item_Id" });
            AddColumn("dbo.ItemAttributes", "ItemType_Id", c => c.Int());
            AddColumn("dbo.ItemAttributeValues", "IsNumber", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItemAttributeValues", "Item_Id", c => c.Int());
            CreateIndex("dbo.ItemAttributeValues", "Item_Id");
            CreateIndex("dbo.ItemAttributes", "ItemType_Id");
            AddForeignKey("dbo.ItemAttributeValues", "Item_Id", "dbo.Items", "Id");
            AddForeignKey("dbo.ItemAttributes", "ItemType_Id", "dbo.ItemTypes", "Id");
            DropColumn("dbo.ItemAttributes", "Item_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItemAttributes", "Item_Id", c => c.Int());
            DropForeignKey("dbo.ItemAttributes", "ItemType_Id", "dbo.ItemTypes");
            DropForeignKey("dbo.ItemAttributeValues", "Item_Id", "dbo.Items");
            DropIndex("dbo.ItemAttributes", new[] { "ItemType_Id" });
            DropIndex("dbo.ItemAttributeValues", new[] { "Item_Id" });
            DropColumn("dbo.ItemAttributeValues", "Item_Id");
            DropColumn("dbo.ItemAttributeValues", "IsNumber");
            DropColumn("dbo.ItemAttributes", "ItemType_Id");
            CreateIndex("dbo.ItemAttributes", "Item_Id");
            AddForeignKey("dbo.ItemAttributes", "Item_Id", "dbo.Items", "Id");
        }
    }
}
