namespace GameCast.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        IHost host = CreateHostBuilder(args).Build();

        host.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}