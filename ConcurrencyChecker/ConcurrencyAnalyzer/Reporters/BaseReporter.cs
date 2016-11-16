
using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Representation;

namespace ConcurrencyAnalyzer.Reporters
{
    public abstract class BaseReporter
    {
        private readonly ICollection<Action<ClassRepresentation>> _classReports = new List<Action<ClassRepresentation>>();
        private readonly ICollection<Action<MethodRepresentation>> _methodReports = new List<Action<MethodRepresentation>>();
        private readonly ICollection<Action<PropertyRepresentation>> _propertyReports = new List<Action<PropertyRepresentation>>();
        private readonly ICollection<Action<Member>> _memberReports = new List<Action<Member>>();
        public readonly ICollection<Diagnostic> Reports = new List<Diagnostic>();
        public abstract void Register();

        public void Clear()
        {
            _classReports.Clear();
            _methodReports.Clear();
            _propertyReports.Clear();
            _memberReports.Clear();
            Reports.Clear();
        }
        public void RegisterClassReport(Action<ClassRepresentation> classReport)
        {
            lock (this)
            {
                _classReports.Add(classReport);
            }
        }
        public void RegisterMethodReport(Action<MethodRepresentation> methodReport)
        {
            lock (this)
            {
                _methodReports.Add(methodReport);
            }
        }
        public void RegisterPropertyReport(Action<PropertyRepresentation> propertyReport)
        {
            lock (this)
            {
                _propertyReports.Add(propertyReport);
            }
        }
        public void RegisterMemberReport(Action<Member> memberReport)
        {
            lock (this)
            {
                _memberReports.Add(memberReport);
            }
        }
        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            lock (this)
            {
                Clear();
                Register();
                foreach (var clazz in solutionRepresentation.Classes)
                {
                    foreach (var classReport in _classReports)
                    {
                        classReport(clazz);
                    }
                }
                foreach (var method in solutionRepresentation.Classes.SelectMany(e => e.Methods))
                {
                    foreach (var methodReport in _methodReports)
                    {
                        methodReport(method);
                    }
                }
                foreach (var property in solutionRepresentation.Classes.SelectMany(e => e.Properties))
                {
                    foreach (var propertyReport in _propertyReports)
                    {
                        propertyReport(property);
                    }
                }
                foreach (var member in solutionRepresentation.Classes.SelectMany(e => e.Members))
                {
                    foreach (var memberReport in _memberReports)
                    {
                        memberReport(member);
                    }
                }
                return Reports;
            }

        }
    }
}
