using System;

namespace XmlSearcher.Models;
public class XmlReaderErrorEventArgs(string file, Exception exception) 
    : EventArgs
{
    public string File => file;

    public Exception Exception => exception;
}
