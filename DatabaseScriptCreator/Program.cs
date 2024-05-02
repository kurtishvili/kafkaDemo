using Npgsql;
using System.Data;

namespace DatabaseScriptCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=51.144.42.171; Port=5432; Username=postgres; Password=postgres; Database=patient_testing";

            var schemas = new List<string>();
            var tables = new List<string>();
            var functions = new List<string>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema')", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string schemaName = reader.GetString(0);
                        Console.WriteLine($"Schema: {schemaName}");
                        schemas.Add(schemaName);
                    }
                }

                connection.Close();
            };

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT concat(specific_schema, '.', routine_name) FROM information_schema.routines WHERE routine_type='FUNCTION' AND specific_schema NOT IN ('pg_catalog', 'information_schema')", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string functionName = reader.GetString(0);
                        Console.WriteLine($"Function: {functionName}");
                        functions.Add(functionName);
                    }
                }

                connection.Close();
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT concat(table_schema, '.', table_name) FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')", connection))

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        Console.WriteLine($"Table: {tableName}");
                        tables.Add(tableName);
                    }
                }

                connection.Close();
            }


            string projectDirectory = "E:\\myProjects\\RSBackEnd\\RS.PatientService";

            string tablesDirectory = Path.Combine(projectDirectory, "Scripts", "Tables");
            string functionsDirectory = Path.Combine(projectDirectory, "Scripts", "Functions");

            // Create Tables directory
            Directory.CreateDirectory(tablesDirectory);

            // Create Functions directory
            Directory.CreateDirectory(functionsDirectory);

            // Create schema directories within Tables and Functions directories
            foreach (string schema in schemas)
            {
                Directory.CreateDirectory(Path.Combine(tablesDirectory, schema));
                Directory.CreateDirectory(Path.Combine(functionsDirectory, schema));
            }

            // Create SQL scripts for tables
            foreach (string schema in schemas)
            {
                var schemaTables = tables.Where(t => t.StartsWith(schema + ".")).ToList();
                foreach (string table in schemaTables)
                {
                    string[] nameParts = table.Split('.');
                    string schemaName = nameParts.Length > 1 ? nameParts[0] : "public"; // Default schema to 'public' if not specified
                    string actualTableName = nameParts[nameParts.Length - 1]; // Extract table name

                    string indexDefinition = GetIndexDev(actualTableName, connectionString);


                    // Retrieve columns and constraints for the table from information_schema
                    List<string> columnDefinitions = new List<string>();
                    List<string> constraints = new List<string>();
                    List<string> pkConstraints = new List<string>();

                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = $@"
                                       SELECT 
                                           columns.column_name,
                                           data_type,
                                           is_nullable,
                                           identity_generation,
                                           fk_constraint_name,
                                       	   foreign_table_schema,
                                       	   foreign_table_name,
                                       	   foreign_column_name
                                          FROM 
                                           information_schema.columns
                                           LEFT JOIN (
                                               SELECT
                                                   tc.constraint_name AS fk_constraint_name,
                                       		    tc.table_name,
                                       		    ccu.table_schema AS foreign_table_schema,
                                       		    ccu.table_name AS foreign_table_name,
                                                   ccu.column_name AS foreign_column_name 
                                       		    
                                               FROM 
                                                   information_schema.table_constraints tc
                                       		JOIN information_schema.constraint_column_usage AS ccu
                                           ON ccu.constraint_name = tc.constraint_name
                                               WHERE 
                                                   constraint_type = 'FOREIGN KEY'
                                           ) AS constraints_fk
                                               ON information_schema.columns.table_name = constraints_fk.table_name
                                               AND information_schema.columns.column_name = (
                                                   SELECT
                                                       kcu.column_name
                                                   FROM 
                                                       information_schema.key_column_usage AS kcu
                                                   WHERE 
                                                       kcu.table_name = information_schema.columns.table_name
                                                       AND kcu.constraint_name = constraints_fk.fk_constraint_name
                                               )
                                       WHERE 
                                           information_schema.columns.table_schema = '{schema}' 
                                           AND information_schema.columns.table_name = '{actualTableName}'
                                       ORDER BY information_schema.columns.ordinal_position";

                        using (var cmd = new NpgsqlCommand(query, connection))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName = reader.GetString(0);
                                string dataType = reader.GetString(1);
                                string isNullable = reader.GetString(2) == "YES" ? "" : "NOT NULL";
                                var identityGeneration = reader.IsDBNull(3) ? "" : reader.GetString(3);

                                if (identityGeneration == "ALWAYS")
                                {
                                    identityGeneration = "GENERATED ALWAYS AS IDENTITY";
                                }
                                else if (identityGeneration == "BY DEFAULT")
                                {
                                    identityGeneration = "GENERATED BY DEFAULT AS IDENTITY";
                                }

                                string columnDefinition = $"{columnName} {dataType}{(isNullable == "" ? "" : " " + isNullable)} {identityGeneration}".Trim();

                                // Append constraints
                                columnDefinition += string.Join("\n\t", constraints);

                                columnDefinitions.Add(columnDefinition);

                                if (!reader.IsDBNull(4))
                                {
                                    // If it is part of the primary key, add it to a separate list
                                    pkConstraints.Add($"CONSTRAINT {reader.GetString(4)} FOREIGN KEY ({columnName})\n" +
                                               $"         REFERENCES {reader.GetString(5)}.{reader.GetString(6)} ({reader.GetString(7)}) MATCH SIMPLE");
                                }
                            }
                        }

                        var primary = GetPrimary(actualTableName, connectionString);

                        if (primary is not null)
                        {
                            pkConstraints.Add($"CONSTRAINT {primary.ConstraintName} PRIMARY KEY ({primary.ConcatenatedColumnNames})");
                        }

                        var unique = GetUnique(actualTableName, connectionString);

                        if (unique is not null)
                        {
                            pkConstraints.Add($"CONSTRAINT {unique.ConstraintName} UNIQUE ({unique.ConcatenatedColumnNames})");
                        }

                        // Add the primary key constraint to the end of the column definitions
                        columnDefinitions.AddRange(pkConstraints);

                        // Construct the CREATE TABLE statement
                        string createTableStatement = $"CREATE TABLE IF NOT EXISTS {schemaName}.{actualTableName} (\n\t{string.Join(",\n\t", columnDefinitions).Replace(" ,", ",")}\n);";

                        // Create schema directory if it doesn't exist
                        string schemaDirectory = Path.Combine(tablesDirectory, schemaName, actualTableName);
                        if (!Directory.Exists(schemaDirectory))
                        {
                            Directory.CreateDirectory(schemaDirectory);
                        }

                        // Create table script file within schema directory
                        string filePath = Path.Combine(schemaDirectory, $"001_{actualTableName}.sql");
                        File.WriteAllText(filePath, createTableStatement);

                        if (!string.IsNullOrEmpty(indexDefinition))
                        {
                            string indexDefinitionWithEmptyLine = $"\n\n{indexDefinition + ";"}";
                            File.AppendAllText(filePath, indexDefinitionWithEmptyLine);
                        }

                    }
                }
            }


            // Create SQL scripts for functions
            foreach (string schema in schemas)
            {
                var schemaFunctions = functions.Where(f => f.StartsWith(schema + ".")).ToList();
                foreach (string function in schemaFunctions)
                {
                    string[] nameParts = function.Split('.');
                    string schemaName = nameParts.Length > 1 ? nameParts[0] : "public"; // Default schema to 'public' if not specified
                    string actualFunctionName = nameParts[nameParts.Length - 1]; // Extract function name


                    // Retrieve function definition from database
                    var functionDefinitions = GetFunctionDefinition(actualFunctionName, connectionString);

                    foreach (var functionDefinition in functionDefinitions)
                    {
                        if (functionDefinitions != null)
                        {
                            var filePath = Path.Combine(functionsDirectory, schemaName, functionDefinitions.Count() > 1 ? $"{actualFunctionName}_p{functionDefinition.ParamsCount}_v1.sql" : $"{actualFunctionName}_v1.sql");

                            var functionText = functionDefinition.Text.TrimEnd('\n').Replace("$function$", "$$");
                            var finalText = functionText.EndsWith("$$") ? functionText.Insert(functionText.LastIndexOf("$$") + 2, ";") : functionText + ";";
                            File.WriteAllText(filePath, finalText);
                        }
                    }

                }
            }

            Console.WriteLine("Database scripts created successfully.");


            static IEnumerable<PreFunc> GetFunctionDefinition(string functionName, string connectionString)
            {
                var list = new List<PreFunc>();

                var oids = GetOids(functionName, connectionString);

                foreach (var oid in oids)
                {
                    var preFunc = new PreFunc
                    {
                        ParamsCount = GetParamsCount(oid, connectionString),
                        Text = GetText(oid, connectionString)
                    };

                    list.Add(preFunc);
                }

                return list;
            }

            static IEnumerable<UInt32> GetOids(string functionName, string connectionString)
            {
                var list = new List<UInt32>();

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var oids = $"SELECT oid FROM pg_proc WHERE proname = '{functionName}'";

                    using (var cmd = new NpgsqlCommand(oids, connection))
                    {
                        var data = cmd.ExecuteReader();

                        while (data.Read())
                        {
                            list.Add((UInt32)data[0]);
                        }
                    }
                }

                return list;
            }

            static int GetParamsCount(long oid, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = $"SELECT pg_get_function_identity_arguments({oid})";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        return ((string)cmd.ExecuteScalar()).Split(",").Length;
                    }
                }
            }

            static string? GetText(long oid, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = $"SELECT pg_get_functiondef({oid})";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        return (string)cmd.ExecuteScalar()!;
                    }
                }
            }

            static string GetIndexDev(string tableName, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = $"SELECT indexdef FROM pg_indexes WHERE tablename = '{tableName}' AND indexname ILIKE '%index'";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        var result = cmd.ExecuteScalar();
                        return result != null ? result.ToString().Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS") : null;
                    }
                }
            }

            static Unique GetUnique(string tableName, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = $@"SELECT tc.constraint_name, STRING_AGG(ccu.column_name, ',') AS concatenated_column_names 
                      FROM information_schema.table_constraints tc 
                      JOIN information_schema.constraint_column_usage AS ccu 
                      ON ccu.constraint_name = tc.constraint_name 
                      WHERE constraint_type = 'UNIQUE' AND tc.table_name = '{tableName}' 
                      GROUP BY tc.constraint_name";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Unique
                                {
                                    ConstraintName = reader.GetString(0),
                                    ConcatenatedColumnNames = reader.GetString(1)
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            static Primary GetPrimary(string tableName, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = $@"SELECT tc.constraint_name, STRING_AGG(ccu.column_name, ',') AS concatenated_column_names 
                      FROM information_schema.table_constraints tc 
                      JOIN information_schema.constraint_column_usage AS ccu 
                      ON ccu.constraint_name = tc.constraint_name 
                      WHERE constraint_type = 'PRIMARY KEY' AND tc.table_name = '{tableName}' 
                      GROUP BY tc.constraint_name";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Primary
                                {
                                    ConstraintName = reader.GetString(0),
                                    ConcatenatedColumnNames = reader.GetString(1)
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

        }
    }


    public class PreFunc
    {
        public int ParamsCount { get; set; }

        public string? Text { get; set; }
    }

    public class Unique
    {
        public string ConstraintName { get; set; }

        public string ConcatenatedColumnNames { get; set; }
    }

    public class Primary
    {
        public string ConstraintName { get; set; }

        public string ConcatenatedColumnNames { get; set; }
    }
}