using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerMigrationsSqlGeneratorWithComment : SqlServerMigrationsSqlGenerator
    {
        public SqlServerMigrationsSqlGeneratorWithComment(MigrationsSqlGeneratorDependencies dependencies, IMigrationsAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            switch (operation)
            {
                case CommentOperation commentOperation:
                    var (schema, table, column, comment) = commentOperation;
                    var col = string.IsNullOrWhiteSpace(column) ? "null, default" : $"'COLUMN', '{column}'";
                    var sql = $@"
if exists(select * from sys.fn_listextendedproperty(N'MS_Description', N'schema','{schema}',N'table',N'{table}', {col}))
exec sys.sp_dropextendedproperty 'MS_Description', 'SCHEMA', '{schema}', 'TABLE', '{table}'";
                    if (!string.IsNullOrWhiteSpace(column))
                    {
                        sql += $", 'COLUMN', '{column}'";
                    }
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        var cm = comment.Replace("'", "''");
                        sql += $@"
exec sp_addextendedproperty 'MS_Description', '{cm}', 'SCHEMA', '{schema}', 'TABLE', '{table}'";
                        if (!string.IsNullOrWhiteSpace(column))
                        {
                            sql += $", 'COLUMN', '{column}'";
                        }
                    }
                    builder.AppendLine(sql).EndCommand();
                    return;
            }
            base.Generate(operation, model, builder);
        }
    }
}