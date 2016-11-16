using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class LockBlock : Body
    {
        public override bool IsSynchronized => true;
        public LockBlock(Member member, SyntaxNode implementation): base(member, implementation){ }
    }
}
