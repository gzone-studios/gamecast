using GameCast.Server.Handlers;

namespace GameCast.Server;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UserManager>();
        services.AddSingleton<RoomManager>();
        services.AddSingleton<GameManager>();

        services.AddTransient<ConnectionManager>();
        services.AddSingleton<GameHandler>();

        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseAuthorization();

        WebSocketOptions webSocketOptions = new()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120)
        };

        //webSocketOptions.AllowedOrigins.Add("https://client.com");

        app.UseWebSockets(webSocketOptions);

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}