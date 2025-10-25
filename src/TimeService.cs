using Microsoft.Extensions.Hosting;


namespace SurguMailBot
{

    public class TimedService : BackgroundService
    {
        BotService _botService;
        DataBaseService _dataBaseService;
        public TimedService(BotService botService, DataBaseService dataBaseService)
        {
            _botService = botService;
            _dataBaseService = dataBaseService;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now} Start core service\n");
            await base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"{DateTime.Now} Start messaging");
                var list = _dataBaseService.GetActive();
                Console.WriteLine($"Active users: {list.Count()}");
                list = MailService.run(_botService, _dataBaseService, list).Result;
                Console.WriteLine($"Users to change: {list.Count()}");
                _dataBaseService.MassUpdate(list);
                // foreach (var user in list)
                // {
                //     if (user._status == false) _dataBaseService.SetStatus(user._chatId, false);
                //     else _dataBaseService.SetLastTime(user._chatId, user._last_check);
                // }
                Console.WriteLine($"{DateTime.Now} End messaging\n");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stop Core");
            await base.StopAsync(cancellationToken);

        }
    }
}