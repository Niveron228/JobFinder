using Flurl.Http;
using JustJoinJobFinder.DB;
using JustJoinJobFinder.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace JustJoinJobFinder
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var db = new AppDbContex())
            {
                db.Database.Migrate();
            }

            var botToken = _configuration["Telegram:BotToken"];
            var chatId = _configuration["Telegram:ChatId"];
            var botClient = new TelegramBotClient(botToken!);

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: null,
                cancellationToken: stoppingToken
            );

            var menu = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("🔍 Find 20 offers", "find_20"));
            await botClient.SendMessage(chatId!, "🤖 Bot is alive.It works automatically but you can find offers by pressingthe button", replyMarkup: menu, cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchAndSendOffersAsync(botClient, chatId!, 10, stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data == "find_20")
            {
                var chatId = update.CallbackQuery.Message!.Chat.Id.ToString();

                await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, "Looking for offers..", cancellationToken: ct);

                await FetchAndSendOffersAsync(botClient, chatId, 20, ct);
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            _logger.LogError($"Error Telegram API: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task FetchAndSendOffersAsync(ITelegramBotClient botClient, string chatId, int count, CancellationToken ct)
        {
            string apiUrl = $"https://justjoin.it/api/candidate-api/offers?city=Warszawa&cityRadius=30&categories=net&sortBy=publishedAt&orderBy=descending&from=0&itemsCount={count}";

            try
            {
                var response = await apiUrl
                    .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36")
                    .WithHeader("Accept", "application/json")
                    .GetJsonAsync<JjitResponse>(cancellationToken: ct);

                if (response?.Data != null)
                {
                    using var db = new AppDbContex();
                    int sentCount = 0;

                    var offersToProcess = response.Data.AsEnumerable().Reverse().ToList();

                    foreach (var offer in offersToProcess)
                    {
                        if (!db.SentOffers.Any(o => o.Slug == offer.Slug))
                        {
                            string message = $"🔥 Offers found: {offer.Title}\n" +
                                             $"🏢 Company: {offer.CompanyName}\n" +
                                             $"🔗 Link: https://justjoin.it/job-offer/{offer.Slug}";

                            await botClient.SendMessage(chatId, message, cancellationToken: ct);

                            db.SentOffers.Add(new SentOffer { Slug = offer.Slug! });
                            await db.SaveChangesAsync(ct);
                            sentCount++;

                            await Task.Delay(500, ct);
                        }
                    }

                    if (sentCount == 0 && count > 10)
                    {
                        await botClient.SendMessage(chatId, "🤷‍ There is no new offers yet :(", cancellationToken: ct);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Backend Error: {ex.Message}");
            }
        }
    }
}