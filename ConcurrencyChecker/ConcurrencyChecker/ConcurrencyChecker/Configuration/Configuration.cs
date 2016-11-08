using System.Collections.Generic;

namespace ConcurrencyChecker.Configuration
{
    public class Configuration
    {
        public List<string> SelectedSmells { get; set; }
        public int MaxDepthAsync { get; set; }
    }
}
