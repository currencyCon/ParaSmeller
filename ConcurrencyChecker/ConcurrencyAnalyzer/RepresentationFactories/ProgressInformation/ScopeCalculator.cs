using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.RepresentationFactories.ProgressInformation
{
    public class ScopeCalculator
    {
        private readonly Compilation _compilation;

        public ScopeCalculator(Compilation compilation)
        {
            _compilation = compilation;
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
            foreach (var syntaxTree in _compilation.SyntaxTrees)
            {
                countClasses += await CountTypes(syntaxTree);
            }
            return countClasses;
        }

        public int CountSyntaxTrees()
        {
            return _compilation.SyntaxTrees.ToList().Count;
        }
    }
}
