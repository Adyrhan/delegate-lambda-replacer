using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using DelegateLambdaReplacer;

namespace DelegateLambdaReplacer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void NoCodeTriggersNoDiagnostic()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void SingleStatementDelegateBodiesAreReplacedByBodylessLambdaExpressions()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Method()
            {
                Action<int> myDelegate = delegate(int someVar) {
                    Console.WriteLine(""lel"");
                };
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DelegateLambdaReplacer",
                Message = "Definition of 'myDelegate' has been made using a delegate instead of a lambda expression.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 42)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Method()
            {
                Action<int> myDelegate = (int someVar) => Console.WriteLine(""lel"");
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void ShouldCreateLambdaWithBlockIfDelegateBodyContainsMoreThanOneStatement()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Method()
            {
                Action<int> myDelegate = delegate(int someVar) {
                    Console.WriteLine(""lel"");
                    Console.WriteLine(""lol"");
                };
            }
        }
    }";
            var fixedTest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public void Method()
            {
                Action<int> myDelegate = (int someVar) =>
                {
                    Console.WriteLine(""lel"");
                    Console.WriteLine(""lol"");
                };
            }
        }
    }";
            VerifyCSharpFix(test, fixedTest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DelegateLambdaReplacerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DelegateLambdaReplacerAnalyzer();
        }
    }
}