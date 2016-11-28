using Microsoft.CodeAnalysis;

namespace ParaSmellerCore.Representation
{
    public class NormalBlock : Body
    {
        public override bool IsSynchronized => false;
        public NormalBlock(Member member, SyntaxNode implementation):base(member, implementation){}
    }
}
