using System;

namespace trustesseApp.Service;

// Infrastructure/Services/TemplateReader.cs
using System.Reflection;
using trustesseApp.Service.Contracts;

public class TemplateReader : ITemplateReader
{
    public async Task<string> ReadTemplateAsync(string templateName, bool isHtml = false)
    {
        var extension = isHtml ? ".html" : ".txt";
        var fullTemplateName = Path.GetFileNameWithoutExtension(templateName) + extension;
        
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.Core.Infrastructure.EmailTemplates.{fullTemplateName}";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException($"Template {fullTemplateName} not found");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }


    public string ReplacePlaceholders(string content, Dictionary<string, string> replacements)
    {
        foreach (var (key, value) in replacements)
        {
            content = content.Replace($"{{{{{key}}}}}", value);
        }
        return content;
    }
}