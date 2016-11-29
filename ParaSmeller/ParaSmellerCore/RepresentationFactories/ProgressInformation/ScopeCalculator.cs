using System.Linq;
using System.Threading;
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

        private static int CountTypes(SyntaxTree syntaxTree)
        {
            var classes =  SyntaxNodeFilter.GetClasses(syntaxTree);
            var interfaces = SyntaxNodeFilter.GetInterfaces(syntaxTree);
            return classes.ToList().Count + interfaces.ToList().Count;
        }

        public int CountTypes()
        {
            var countClasses = 0;
            Parallel.ForEach(_compilation.SyntaxTrees, syntaxTree =>
            {
                Interlocked.Add(ref countClasses, CountTypes(syntaxTree));
            });
            return countClasses;
        }

        public int CountSyntaxTrees()
        {
            return _compilation.SyntaxTrees.ToList().Count;
        }
    }
}
