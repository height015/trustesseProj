using System;

namespace trustesseApp.Service.Contracts;

public interface ITemplateReader
{
    Task<string> ReadTemplateAsync(string templateName, bool isHtml = false);
    string ReplacePlaceholders(string content, Dictionary<string, string> replacements);
}