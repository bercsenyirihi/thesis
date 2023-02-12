namespace Example
{
    public class transformer
    {
        public static void Processor()
        {
            Console.WriteLine("APP: transformer started");
            while (true)
            {
                // get all available symbols
                List<string> connections = sqlite_commands.ReadTableNames(Globals.OrderbookConnection!);
                // iterate through every simbol -> split them into token pairs
                foreach (var symbol in connections)
                {
                    var tokenlist = symbol.Split('_');
                    //symbols are in pairs, enough to add only the first token as the second will come up as well
                    sqlite_commands.CreateTableTransformed(Globals.TransformedConnection!, tokenlist[0]);
                    //read min price for in specific table
                    List<string> tmp = sqlite_commands.ReadMinRow(Globals.OrderbookConnection!, symbol);
                    // If the table has any values
                    if (tmp != null && string.Join(",", tmp).Length != 0)
                    {
                        // insert into db
                        sqlite_commands.UpdateDataTransformed(Globals.TransformedConnection!, tokenlist[0], tokenlist[1], tmp[0], tmp[1]);
                    }
                }
            }
        }
    }
}