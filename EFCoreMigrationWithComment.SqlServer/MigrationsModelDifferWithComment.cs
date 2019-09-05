using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationsModelDifferWithComment : MigrationsModelDiffer
    {
        public MigrationsModelDifferWithComment(IRelationalTypeMappingSource typeMappingSource, IMigrationsAnnotationProvider migrationsAnnotations, IChangeDetector changeDetector, StateManagerDependencies stateManagerDependencies, CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource, migrationsAnnotations, changeDetector, stateManagerDependencies, commandBatchPreparerDependencies)
        {
        }

        protected override IEnumerable<MigrationOperation> Diff(TableMapping source, TableMapping target, DiffContext diffContext)
        {
            var clrComment = source?.EntityTypes.Select(e => e.FindAnnotation("ClrComment")).FirstOrDefault(e => e != null)?.Value.ToString();
            var clrComment2 = target?.EntityTypes.Select(e => e.FindAnnotation("ClrComment")).FirstOrDefault(e => e != null)?.Value.ToString();
            var operations = base.Diff(source, target, diffContext);
            if (clrComment == clrComment2)
            {
                return operations;
            }
            var tableName = target?.EntityTypes.Select(e => e.FindAnnotation("Relational:TableName")).FirstOrDefault(e => e != null)?.Value.ToString();
            if (tableName == null)
            {
                tableName = source?.EntityTypes.Select(e => e.FindAnnotation("Relational:TableName")).FirstOrDefault(e => e != null)?.Value.ToString();
            }
            if (tableName == null)
            {
                foreach (var t in target.EntityTypes.Concat(new[] { target.GetRootType() }))
                {
                    Console.WriteLine($"{t.Name}");
                    foreach (var annotation in t.GetAnnotations())
                    {
                        Console.WriteLine($"{annotation.Name} => {annotation.Value}");
                    }
                }
                return operations;
            }
            var schema = target?.EntityTypes.Select(e => e.FindAnnotation("Relational:Schema")).FirstOrDefault(e => e != null)?.Value.ToString() ?? "dbo";
            var diff = operations.ToList();
            diff.Add(new CommentOperation()
            {
                Table = tableName,
                Schema = schema,
                Comment = clrComment2,
            });
            return diff;
        }

        protected override IEnumerable<MigrationOperation> Diff(IProperty source, IProperty target, DiffContext diffContext)
        {
            var clrComment = source?.FindAnnotation("ClrComment")?.Value.ToString();
            var clrComment2 = target?.FindAnnotation("ClrComment")?.Value.ToString();
            var operations = base.Diff(source, target, diffContext);
            if (clrComment == clrComment2)
            {
                return operations;
            }
            var entityType = target?.DeclaringEntityType;
            var tableName = entityType?.FindAnnotation("Relational:TableName")?.Value.ToString();
            if (tableName == null)
            {
                entityType = source?.DeclaringEntityType;
                tableName = entityType?.FindAnnotation("Relational:TableName")?.Value.ToString();
            }
            if (tableName == null)
            {
                return operations;
            }
            var column = target.FindAnnotation("Relational:ColumnName")?.Value.ToString() ?? target.Name;
            var schema = entityType.FindAnnotation("Relational:Schema")?.Value.ToString() ?? "dbo";
            var diff = operations.ToList();
            diff.Add(new CommentOperation()
            {
                Table = tableName,
                Schema = schema,
                Column = column,
                Comment = clrComment2,
            });
            return diff;
        }
    }
}