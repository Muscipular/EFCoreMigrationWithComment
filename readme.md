# EFCoreMigrationWithComment.SqlServer

add ef core 2.x migrations comment to sqlserver

## usage
just modify DbContext like below, xml comment or DescriptionAttrite will add MS_Description property in database.
```csharp
class MyDBContext : DbContext 
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /* create model code */
        modelBuilder.ApplyComment();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        /* another config code */
        builder.ReplaceService<IMigrationsModelDiffer, MigrationsModelDifferWithComment>();
        builder.ReplaceService<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGeneratorWithComment>();
        builder.ReplaceService<IMigrationsSqlGenerator, SqlServerMigrationsSqlGeneratorWithComment>();        
    }
}
```