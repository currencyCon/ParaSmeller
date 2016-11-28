using Microsoft.CodeAnalysis;

namespace ParaSmellerCore.Representation
{
    public class LockBlock : Body
    {
        public override bool IsSynchronized => true;
        public LockBlock(Member member, SyntaxNode implementation): base(member, implementation){ }
    }
}
