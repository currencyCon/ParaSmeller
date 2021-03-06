﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class ClassRepresentationFactory
    {
        public static ClassRepresentation Create(ClassDeclarationSyntax syntaxTree, SemanticModel semanticModel)
        {
            var classRepresentation = new ClassRepresentation(syntaxTree, semanticModel);
            AddMethods(classRepresentation, semanticModel);
            AddProperties(classRepresentation, semanticModel);
            InitMembers(classRepresentation);
            return classRepresentation;
        }

        private static void InitMembers(ClassRepresentation classRepresentation)
        {
            classRepresentation.SynchronizedMethods = GetMembers<MethodRepresentation>(classRepresentation,true);
            classRepresentation.UnSynchronizedMethods = GetMembers<MethodRepresentation>(classRepresentation, false);
            classRepresentation.SynchronizedProperties = GetMembers<PropertyRepresentation>(classRepresentation, true);
            classRepresentation.UnSynchronizedProperties = GetMembers<PropertyRepresentation>(classRepresentation, false);
        }

        private static ICollection<TMember> GetMembers<TMember>(ClassRepresentation classRepresentation,
            bool synchronized) where TMember : Member
        {
            var members = classRepresentation.Members.Where(e => e is TMember);
            members = FilterSynchronization(synchronized, members);
            return members.Select(e => e as TMember).ToList();
        }

        private static IEnumerable<Member> FilterSynchronization(bool synchronized, IEnumerable<Member> members)
        {
            if (synchronized)
            {
                members = members.Where(e => e.IsFullySynchronized());
            }
            else
            {
                members = members.Where(e => !e.IsFullySynchronized());
            }
            return members;
        }

        private static void AddProperties(ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var properties = classRepresentation.Implementation.GetChildren<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in properties)
            {
                classRepresentation.Members.Add(PropertyRepresentationFactory.Create(propertyDeclarationSyntax, classRepresentation, semanticModel));
            }
        }

        private static void AddMethods(ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methods = classRepresentation.Implementation.GetChildren<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methods)
            {
                classRepresentation.Members.Add(MethodRepresentationFactory.Create(methodDeclarationSyntax, classRepresentation, semanticModel));
            }
        }
    }
}
