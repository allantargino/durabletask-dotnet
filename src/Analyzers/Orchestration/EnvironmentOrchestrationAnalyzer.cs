// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.DurableTask.Analyzers.Orchestration.EnvironmentOrchestrationAnalyzer;

namespace Microsoft.DurableTask.Analyzers.Orchestration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnvironmentOrchestrationAnalyzer : OrchestrationAnalyzer<EnvironmentOrchestrationVisitor>
{
    /// <summary>
    /// Diagnostic ID supported for the analyzer.
    /// </summary>
    public const string DiagnosticId = "DURABLE0006";

    static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "TODO",
        "TODO",
        AnalyzersCategories.Orchestration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public class EnvironmentOrchestrationVisitor : MethodProbeOrchestrationVisitor
    {
        /// <inheritdoc/>
        public override bool Initialize()
        {
            return this.KnownTypeSymbols.Environment != null;
        }

        /// <inheritdoc/>
        protected override void VisitMethod(SemanticModel semanticModel, SyntaxNode methodSyntax, IMethodSymbol methodSymbol, string orchestrationName, Action<Diagnostic> reportDiagnostic)
        {
            IOperation? methodOperation = semanticModel.GetOperation(methodSyntax);
            if (methodOperation is null)
            {
                return;
            }

            foreach (IInvocationOperation invocation in methodOperation.Descendants().OfType<IInvocationOperation>())
            {
                IMethodSymbol targetMethod = invocation.TargetMethod;

                if (!targetMethod.ContainingType.Equals(this.KnownTypeSymbols.Environment, SymbolEqualityComparer.Default))
                {
                    return;
                }

                if (targetMethod.Name is "GetEnvironmentVariable" or "GetEnvironmentVariables" or "ExpandEnvironmentVariables")
                {
                    string invocationName = targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                    // e.g.: "The method 'Method1' uses 'Environment.GetEnvironmentVariable()' that may cause non-deterministic behavior when invoked from orchestration 'MyOrchestrator'"
                    reportDiagnostic(RoslynExtensions.BuildDiagnostic(Rule, invocation, methodSymbol.Name, invocationName, orchestrationName));
                }
            }
        }
    }
}
