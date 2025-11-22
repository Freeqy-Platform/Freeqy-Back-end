namespace Freeqy_APIs.Helper;

public class EmailBuilder
{
    public static string GenerateEmailBody(string templateName, Dictionary<string, string> emailBody)
    {
        var rootPath = AppContext.BaseDirectory; 
        var path = Path.Combine(rootPath, "Templates", $"{templateName}.html");

        if (!File.Exists(path))
            throw new FileNotFoundException("Template not found", path);

        string temp = File.ReadAllText(path);

        foreach (var item in emailBody)
        {
            temp = temp.Replace(item.Key, item.Value);
        }

        return temp;
    }
}