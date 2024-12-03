namespace XmlSearcher.Models;

public interface IXmlSearcher
{
    List<SearchResult> Search(string tagName);

    List<SearchResult> Search(string tagName, string attributeName);

    List<SearchResult> Search(string tagName, string attributeName, string attributeValue);
}
