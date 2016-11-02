using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MethodExtensions
    {
        public static List<InvocationExpressionRepresentation> AllInvocationExpressionRepresentations(this MethodRepresentation method)
        {
            var invocations = new List<InvocationExpressionRepresentation>();

            invocations.AddRange(method.InvocationExpressions);

            foreach (var block in method.Blocks)
            {
                GetInvocationExpression(block, invocations);
            }
            
            return invocations;
        }


        private static void GetInvocationExpression(IBody block, List<InvocationExpressionRepresentation> invocations)
        {
            invocations.AddRange(block.InvocationExpressions);

            /*foreach (var subLockBlock in block.Blocks)
            {
                GetInvocationExpression(subLockBlock, invocations);
            }*/
        }


    }
}
