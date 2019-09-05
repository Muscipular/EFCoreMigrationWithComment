using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class CommentOperation : MigrationOperation
    {
        public string Schema { get; set; }

        public string Table { get; set; }

        public string Column { get; set; }

        public string Comment { get; set; }

        public void Deconstruct(out string schema, out string table, out string column, out string comment)
        {
            schema = Schema;
            table = Table;
            column = Column;
            comment = Comment;
        }
    }
}