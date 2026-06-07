using ReportPluginBase;
using System.Reflection;

namespace Task4
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
                Console.WriteLine("Папку Plugins створено.");
                Console.WriteLine("Скопіюйте туди WordReportPlugin.dll та ExcelReportPlugin.dll.");
                Console.ReadLine();
                return;
            }

            var plugins = LoadPlugins(pluginsPath);

            if (plugins.Count == 0)
            {
                Console.WriteLine("Плагіни не знайдено.");
                Console.WriteLine("Перевірте, чи є DLL-файли у папці Plugins.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Доступні модулі експорту:");

            for (int i = 0; i < plugins.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {plugins[i].Name} - {plugins[i].Description}");
            }

            Console.Write("Оберіть модуль експорту: ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int choice) &&
                choice >= 1 &&
                choice <= plugins.Count)
            {
                plugins[choice - 1].GenerateReport();
            }
            else
            {
                Console.WriteLine("Неправильний вибір.");
            }

            Console.WriteLine("Натисніть Enter для завершення...");
            Console.ReadLine();
        }

        static List<IReportPlugin> LoadPlugins(string pluginsPath)
        {
            var plugins = new List<IReportPlugin>();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                string dependencyPath = Path.Combine(pluginsPath, assemblyName);

                if (File.Exists(dependencyPath))
                {
                    return Assembly.LoadFrom(dependencyPath);
                }

                return null;
            };

            foreach (var file in Directory.GetFiles(pluginsPath, "*Plugin.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);

                    var types = assembly.GetTypes()
                        .Where(t => typeof(IReportPlugin).IsAssignableFrom(t)
                                 && !t.IsInterface
                                 && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        var plugin = Activator.CreateInstance(type) as IReportPlugin;

                        if (plugin != null)
                        {
                            plugins.Add(plugin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не вдалося завантажити файл: " + file);
                    Console.WriteLine("Причина: " + ex.Message);
                }
            }

            return plugins;
        }
    }
}
  