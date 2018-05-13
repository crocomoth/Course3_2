namespace Course3_2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContextMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailModels",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Email = c.String(),
                        Password = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailModels", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.EmailModels", new[] { "User_Id" });
            DropTable("dbo.EmailModels");
        }
    }
}
