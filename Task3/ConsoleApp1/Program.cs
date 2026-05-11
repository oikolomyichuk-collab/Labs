using Domain;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient bot;
        static DB db;
        static Dictionary<long, UserSession> sessions = new Dictionary<long, UserSession>();

        static UserSession GetOrCreateSession(long chatId)
        {
            if (!sessions.ContainsKey(chatId))
                sessions[chatId] = new UserSession();
            return sessions[chatId];
        }

        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            bot = new TelegramBotClient("8798307009:AAFkWkdNyT-95SJjcQQnlerfiMZlJx8ZvdE", cancellationToken: cts.Token);
            db = new DB();

            await bot.SetMyCommands(new[]
            {
                new BotCommand { Command = "start", Description = "Запустити бота" },
                new BotCommand { Command = "help", Description = "Допомога" },
                new BotCommand { Command = "exit", Description = "Завершити роботу" }
            });

            var me = await bot.GetMe();

            bot.OnMessage += OnMessage;
            bot.OnUpdate += OnUpdate;
            bot.OnError += OnError;

            Console.WriteLine($"@{me.Username} is running...");
            Console.ReadLine();
            cts.Cancel();
            
        }

        static async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        static async Task OnMessage(Message msg, UpdateType type)
        {
            var chatId = msg.Chat.Id;
            var session = GetOrCreateSession(chatId);

            if (msg.Text == "/start" && session.Un == Userenum.None)
            {
                await bot.SendMessage(msg.Chat.Id,
                    "Вітаю! Ви запустили бота Manufacturing of parts! " +
                    "Команди:\"/help\", \"/exit\"");

                await SendTablesMenu(msg.Chat.Id);
            }

            else if (msg.Text == "Деталь" && session.Un == Userenum.None)
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Переглянути", "detail_show"),
                        InlineKeyboardButton.WithCallbackData("Додати", "detail_add"),
                        InlineKeyboardButton.WithCallbackData("Оновити", "detail_update"),
                        InlineKeyboardButton.WithCallbackData("Видалити", "detail_delete")
                    }
                });
                await bot.SendMessage(chatId, "Оберіть дію", replyMarkup: keyboard);
            }
            else if (msg.Text == "Операція" && session.Un == Userenum.None)
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Переглянути", "operation_show"),
                        InlineKeyboardButton.WithCallbackData("Додати", "operation_add"),
                        InlineKeyboardButton.WithCallbackData("Оновити", "operation_update"),
                        InlineKeyboardButton.WithCallbackData("Видалити", "operation_delete")
                    }
                });

                    await bot.SendMessage(chatId, "Оберіть дію", replyMarkup: keyboard);
                
            }

            else if (msg.Text == "Виробництво" && session.Un == Userenum.None)
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Переглянути", "production_show"),
                        InlineKeyboardButton.WithCallbackData("Додати", "production_add"),
                        InlineKeyboardButton.WithCallbackData("Видалити", "production_delete")
                    }
                });
                   await bot.SendMessage(chatId, "Оберіть дію", replyMarkup: keyboard);
             }
     
            if (msg.Text == "/help")
            {
                await bot.SendMessage(msg.Chat.Id, "Таблиця \"Операція\" - в цій таблиці ви вводите рядки: Код операції (ціле число 1-50 символів), Номер цеху(від 1 до 20),Тривалість операції в годинах (1-8 годин), Вартість виконання операції, грн (число із двома знаками після коми)." +
                    "Таблиця \"Деталь\" - в цій таблиці ви вводите рядки: Код деталі,Децимальний номер деталі (до 20 символів, формат: хххх.хххх.ххх-хх), Назва деталі, Марка сплаву, Маса, кг( формат: ххх,ххх)." +
                    "Таблиця \"Виробництво\" - в цій таблиці ви вводите рядки: існуючий код деталі, Номер операції в технологічному процесі (від 1 до 100) та існуючий код операції.");
            }
            else if (msg.Text == "/exit")
            {
                session.Endb.Clear();
                session.Un = Userenum.None;
                await bot.SendMessage(chatId, "До побачення! Для повернення введіть /start");
            }


            switch (session.Un)
            {
                //додавання айді деталі
                case Userenum.detail_code:
                    

                    if (int.TryParse(msg.Text, out int DetailCode) && DetailCode > 0)
                    {
                        session.Endb["DetailCode"] = msg.Text;
                        session.Un = Userenum.detail_decimal_number;
                        await bot.SendMessage(chatId, "Введіть децімальний номер, у форматі хххх.хххх.ххх-хх:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //додавання децимального номеру деталі
                case Userenum.detail_decimal_number:

                    if (Regex.IsMatch(msg.Text, @"^\d{4}\.\d{4}\.\d{3}-\d{2}$"))
                    {
                        session.Endb["DecimalNumber"] = msg.Text;
                        session.Un = Userenum.detail_name;
                        await bot.SendMessage(chatId, "Введіть назву деталі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                //додавання назви деталі
                case Userenum.detail_name:

                    session.Endb["DetailName"] = msg.Text;
                    session.Un = Userenum.detail_alloy_grade;
                    await bot.SendMessage(chatId, "Введіть матеріал деталі:");
                    break;

                //додавання сплаву деталі
                case Userenum.detail_alloy_grade:

                    session.Endb["AlloyGrade"] = msg.Text;
                    session.Un = Userenum.detail_mass;
                    await bot.SendMessage(chatId, "Введіть масу деталі (формат: ххх,ххх):");
                    break;

                //додавання маси деталі
                case Userenum.detail_mass:

                    if (Regex.IsMatch(msg.Text, @"^\d{1,3},\d{1,3}$"))
                    {
                        decimal Mass = decimal.Parse(msg.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                        session.Endb["Mass"] = msg.Text;
                        var detail = new Detail
                        {
                            DetailCode = int.Parse(session.Endb["DetailCode"]),
                            DecimalNumber = session.Endb["DecimalNumber"],
                            DetailName = session.Endb["DetailName"],
                            AlloyGrade = session.Endb["AlloyGrade"],
                            Mass = Mass
                        };

                        var existing = db.repoDetail.GetById(detail.DetailCode);
                        if (existing != null)
                        {
                            await bot.SendMessage(chatId, "Деталь з таким ID вже існує! Введіть інший ID:");
                            session.Un = Userenum.detail_code;
                            session.Endb.Clear();
                        }
                        else
                        {
                            db.repoDetail.Add(detail);
                            db.repoDetail.Save();
                            session.Endb.Clear();
                            session.Un = Userenum.None;
                            await bot.SendMessage(chatId, "Деталь успішно додана!");
                            await SendTablesMenu(chatId);
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //видалення деталі
                case Userenum.detail_delete:

                    if (int.TryParse(msg.Text, out int deleteId) && deleteId > 0)
                    {
                        var existing = db.repoDetail.GetById(deleteId);
                        if (existing != null)
                        { 
                            db.repoDetail.Delete(existing);
                            db.repoDetail.Save();
                            session.Endb.Clear();
                            session.Un = Userenum.None;
                            await bot.SendMessage(chatId, "Видалення успішне!");
                            await SendTablesMenu(chatId);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "Помилка! Деталі з таким Id не існує!");
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Деталі з таким Id не існує!");
                    }
                    break;

                //оновлення деталі, пошук по айді
                case Userenum.detail_code_update:

                    if (int.TryParse(msg.Text, out int ID_update) && ID_update > 0)
                    {
                        var existing = db.repoDetail.GetById(ID_update);
                        if (existing != null)
                        {
                            session.Endb["DetailCode"] = msg.Text;
                            session.Un = Userenum.detail_decimal_number_update;
                            await bot.SendMessage(chatId, "Деталь знайдена! Введіть новий децимальний номер:");
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "Помилка! Деталі з таким Id не існує!");
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Деталі з таким Id не існує!");
                    }

                    break;

                //оновленння децимального номера деталі
                case Userenum.detail_decimal_number_update:

                    if (msg.Text.Length >= 1 && msg.Text.Length <= 20)
                    {
                        session.Endb["DecimalNumber"] = msg.Text;
                        session.Un = Userenum.detail_name_update;
                        await bot.SendMessage(chatId, "Введіть нову назву деталі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                //оновленння назви деталі
                case Userenum.detail_name_update:

                    session.Endb["DetailName"] = msg.Text;
                    session.Un = Userenum.detail_alloy_grade_update;
                    await bot.SendMessage(chatId, "Введіть нову назву матеріалу деталі:");
                    break;

                //оновленння сплаву деталі
                case Userenum.detail_alloy_grade_update:

                    session.Endb["AlloyGrade"] = msg.Text;
                    session.Un = Userenum.detail_mass_update;
                    await bot.SendMessage(chatId, "Введіть нову масу деталі (формат: ххх,ххх):");
                    break;

                //оновленння маси деталі та збереження оновлень
                case Userenum.detail_mass_update:

                    if (decimal.TryParse(msg.Text, out decimal Massup) && Massup > 0)
                    {
                        session.Endb["Massup"] = msg.Text;
                        var existing = db.repoDetail.GetById(int.Parse(session.Endb["DetailCode"]));
                        existing.DecimalNumber = session.Endb["DecimalNumber"];
                        existing.DetailName = session.Endb["DetailName"];
                        existing.AlloyGrade = session.Endb["AlloyGrade"];
                        existing.Mass = Massup;
                        db.repoDetail.Update(existing);
                        db.repoDetail.Save();
                        session.Endb.Clear();
                        session.Un = Userenum.None;
                        await bot.SendMessage(chatId, "Деталь успішно оновлена!");
                        await SendTablesMenu(chatId);
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //додавання айді операції
                case Userenum.operation_code:


                    if (int.TryParse(msg.Text, out int OperationCode) && OperationCode > 0)
                    {
                        session.Endb["OperationCode"] = msg.Text;
                        session.Un = Userenum.operation_workshop_number;
                        await bot.SendMessage(chatId, "Введіть  номер цеху, від 1 до 20 символів");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //додавання номеру цеху
                case Userenum.operation_workshop_number:

                    if (int.TryParse(msg.Text, out int WorkshopNumber) && WorkshopNumber > 0 && WorkshopNumber <= 20)
                    {
                        session.Endb["WorkshopNumber"] = msg.Text;
                        session.Un = Userenum.operation_duration_hours;
                        await bot.SendMessage(chatId, "Введіть тривалість операції в годинах:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер цеху!");
                    }
                    break;

                //додавання тривалості операції
                case Userenum.operation_duration_hours:
                    
                    if (int.TryParse(msg.Text, out int DurationHours) && DurationHours > 0 && DurationHours <=8)
                    {
                        session.Endb["DurationHours"] = msg.Text;
                        session.Un = Userenum.operation_cost;
                        await bot.SendMessage(chatId, "Введіть вартість виконання операції:");
                    }

                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректну тривалість операції в годинах:");
                    }

                        break;

                //додавання вартості виконання операції
                case Userenum.operation_cost:

                    if (decimal.TryParse(msg.Text, out decimal Cost) && Cost > 0)
                    {
                        Cost = Math.Round(Cost, 2);
                        session.Endb["Cost"] = msg.Text;
                        var operation = new Operation
                        {
                            OperationCode = int.Parse(session.Endb["OperationCode"]),
                            WorkshopNumber = int.Parse(session.Endb["WorkshopNumber"]),
                            DurationHours = int.Parse(session.Endb["DurationHours"]),
                            Cost = Cost
                        };

                        var existing = db.repoOperation.GetById(operation.OperationCode);
                        if (existing != null)
                        {
                            await bot.SendMessage(chatId, "Операція з таким ID вже існує! Введіть інший ID:");
                            session.Un = Userenum.operation_code;
                            session.Endb.Clear();
                        }
                        else
                        {
                            db.repoOperation.Add(operation);
                            db.repoOperation.Save();
                            session.Endb.Clear();
                            session.Un = Userenum.None;
                            await bot.SendMessage(chatId, "Операція успішно додана!");
                            await SendTablesMenu(chatId);
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //видалення операції
                case Userenum.operation_delete:

                    if (int.TryParse(msg.Text, out int operationId) && operationId > 0)
                    {
                        var existing = db.repoOperation.GetById(operationId);
                        if (existing != null)
                        {
                            db.repoOperation.Delete(existing);
                            db.repoOperation.Save();
                            session.Endb.Clear();
                            session.Un = Userenum.None;
                            await bot.SendMessage(chatId, "Видалення успішне!");
                            await SendTablesMenu(chatId);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "Помилка! Операції з таким Id не існує!");
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Операції з таким Id не існує!");
                    }
                    break;

                //оновлення операції, пошук по айді
                case Userenum.operation_code_update:

                    if (int.TryParse(msg.Text, out int ID_update_op) && ID_update_op > 0)
                    {
                        var existing = db.repoOperation.GetById(ID_update_op);
                        if (existing != null)
                        {
                            session.Endb["OperationCode"] = msg.Text;
                            session.Un = Userenum.operation_workshop_number_update;
                            await bot.SendMessage(chatId, "Операція знайдена! Введіть новий номер цеху:");
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "Помилка! Операція з таким Id не існує!");
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Операція з таким Id не існує!");
                    }

                    break;

                //оновленння номеру цеху
                case Userenum.operation_workshop_number_update:

                    if (int.TryParse(msg.Text, out int WorkshopNumberUpdate) && WorkshopNumberUpdate > 0 && WorkshopNumberUpdate <= 20)
                    {
                        session.Endb["WorkshopNumber"] = msg.Text;
                        session.Un = Userenum.operation_duration_hours_update;
                        await bot.SendMessage(chatId, "Введіть новий час операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний час!");
                    }
                    break;

                //оновленння тривалості операції
                case Userenum.operation_duration_hours_update:
                    if(int.TryParse(msg.Text, out int DurationHoursUpdate) && DurationHoursUpdate > 0 && DurationHoursUpdate <= 8)
                    {
                        session.Endb["DurationHours"] = msg.Text;
                        session.Un = Userenum.operation_cost_update;
                        await bot.SendMessage(chatId, "Введіть нову вартість виконання операції:");
                        
                    }

                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректну тривалість операції в годинах:");
                    }

                    break;


                //оновленння вартості виконання операції та збереження оновлень
                case Userenum.operation_cost_update:

                    if (decimal.TryParse(msg.Text, out decimal Costup) && Costup > 0)
                    {
                        session.Endb["Costup"] = msg.Text;
                        var existing = db.repoOperation.GetById(int.Parse(session.Endb["OperationCode"]));
                        existing.WorkshopNumber = int.Parse(session.Endb["WorkshopNumber"]);
                        existing.DurationHours = int.Parse(session.Endb["DurationHours"]);
                        existing.Cost = Costup;
                        db.repoOperation.Update(existing);
                        db.repoOperation.Save();
                        session.Endb.Clear();
                        session.Un = Userenum.None;
                        await bot.SendMessage(chatId, "Операція успішно оновлена!");
                        await SendTablesMenu(chatId);
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //додавання код деталі
                case Userenum.production_detail_code:


                    if (int.TryParse(msg.Text, out int DetailCodeP) && DetailCodeP > 0 )
                    {
                        session.Endb["DetailCodeP"] = msg.Text;
                        session.Un = Userenum.production_operation_number_in_process;
                        await bot.SendMessage(chatId, "Введіть номер операції в технологічному процесі (від 1 до 100):");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                //додавання номеру операції
                case Userenum.production_operation_number_in_process:

                    if (int.TryParse(msg.Text, out int OperationNumberInProcess) && OperationNumberInProcess > 0 && OperationNumberInProcess <= 100)
                    {
                        session.Endb["OperationNumberInProcessP"] = msg.Text;
                        session.Un = Userenum.production_operation_code;
                        await bot.SendMessage(chatId, "Введіть код операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер операції!");
                    }
                    break;

                //додавання вартості виконання операції
                case Userenum.production_operation_code:

                    if (int.TryParse(msg.Text, out int OperationCodeP) && OperationCodeP > 0)
                    {
                        session.Endb["OperationCodeP"] = msg.Text;
                        var production = new Production
                        {
                            DetailCode = int.Parse(session.Endb["DetailCodeP"]),
                            OperationNumberInProcess = int.Parse(session.Endb["OperationNumberInProcessP"]),
                            OperationCode = int.Parse(session.Endb["OperationCodeP"])
                        };

                        var detailExists = db.repoDetail.GetById(int.Parse(session.Endb["DetailCodeP"]));
                        var operationExists = db.repoOperation.GetById(OperationCodeP);
                        if (detailExists == null || operationExists == null)
                        {
                            await bot.SendMessage(chatId, "Помилка! Деталь або операція з таким ID не існує!");
                            session.Un = Userenum.production_detail_code;
                            session.Endb.Clear();

                        }
                        else
                        {
                            var existing = db.repoProduction.GetById(production.DetailCode, production.OperationNumberInProcess); ;
                            if (existing != null)
                            {
                                await bot.SendMessage(chatId, "Операція з таким ID вже існує! Введіть інший ID:");
                                session.Un = Userenum.production_detail_code;
                                session.Endb.Clear();
                            }
                            else
                            {
                                db.repoProduction.Add(production);
                                db.repoProduction.Save();
                                session.Endb.Clear();
                                session.Un = Userenum.None;
                                await bot.SendMessage(chatId, "Операція успішно додана!");
                                await SendTablesMenu(chatId);

                            }
                            
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                // по коду деталі
                case Userenum.production_detail_code_delete:
                    if (int.TryParse(msg.Text, out int pdcd) && pdcd > 0)
                    {
                        session.Endb["DetailCodePD"] = msg.Text;
                        session.Un = Userenum.production_operation_number_in_process_delete;
                        await bot.SendMessage(chatId, "Введіть номер операції в технологічному процесі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                //видалення по номеру операції
                case Userenum.production_operation_number_in_process_delete:

                    if (int.TryParse(msg.Text, out int ponupd) && ponupd > 0)
                    {
                        var existing = db.repoProduction.GetById(int.Parse(session.Endb["DetailCodePD"]),ponupd);
                        if (existing != null)
                        {
                            db.repoProduction.Delete(existing);
                            db.repoProduction.Save();
                            session.Endb.Clear();
                            session.Un = Userenum.None;
                            await bot.SendMessage(chatId, "Видалення успішне!");
                            await SendTablesMenu(chatId);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "Помилка! Операції з таким Id не існує!");
                        }
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Операції з таким Id не існує!");
                    }
                    break;
            }
        }

        static async Task SendTablesMenu(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Деталь"),
                    new KeyboardButton("Операція"),
                    new KeyboardButton("Виробництво")
                }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(chatId, "Виберіть таблицю", replyMarkup: keyboard);
        }

        static async Task OnUpdate(Telegram.Bot.Types.Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                var chatId = query.Message.Chat.Id;
                var session = GetOrCreateSession(chatId);

                if (query.Data == "detail_show")
                {
                    var items = db.repoDetail.GetAll();

                    if (!items.Any())
                    {
                        await bot.SendMessage(chatId, "Таблиця порожня!");
                        await SendTablesMenu(chatId);
                    }
                    else
                    {
                        string text = "";

                        foreach (var d in items)
                        {
                            text += $"ID: {d.DetailCode}\n" +
                                    $"Назва деталі : {d.DetailName}\n" +
                                    $"Маса, кг: {d.Mass}\n\n" +
                                    $"Децимальний номер деталі:  {d.DecimalNumber}\n\n " +
                                    $"Марка сплаву : {d.AlloyGrade}\n\n";
                        }

                        await bot.SendMessage(chatId, text);
                        await SendTablesMenu(chatId);
                    }
                }

                else if (query.Data == "detail_add")
                {
                    session.Un = Userenum.detail_code;
                    await bot.SendMessage(chatId, "Введіть ID (від 1 до 50 символів):");
                }

                else if(query.Data == "detail_delete")
                {
                    session.Un = Userenum.detail_delete;
                    await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете видалити:");
                }

                else if (query.Data == "detail_update")
                {
                    session.Un = Userenum.detail_code_update;
                    await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете оновити:");
                }

                

                else if (query.Data == "operation_show")
                {
                    var items = db.repoOperation.GetAll();

                    if (!items.Any())
                    {
                        await bot.SendMessage(chatId, "Таблиця порожня!");
                        await SendTablesMenu(chatId);
                    }
                    else
                    {
                        string text = "";

                        foreach (var o in items)
                        {
                            text += $"ID: {o.OperationCode}\n" +
                                    $"Номер цеху: {o.WorkshopNumber}\n" +
                                    $"Тривалість операції, годин: {o.DurationHours}\n\n" +
                                    $"Вартість виконання операції, грн:  {o.Cost}\n\n ";
                        }
                        await bot.SendMessage(chatId, text);
                        await SendTablesMenu(chatId);
                    }
                }

                else if (query.Data == "operation_add")
                {
                    session.Un = Userenum.operation_code;
                    await bot.SendMessage(chatId, "Введіть ID (від 1 до 50 символів):");
                }

                else if (query.Data == "operation_delete")
                {
                    session.Un = Userenum.operation_delete;
                    await bot.SendMessage(chatId, "Введіть ID операції, яку хочете видалити:");
                }

                else if (query.Data == "operation_update")
                {
                    session.Un = Userenum.operation_code_update;
                    await bot.SendMessage(chatId, "Введіть ID операції, яку хочете оновити:");
                }


                else if (query.Data == "production_show")
                {
                    var items = db.repoProduction.GetAll();

                    if (!items.Any())
                    {
                        await bot.SendMessage(chatId, "Таблиця порожня!");
                        await SendTablesMenu(chatId);
                    }
                    else
                    {
                        string text = "";

                        foreach (var p in items)
                        {
                            text += $"ID деталі: {p.DetailCode}\n" +
                                    $"Номер операції: {p.OperationNumberInProcess}\n" +
                                    $"ID операції: {p.OperationCode}\n\n";
                        }
                        await bot.SendMessage(chatId, text);
                        await SendTablesMenu(chatId);
                    }
                }

                else if (query.Data == "production_add")
                {
                    session.Un = Userenum.production_detail_code;
                    await bot.SendMessage(chatId, "Введіть ID:");
                }

                else if (query.Data == "production_delete")
                {
                    session.Un = Userenum.production_detail_code_delete;
                    await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете видалити:");
                }
            }
        }
    }
}