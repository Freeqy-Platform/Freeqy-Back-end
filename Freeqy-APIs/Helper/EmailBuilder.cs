namespace Freeqy_APIs.Helper;

public class EmailBuilder
{
    public static string GenerateEmailBody(string templateName, Dictionary<string, string> emailBody)
    {
        var path = $"{Directory.GetCurrentDirectory()}/Templates/{templateName}.html";
        var reader = new StreamReader(File.OpenRead(path));
        string temp =  reader.ReadToEnd();
        reader.Close();

        foreach (var item in emailBody)
        {
            temp = temp.Replace(item.Key, item.Value);
        }
        
        return temp;
    }
}