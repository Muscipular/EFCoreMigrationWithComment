using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class CSharpMigrationOperationGeneratorWithComment : CSharpMigrationOperationGenerator
    {
        public CSharpMigrationOperationGeneratorWithComment(CSharpMigrationOperationGeneratorDependencies dependencies) : base(dependencies)
        {
        }

        protected override void Generate(MigrationOperation operation, IndentedStringBuilder builder)
        {
            switch (operation)
            {
                case CommentOperation commentOperation:
                    Generate(commentOperation, builder);
                    return;
            }
            base.Generate(operation, builder);
        }

        protected virtual void Generate(CommentOperation operation, IndentedStringBuilder builder)
        {
            var cSharpHelper = Dependencies.CSharpHelper;
            string code(string s) => s == null ? "null" : cSharpHelper.Literal(s);
            builder.Append($".Comment(schema: {code(operation.Schema)}" +
                $", table: {code(operation.Table)}" +
                $", column: {code(operation.Column)}" +
                $", comment: {code(operation.Comment)}" +
                $")");
        }
    }
}