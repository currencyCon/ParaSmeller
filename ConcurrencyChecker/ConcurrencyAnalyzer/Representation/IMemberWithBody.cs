using System;
using System.Collections.Generic;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMemberWithBody
    {
        ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        ClassRepresentation ContainingClass { get; set; }
        ICollection<IBody> Blocks { get; set; }
        
    }
}
