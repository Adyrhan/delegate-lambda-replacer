using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DelegateLambdaReplacer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DelegateLambdaReplacerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DelegateLambdaReplacer";

        // You can change these strings in the Resources.resx file.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Syntax";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, ImmutableArray.Create(SyntaxKind.AnonymousMethodExpression));
        }

        public static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;

            var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

    }
}
