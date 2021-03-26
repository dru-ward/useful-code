
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namespace
{
    public static class EnableSystemVersionedTemporalTables
    {
        private static string StartTimeColumnName => "Audit_StartTime";
        private static string EndTimeColumnName => "Audit_EndTime";

        public static void AddSystemVersionedTemporalTable(this MigrationBuilder migrationBuilder, string tableName, string temporalScheme = null, string temporalTableSuffix = @"History")
        {
            var temporalTableName = $"{tableName}_{temporalTableSuffix}";
            var schemaName = temporalScheme ?? "dbo";

            migrationBuilder.Sql($@"
                    IF NOT EXISTS (SELECT * FROM sys.[tables] t 
                                        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id 
                                            WHERE t.name = '{tableName}' AND temporal_type = 2 AND s.name = '{schemaName}')
                    BEGIN
                        ALTER TABLE [{schemaName}].[{tableName}]   
                        ADD   {StartTimeColumnName} DATETIME2 (0) GENERATED ALWAYS AS ROW START HIDDEN constraint DF_{tableName}_{StartTimeColumnName} DEFAULT DATEADD(SECOND, -1, SYSUTCDATETIME())  
                            , {EndTimeColumnName}   DATETIME2 (0) GENERATED ALWAYS AS ROW END   HIDDEN constraint DF_{tableName}_{EndTimeColumnName}   DEFAULT '9999.12.31 23:59:59.99'  
                            , PERIOD FOR SYSTEM_TIME ({StartTimeColumnName}, {EndTimeColumnName});   
 
                        ALTER TABLE [{schemaName}].[{tableName}]
                        SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{schemaName}].[{temporalTableName}])); 
                    END
                ");
        }

        public static void RemoveSystemVersionedTemporalTable(this MigrationBuilder migrationBuilder, string tableName, string temporalScheme = null, string temporalTableSuffix = @"History")
        {
            var temporalTableName = $"{tableName}_{temporalTableSuffix}";
            var schemaName = temporalScheme ?? "dbo";

            var alterStatement = $@"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF);";
            migrationBuilder.Sql(alterStatement);
            alterStatement = $@"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME";
            migrationBuilder.Sql(alterStatement);
            alterStatement = $@"ALTER TABLE [{tableName}] DROP CONSTRAINT DF_{tableName}_{StartTimeColumnName}, DF_{tableName}_{EndTimeColumnName}";
            migrationBuilder.Sql(alterStatement);
            alterStatement = $@"ALTER TABLE [{tableName}] DROP COLUMN {StartTimeColumnName}, COLUMN {EndTimeColumnName}";
            migrationBuilder.Sql(alterStatement);
            alterStatement = $@"DROP TABLE [{schemaName}].[{temporalTableName}]";
            migrationBuilder.Sql(alterStatement);
        }
    }
}
