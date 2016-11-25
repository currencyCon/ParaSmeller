
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.RepresentationFactories.ProgressInformation
{
    public class ScopeCalculator
    {
        private Compilation compilation;

        public ScopeCalculator(Compilation compilation)
        {
            this.compilation = compilation;
        }

        private async Task<int> CountTypes(SyntaxTree syntaxTree)
        {
            var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
            var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
            return classes.ToList().Count + interfaces.ToList().Count;
        }

        public async Task<int> CountTypes()
        {
            var countClasses = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                countClasses += await CountTypes(syntaxTree);
            }
            return countClasses;
        }

        public int CountSyntaxTrees()
        {
            return compilation.SyntaxTrees.ToList().Count;
        }
    }
}
