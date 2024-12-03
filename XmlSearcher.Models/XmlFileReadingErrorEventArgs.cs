namespace XmlSearcher.Models;
public class XmlFileReadingErrorEventArgs(string file, Exception exception)
    : EventArgs
{
    public string File => file;

    public Exception Exception => exception;
}
