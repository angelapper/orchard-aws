using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Migration;

namespace Apper.Aws
{
    public class Migrations : DataMigrationImpl 
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("AwsParaRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("AccessKey")
                .Column<string>("SecretKey")
                .Column<string>("FileBucket")
                .Column<string>("LoggingBucket")
            );

            return 1;
        }

    }
}