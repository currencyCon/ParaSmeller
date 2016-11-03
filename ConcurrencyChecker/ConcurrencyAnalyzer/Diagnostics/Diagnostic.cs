

using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Diagnostics
{
    public class Diagnostic
    {
        public readonly string Id;
        public readonly LocalizableString Title; 

        public readonly LocalizableString MessageFormat;
        public readonly LocalizableString Description;
        public readonly string Category;
        public readonly Location Location;
        public readonly object[] Params;


        public Diagnostic(string id, LocalizableString title, LocalizableString messageFormat, LocalizableString description,
            string category)
        {
            Id = id;
            Title = title;
            MessageFormat = messageFormat;
            Description = description;
            Category = category;
        }

        public Diagnostic(string id, LocalizableString title, LocalizableString messageFormat, LocalizableString description,
    string category, Location location, object[] par)
        {
            Id = id;
            Title = title;
            MessageFormat = messageFormat;
            Description = description;
            Category = category;
            Location = location;
            Params = par;
        }

        public Diagnostic(string id, LocalizableString title, LocalizableString messageFormat, LocalizableString description,
string category, Location location)
        {
            Id = id;
            Title = title;
            MessageFormat = messageFormat;
            Description = description;
            Category = category;
            Location = location;
        }
    }
}
