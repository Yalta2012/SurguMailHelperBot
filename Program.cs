using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;




using SurguMailBot;


try
{
    var db = new DataBaseService("database.db");
    var bot = new BotService(File.ReadLines(".token").First(), db);

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddHostedService<TimedService>(provider => new TimedService(bot, db));

    builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    var host = builder.Build();

    await host.StartAsync();
    Console.ReadLine();
    await host.StopAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
}












