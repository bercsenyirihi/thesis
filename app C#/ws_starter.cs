using System.Diagnostics;

namespace Example
{
    public static class ws_starter
    {
        public static void Start()
        {
            Console.WriteLine("APP: ws_starter started");
            Globals.wsprocess = new Process();
            Globals.wsprocess.StartInfo.FileName = "./ws C#.exe";
            Globals.wsprocess.StartInfo.Arguments = "init /value=1";
            Globals.wsprocess.StartInfo.UseShellExecute = false;
            Globals.wsprocess.StartInfo.CreateNoWindow = true;
            Globals.wsprocess.Start();
        }
    }
}
