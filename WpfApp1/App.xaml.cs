using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;

namespace WpfApp1;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly object _lock = new();
    private const string fileName = "errors.log";

    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError(GetErrorFromException(ex));
            }
            else
            {
                LogError("Unhandled exception caught by app domain event handler.");
            }
        };

        DispatcherUnhandledException += (_, e) => LogError(GetErrorFromException(e.Exception));

        TaskScheduler.UnobservedTaskException += (_, e) => LogError(GetErrorFromException(e.Exception));
    }

    private static string GetErrorFromException(Exception? ex)
    {
        var sb = new StringBuilder();
        for (int i = 0; ex is not null; i++, ex = ex.InnerException)
        {
            if (i > 0)
            {
                sb.AppendLine("----------------------------------- INNER EXCEPTION -------------------------------------");
            }

            sb.AppendLine(ex.GetType().FullName + ": " + ex.Message);
            sb.AppendLine(ex.StackTrace);
        }

        return sb.ToString();
    }

    private static void LogError(string error)
    {
        lock (_lock)
        {
            File.AppendAllText(fileName, Environment.NewLine + error);
        }
    }
}

