using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public static class CommentOperationExtensions
    {
        public static OperationBuilder<CommentOperation> Comment(this MigrationBuilder builder, string table, string comment, string schema = "dbo", string column = null)
        {
            var operation = new CommentOperation()
            {
                Table = table,
                Comment = comment,
                Schema = schema,
                Column = column,
            };
            builder.Operations.Add(operation);
            return new OperationBuilder<CommentOperation>(operation);
        }
    }
}