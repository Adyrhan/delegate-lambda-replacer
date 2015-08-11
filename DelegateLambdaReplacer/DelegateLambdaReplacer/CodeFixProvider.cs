using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace DelegateLambdaReplacer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DelegateLambdaReplacerCodeFixProvider)), Shared]
    public class DelegateLambdaReplacerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Replace with lambda";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DelegateLambdaReplacerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            //var declaration = root.FindNode(diagnosticSpan).Parent.AncestorsAndSelf().OfType<AnonymousMethodExpressionSyntax>().First();

            var declaration = (AnonymousMethodExpressionSyntax) root.FindNode(diagnosticSpan);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MakeLambdaExpressionAsync(context.Document, declaration, c),
                    equivalenceKey: title
                ),
                diagnostic
            );
        }

        private async Task<Document> MakeLambdaExpressionAsync(Document document, AnonymousMethodExpressionSyntax anonMethod, CancellationToken cancellationToken)
        {
            var parent = anonMethod.Parent;

            var parameterList = anonMethod.ParameterList != null ? anonMethod.ParameterList : SyntaxFactory.ParameterList();

            //var body = anonMethod.Body != null ? anonMethod.Body : SyntaxFactory.Block().;

            SyntaxNode body;

            if (anonMethod.Block != null && anonMethod.Block.Statements.Count == 1)
            {
                body = anonMethod.Block.Statements.ElementAt(0).ChildNodes().ElementAt(0);
            }
            else if(anonMethod.Block != null)
            {
                body = anonMethod.Body;
            }
            else
            {
                body = SyntaxFactory.Block();
            }

            var lambdaExpr = SyntaxFactory.
                ParenthesizedLambdaExpression(parameterList, (CSharpSyntaxNode)body);

            var newParent = parent.ReplaceNode(anonMethod, lambdaExpr);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newRoot = root.ReplaceNode(parent, newParent);

            return document.WithSyntaxRoot(newRoot);
        }

    }
}