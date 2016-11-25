
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.RepresentationFactories.ProgressInformation
{
    public class ScopeCalculator
    {
        private static async Task<int> CountTypes(SyntaxTree syntaxTree)
        {
            var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
            var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
            return classes.ToList().Count + interfaces.ToList().Count;
        }

        public static async Task<int> CountTypes(Compilation compilation)
        {
            var countClasses = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                countClasses += await CountTypes(syntaxTree);
            }
            return countClasses;
        }
    }
}
