using Npgsql;

namespace DatabaseScriptCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost; Port=5432; Username=postgres; Password=postgres; Database=internal_user";

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


            string projectDirectory = "E:\\myProjects\\kafkaDemo\\DatabaseScriptCreator";

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

                    // Retrieve columns for the table from information_schema
                    List<string> columns = new List<string>();
                    string query = $@"SELECT column_name || ' ' || data_type ||
                     CASE
                        WHEN character_maximum_length IS NOT NULL THEN '(' || character_maximum_length || ')'
                        ELSE ''
                     END AS column_definition
                     FROM information_schema.columns
                     WHERE table_schema = '{schemaName}' AND table_name = '{actualTableName}'
                     ORDER BY ordinal_position";

                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();

                        using (var cmd = new NpgsqlCommand(query, connection))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader.GetString(0));
                            }
                        }
                    }


                    // Construct the CREATE TABLE statement
                    string createTableStatement = $"CREATE TABLE IF NOT EXISTS {table} (\n\t{string.Join(",\n\t", columns)}\n);";

                    // Create schema directory if it doesn't exist
                    string schemaDirectory = Path.Combine(tablesDirectory, schemaName);
                    if (!Directory.Exists(schemaDirectory))
                    {
                        Directory.CreateDirectory(schemaDirectory);
                    }

                    // Create table script file within schema directory
                    string filePath = Path.Combine(schemaDirectory, $"{actualTableName}.sql");
                    File.WriteAllText(filePath, createTableStatement);
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

                            File.WriteAllText(filePath, functionDefinition.Text.Replace("$function$", "$$"));
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
        }
    }


    public class PreFunc
    {
        public int ParamsCount { get; set; }

        public string? Text { get; set; }
    }
}