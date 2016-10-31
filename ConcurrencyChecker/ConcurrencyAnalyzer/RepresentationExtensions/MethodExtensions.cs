using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
