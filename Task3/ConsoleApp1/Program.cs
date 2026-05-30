using Domain;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient bot = null!;
        static DB db = null!;
        static Dictionary<long, UserSession> sessions = new Dictionary<long, UserSession>();
        static EntityTextServant textServant = new EntityTextServant();

        static UserSession GetOrCreateSession(long chatId)
        {
            if (!sessions.ContainsKey(chatId))
            {
                sessions[chatId] = new UserSession();
            }

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
            await Task.CompletedTask;
        }

        static async Task OnMessage(Message msg, UpdateType type)
        {
            if (string.IsNullOrWhiteSpace(msg.Text))
            {
                return;
            }

            string text = msg.Text;

            var chatId = msg.Chat.Id;
            var session = GetOrCreateSession(chatId);

            if (text == "/start" && session.Un == Userenum.None)
            {
                await bot.SendMessage(chatId,
                    "Вітаю! Ви запустили бота Manufacturing of parts! " +
                    "Команди:\"/help\", \"/exit\"");

                await SendTablesMenu(chatId);
            }
            else if (text == "Деталь" && session.Un == Userenum.None)
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
            else if (text == "Операція" && session.Un == Userenum.None)
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
            else if (text == "Виробництво" && session.Un == Userenum.None)
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

            if (text == "/help")
            {
                await bot.SendMessage(chatId,
                    "Таблиця \"Операція\" - в цій таблиці ви вводите рядки: Код операції (ціле число 1-50 символів), Номер цеху(від 1 до 20),Тривалість операції в годинах (1-8 годин), Вартість виконання операції, грн (число із двома знаками після коми)." +
                    "Таблиця \"Деталь\" - в цій таблиці ви вводите рядки: Код деталі,Децимальний номер деталі (до 20 символів, формат: хххх.хххх.ххх-хх), Назва деталі, Марка сплаву, Маса, кг( формат: ххх,ххх)." +
                    "Таблиця \"Виробництво\" - в цій таблиці ви вводите рядки: існуючий код деталі, Номер операції в технологічному процесі (від 1 до 100) та існуючий код операції.");
            }
            else if (text == "/exit")
            {
                session.Endb.Clear();
                session.Un = Userenum.None;

                await bot.SendMessage(chatId, "До побачення! Для повернення введіть /start");
            }

            switch (session.Un)
            {
                case Userenum.detail_code:
                    if (int.TryParse(text, out int detailCode) && detailCode > 0)
                    {
                        session.Endb["DetailCode"] = text;
                        session.Un = Userenum.detail_decimal_number;

                        await bot.SendMessage(chatId, "Введіть децімальний номер, у форматі хххх.хххх.ххх-хх:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                case Userenum.detail_decimal_number:
                    if (Regex.IsMatch(text, @"^\d{4}\.\d{4}\.\d{3}-\d{2}$"))
                    {
                        session.Endb["DecimalNumber"] = text;
                        session.Un = Userenum.detail_name;

                        await bot.SendMessage(chatId, "Введіть назву деталі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                case Userenum.detail_name:
                    session.Endb["DetailName"] = text;
                    session.Un = Userenum.detail_alloy_grade;

                    await bot.SendMessage(chatId, "Введіть матеріал деталі:");
                    break;

                case Userenum.detail_alloy_grade:
                    session.Endb["AlloyGrade"] = text;
                    session.Un = Userenum.detail_mass;

                    await bot.SendMessage(chatId, "Введіть масу деталі (формат: ххх,ххх):");
                    break;

                case Userenum.detail_mass:
                    if (Regex.IsMatch(text, @"^\d{1,3},\d{1,3}$"))
                    {
                        decimal mass = decimal.Parse(text.Replace(",", "."), CultureInfo.InvariantCulture);

                        var detail = new Detail
                        {
                            DetailCode = int.Parse(session.Endb["DetailCode"]),
                            DecimalNumber = session.Endb["DecimalNumber"],
                            DetailName = session.Endb["DetailName"],
                            AlloyGrade = session.Endb["AlloyGrade"],
                            Mass = mass
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

                case Userenum.detail_delete:
                    if (int.TryParse(text, out int deleteId) && deleteId > 0)
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

                case Userenum.detail_code_update:
                    if (int.TryParse(text, out int detailIdUpdate) && detailIdUpdate > 0)
                    {
                        var existing = db.repoDetail.GetById(detailIdUpdate);

                        if (existing != null)
                        {
                            session.Endb["DetailCode"] = text;
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

                case Userenum.detail_decimal_number_update:
                    if (text.Length >= 1 && text.Length <= 20)
                    {
                        session.Endb["DecimalNumber"] = text;
                        session.Un = Userenum.detail_name_update;

                        await bot.SendMessage(chatId, "Введіть нову назву деталі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                case Userenum.detail_name_update:
                    session.Endb["DetailName"] = text;
                    session.Un = Userenum.detail_alloy_grade_update;

                    await bot.SendMessage(chatId, "Введіть нову назву матеріалу деталі:");
                    break;

                case Userenum.detail_alloy_grade_update:
                    session.Endb["AlloyGrade"] = text;
                    session.Un = Userenum.detail_mass_update;

                    await bot.SendMessage(chatId, "Введіть нову масу деталі (формат: ххх,ххх):");
                    break;

                case Userenum.detail_mass_update:
                    if (decimal.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal massUpdate) && massUpdate > 0)
                    {
                        var existing = db.repoDetail.GetById(int.Parse(session.Endb["DetailCode"]));

                        if (existing == null)
                        {
                            await bot.SendMessage(chatId, "Помилка! Деталь не знайдена!");

                            session.Endb.Clear();
                            session.Un = Userenum.None;

                            await SendTablesMenu(chatId);
                            break;
                        }

                        existing.DecimalNumber = session.Endb["DecimalNumber"];
                        existing.DetailName = session.Endb["DetailName"];
                        existing.AlloyGrade = session.Endb["AlloyGrade"];
                        existing.Mass = massUpdate;

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

                case Userenum.operation_code:
                    if (int.TryParse(text, out int operationCode) && operationCode > 0)
                    {
                        session.Endb["OperationCode"] = text;
                        session.Un = Userenum.operation_workshop_number;

                        await bot.SendMessage(chatId, "Введіть номер цеху, від 1 до 20:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                case Userenum.operation_workshop_number:
                    if (int.TryParse(text, out int workshopNumber) && workshopNumber > 0 && workshopNumber <= 20)
                    {
                        session.Endb["WorkshopNumber"] = text;
                        session.Un = Userenum.operation_duration_hours;

                        await bot.SendMessage(chatId, "Введіть тривалість операції в годинах:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер цеху!");
                    }
                    break;

                case Userenum.operation_duration_hours:
                    if (int.TryParse(text, out int durationHours) && durationHours > 0 && durationHours <= 8)
                    {
                        session.Endb["DurationHours"] = text;
                        session.Un = Userenum.operation_cost;

                        await bot.SendMessage(chatId, "Введіть вартість виконання операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректну тривалість операції в годинах:");
                    }
                    break;

                case Userenum.operation_cost:
                    if (decimal.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cost) && cost > 0)
                    {
                        cost = Math.Round(cost, 2);

                        var operation = new Operation
                        {
                            OperationCode = int.Parse(session.Endb["OperationCode"]),
                            WorkshopNumber = int.Parse(session.Endb["WorkshopNumber"]),
                            DurationHours = int.Parse(session.Endb["DurationHours"]),
                            Cost = cost
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

                case Userenum.operation_delete:
                    if (int.TryParse(text, out int operationId) && operationId > 0)
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

                case Userenum.operation_code_update:
                    if (int.TryParse(text, out int operationIdUpdate) && operationIdUpdate > 0)
                    {
                        var existing = db.repoOperation.GetById(operationIdUpdate);

                        if (existing != null)
                        {
                            session.Endb["OperationCode"] = text;
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

                case Userenum.operation_workshop_number_update:
                    if (int.TryParse(text, out int workshopNumberUpdate) && workshopNumberUpdate > 0 && workshopNumberUpdate <= 20)
                    {
                        session.Endb["WorkshopNumber"] = text;
                        session.Un = Userenum.operation_duration_hours_update;

                        await bot.SendMessage(chatId, "Введіть новий час операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний час!");
                    }
                    break;

                case Userenum.operation_duration_hours_update:
                    if (int.TryParse(text, out int durationHoursUpdate) && durationHoursUpdate > 0 && durationHoursUpdate <= 8)
                    {
                        session.Endb["DurationHours"] = text;
                        session.Un = Userenum.operation_cost_update;

                        await bot.SendMessage(chatId, "Введіть нову вартість виконання операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректну тривалість операції в годинах:");
                    }
                    break;

                case Userenum.operation_cost_update:
                    if (decimal.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costUpdate) && costUpdate > 0)
                    {
                        var existing = db.repoOperation.GetById(int.Parse(session.Endb["OperationCode"]));

                        if (existing == null)
                        {
                            await bot.SendMessage(chatId, "Помилка! Операція не знайдена!");

                            session.Endb.Clear();
                            session.Un = Userenum.None;

                            await SendTablesMenu(chatId);
                            break;
                        }

                        existing.WorkshopNumber = int.Parse(session.Endb["WorkshopNumber"]);
                        existing.DurationHours = int.Parse(session.Endb["DurationHours"]);
                        existing.Cost = Math.Round(costUpdate, 2);

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

                case Userenum.production_detail_code:
                    if (int.TryParse(text, out int detailCodeP) && detailCodeP > 0)
                    {
                        session.Endb["DetailCodeP"] = text;
                        session.Un = Userenum.production_operation_number_in_process;

                        await bot.SendMessage(chatId, "Введіть номер операції в технологічному процесі (від 1 до 100):");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть числове значення!");
                    }
                    break;

                case Userenum.production_operation_number_in_process:
                    if (int.TryParse(text, out int operationNumberInProcess) && operationNumberInProcess > 0 && operationNumberInProcess <= 100)
                    {
                        session.Endb["OperationNumberInProcessP"] = text;
                        session.Un = Userenum.production_operation_code;

                        await bot.SendMessage(chatId, "Введіть код операції:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер операції!");
                    }
                    break;

                case Userenum.production_operation_code:
                    if (int.TryParse(text, out int operationCodeP) && operationCodeP > 0)
                    {
                        var production = new Production
                        {
                            DetailCode = int.Parse(session.Endb["DetailCodeP"]),
                            OperationNumberInProcess = int.Parse(session.Endb["OperationNumberInProcessP"]),
                            OperationCode = operationCodeP
                        };

                        var detailExists = db.repoDetail.GetById(production.DetailCode);
                        var operationExists = db.repoOperation.GetById(production.OperationCode);

                        if (detailExists == null || operationExists == null)
                        {
                            await bot.SendMessage(chatId, "Помилка! Деталь або операція з таким ID не існує!");

                            session.Un = Userenum.production_detail_code;
                            session.Endb.Clear();
                        }
                        else
                        {
                            var existing = db.repoProduction.GetById(production.DetailCode, production.OperationNumberInProcess);

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

                case Userenum.production_detail_code_delete:
                    if (int.TryParse(text, out int detailCodeDelete) && detailCodeDelete > 0)
                    {
                        session.Endb["DetailCodePD"] = text;
                        session.Un = Userenum.production_operation_number_in_process_delete;

                        await bot.SendMessage(chatId, "Введіть номер операції в технологічному процесі:");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Помилка! Введіть коректний номер!");
                    }
                    break;

                case Userenum.production_operation_number_in_process_delete:
                    if (int.TryParse(text, out int operationNumberDelete) && operationNumberDelete > 0)
                    {
                        if (!session.Endb.ContainsKey("DetailCodePD"))
                        {
                            await bot.SendMessage(chatId, "Помилка! Спочатку введіть ID деталі.");
                            break;
                        }

                        var existing = db.repoProduction.GetById(int.Parse(session.Endb["DetailCodePD"]), operationNumberDelete);

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

        static async Task OnUpdate(Update update)
        {
            if (update.CallbackQuery == null)
            {
                return;
            }

            var query = update.CallbackQuery;

            if (query.Message == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(query.Data))
            {
                return;
            }

            string data = query.Data;

            var chatId = query.Message.Chat.Id;
            var session = GetOrCreateSession(chatId);

            if (data == "detail_show")
            {
                var items = db.repoDetail.GetAll();

                if (!items.Any())
                {
                    await bot.SendMessage(chatId, "Таблиця порожня!");
                    await SendTablesMenu(chatId);
                }
                else
                {
                    var text = new StringBuilder();

                    foreach (var d in items)
                    {
                        text.AppendLine(textServant.FormatDetail(d));
                    }

                    await bot.SendMessage(chatId, text.ToString());
                    await SendTablesMenu(chatId);
                }
            }
            else if (data == "detail_add")
            {
                session.Un = Userenum.detail_code;
                await bot.SendMessage(chatId, "Введіть ID (від 1 до 50 символів):");
            }
            else if (data == "detail_delete")
            {
                session.Un = Userenum.detail_delete;
                await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете видалити:");
            }
            else if (data == "detail_update")
            {
                session.Un = Userenum.detail_code_update;
                await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете оновити:");
            }
            else if (data == "operation_show")
            {
                var items = db.repoOperation.GetAll();

                if (!items.Any())
                {
                    await bot.SendMessage(chatId, "Таблиця порожня!");
                    await SendTablesMenu(chatId);
                }
                else
                {
                    var text = new StringBuilder();

                    foreach (var o in items)
                    {
                        text.AppendLine(textServant.FormatOperation(o));
                    }

                    await bot.SendMessage(chatId, text.ToString());
                    await SendTablesMenu(chatId);
                }
            }
            else if (data == "operation_add")
            {
                session.Un = Userenum.operation_code;
                await bot.SendMessage(chatId, "Введіть ID (від 1 до 50 символів):");
            }
            else if (data == "operation_delete")
            {
                session.Un = Userenum.operation_delete;
                await bot.SendMessage(chatId, "Введіть ID операції, яку хочете видалити:");
            }
            else if (data == "operation_update")
            {
                session.Un = Userenum.operation_code_update;
                await bot.SendMessage(chatId, "Введіть ID операції, яку хочете оновити:");
            }
            else if (data == "production_show")
            {
                var items = db.repoProduction.GetAll();

                if (!items.Any())
                {
                    await bot.SendMessage(chatId, "Таблиця порожня!");
                    await SendTablesMenu(chatId);
                }
                else
                {
                    var text = new StringBuilder();

                    foreach (var p in items)
                    {
                        text.AppendLine(textServant.FormatProduction(p));
                    }

                    await bot.SendMessage(chatId, text.ToString());
                    await SendTablesMenu(chatId);
                }
            }
            else if (data == "production_add")
            {
                session.Un = Userenum.production_detail_code;
                await bot.SendMessage(chatId, "Введіть ID:");
            }
            else if (data == "production_delete")
            {
                session.Un = Userenum.production_detail_code_delete;
                await bot.SendMessage(chatId, "Введіть ID деталі, яку хочете видалити:");
            }
        }
    }
}