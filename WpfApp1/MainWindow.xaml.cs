using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XmlSearcher;

namespace WpfApp1;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Txt_RootDirectory.Text))
        {
            throw new Exception("Root directory is not selected.");
        }

        if (string.IsNullOrWhiteSpace(Txt_TagName.Text))
        {
            throw new Exception("Tag name is required.");
        }

        if (string.IsNullOrWhiteSpace(Txt_AttributeName.Text) && !string.IsNullOrWhiteSpace(Txt_AttributeValue.Text))
        {
            throw new Exception("Attribute name is required when attribute value is given.");
        }
    }

    private void ClearOutput()
    {
        Txt_Results.Text = string.Empty;
        Txt_Errors.Text = string.Empty;
    }

    private void BlockOperations()
    {
        Win_Main.IsEnabled = false;
    }

    private void UnblockOperations()
    {
        Win_Main.IsEnabled = true;
    }

    private void ShowError(string message, string title = "Error") => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    private void Btn_BrowseRootDirectory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Root Directory"
        };

        if (dialog.ShowDialog() is true)
        {
            var path = System.IO.Path.GetDirectoryName(dialog.FileName);
            Txt_RootDirectory.Text = path;
        }
    }

    private async void Btn_Search_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BlockOperations();
            ValidateInputs();
            ClearOutput();
            var searcher = new XmlSearcher.XmlSearcher(Txt_RootDirectory.Text);

            searcher.ReaderErrorEventHandler += (sender, e) => Dispatcher.Invoke(() => Txt_Errors.Text += $"Error in file {e.File}: {e.Exception.Message}{Environment.NewLine}");
            searcher.FileReadingErrorEventHandler += (sender, e) => Dispatcher.Invoke(() => Txt_Errors.Text += $"Error in file {e.File}: {e.Exception.Message}{Environment.NewLine}");

            var tagName = Txt_TagName.Text;
            var attributeName = Txt_AttributeName.Text;
            var attributeValue = Txt_AttributeValue.Text;
            var results = await Task.Run(() => searcher.Search(tagName, attributeName, attributeValue));
            results.ForEach(r => Txt_Results.Text += $"Found in file {r.File} at line number {r.LineNumber}.{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            UnblockOperations();
        }
    }
}