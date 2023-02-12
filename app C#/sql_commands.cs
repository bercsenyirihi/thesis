using Microsoft.Data.Sqlite;

namespace Example
{
    public class sqlite_commands
    {
        public static void CreateTableOrderBook(SqliteConnection conn, string symbol)
        {
            try
            {
                var createCommand = conn.CreateCommand();
                createCommand.CommandText = "CREATE TABLE IF NOT EXISTS " + symbol + " (Price REAL PRIMARY KEY, Quantity REAL)";
                createCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in CreateTableOrderBook: " + e.Message);
            }
        }

        public static void CreateTableTransformed(SqliteConnection conn, string basetoken)
        {
            try
            {
                var createCommand = conn.CreateCommand();
                createCommand.CommandText = "CREATE TABLE IF NOT EXISTS " + basetoken + " (QuoteToken TEXT PRIMARY KEY, Price REAL, Quantity REAL)";
                createCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in CreateTableTransformed: " + e.Message);
            }
        }

        public static void UpsertOrderBook(SqliteConnection conn, string symbol, string price, string quantity)
        {
            try
            {
                var updateCommand = conn.CreateCommand();
                updateCommand.CommandText = "INSERT OR IGNORE INTO " + symbol + " (Price, Quantity) VALUES (" + price + ", " + quantity + ");";
                updateCommand.ExecuteNonQuery();
                updateCommand.CommandText = "UPDATE " + symbol + " SET Quantity = " + quantity + " WHERE Price=" + price + ";";
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in UpdateDataOrderBook: " + e.Message);
            }
        }

        public static void UpdateDataTransformed(SqliteConnection conn, string basetoken, string quotetoken, string price, string quantity)
        {
            try
            {
                var updateCommand = conn.CreateCommand();
                updateCommand.CommandText = "INSERT OR IGNORE INTO " + basetoken + " VALUES (\"" + quotetoken + "\", " + price + ", " + quantity + ");";
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in UpdateDataTransformed_INSERT: " + e.Message);
            }
            try
            {
                var updateCommand = conn.CreateCommand();
                updateCommand.CommandText = "UPDATE " + basetoken + " SET Price = " + price + " WHERE QuoteToken=\"" + quotetoken + "\";";
                updateCommand.ExecuteNonQuery();
                updateCommand.CommandText = "UPDATE " + basetoken + " SET Quantity = " + quantity + " WHERE QuoteToken=\"" + quotetoken + "\";";
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in UpdateDataTransformed_UPDATE: " + e.Message);
            }
        }

        public static void DeleteData(SqliteConnection conn, string symbol, string price)
        {
            try
            {
                var deleteCommand = conn.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM " + symbol + " WHERE price = " + price + ";";
                deleteCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in DeleteData: " + e.Message);
            }
        }

        public static List<String> ReadTableNames(SqliteConnection conn)
        {
            try
            {
                var queryCommand = conn.CreateCommand();
                queryCommand.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
                List<String> tables = new List<string>();
                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                return tables;
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in ReadTableNames: " + e.Message);
                return null;
            }
        }

        public static List<String> ReadTransfomedRowNames(SqliteConnection conn, string table)
        {
            try
            {
                var queryCommand = conn.CreateCommand();
                queryCommand.CommandText = "SELECT QuoteToken FROM " + table + ";";
                List<String> rows = new List<string>();
                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(reader.GetString(0));
                    }
                }
                return rows;
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in ReadTransfomedRowNames: " + e.Message);
                return null;
            }
        }

        public static List<String> ReadTransformedRow(SqliteConnection conn, string baseToken, string quoteToken)
        {
            try
            {
                var queryCommand = conn.CreateCommand();
                queryCommand.CommandText = "SELECT Price FROM " + baseToken + " WHERE QuoteToken = '" + quoteToken + "';";
                List<String> tables = new List<string>();
                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                return tables;
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in ReadTransformedRow: " + e.Message);
                return null;
            }
        }

        public static List<String> ReadMinRow(SqliteConnection conn, string symbol)
        {
            try
            {
                var queryCommand = conn.CreateCommand();
                queryCommand.CommandText = "SELECT Price, Quantity FROM " + symbol + " ORDER BY Price ASC LIMIT 1";
                List<String> tables = new List<string>();
                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                        tables.Add(reader.GetString(1));
                    }
                }
                return tables;
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception in ReadMinRow: " + e.Message);
                return null;
            }
        }
    }
}