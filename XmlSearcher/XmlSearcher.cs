using System.Xml;
using XmlSearcher.Models;

namespace XmlSearcher;

public class XmlSearcher(string rootDirectory) : IXmlSearcher
{
    public event EventHandler<XmlFileReadingErrorEventArgs>? FileReadingErrorEventHandler;
    public event EventHandler<XmlReaderErrorEventArgs>? ReaderErrorEventHandler;

    private readonly string rootDirectory = Directory.Exists(rootDirectory) ?
            rootDirectory :
            throw new DirectoryNotFoundException($"Directory {rootDirectory} does not exist.");

    public List<SearchResult> Search(string tagName) => SearchInternal(tagName);

    public List<SearchResult> Search(string tagName, string attributeName) => SearchInternal(tagName, attributeName);

    public List<SearchResult> Search(string tagName, string attributeName, string attributeValue) => SearchInternal(tagName, attributeName, attributeValue);

    private List<SearchResult> SearchInternal(string tagName, string? attributeName = null, string? attributeValue = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
        {
            throw new ArgumentException("Invalid root directory specified.");
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("tagName is required.");
        }

        if (attributeName == null && attributeValue != null)
        {
            throw new ArgumentException("attributeName must be specified if attributeValue is provided.");
        }

        var matchingResults = new List<SearchResult>();

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreWhitespace = true
        };

        foreach (var file in Directory.EnumerateFiles(rootDirectory, "*.xml", SearchOption.AllDirectories))
        {
            try
            {
                using var reader = XmlReader.Create(file, settings);
                var lineInfo = (IXmlLineInfo)reader;

                while (reader.Read())
                {
                    try
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == tagName)
                        {
                            var match = true;

                            if (!string.IsNullOrEmpty(attributeName))
                            {
                                var attrValue = reader.GetAttribute(attributeName);

                                if (!string.IsNullOrEmpty(attributeValue) && attrValue != attributeValue)
                                {
                                    match = false;
                                }
                            }

                            if (match && lineInfo.HasLineInfo())
                            {
                                matchingResults.Add(new(Path.GetRelativePath(rootDirectory, file), (uint)lineInfo.LineNumber));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = new Exception($"Error occurred while scanning the file {file}.", ex);
                        if (ReaderErrorEventHandler == null)
                        {
                            throw error;
                        }
                        else
                        {
                            ReaderErrorEventHandler?.Invoke(this, new(file, error));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var error = new Exception($"Error occurred while reading the file {file}.", ex);
                if (FileReadingErrorEventHandler == null)
                {
                    throw error;
                }
                else
                {
                    FileReadingErrorEventHandler?.Invoke(this, new(file, error));
                }
            }
        }

        return matchingResults;
    }
}
