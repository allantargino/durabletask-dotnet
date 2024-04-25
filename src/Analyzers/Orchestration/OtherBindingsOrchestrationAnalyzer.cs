// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Microsoft.DurableTask.Analyzers.Orchestration.OtherBindingsOrchestrationAnalyzer;

namespace Microsoft.DurableTask.Analyzers.Orchestration;

/// <summary>
/// Analyzer that reports a warning when a Durable Function Orchestration has parameters bindings other than OrchestrationTrigger.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
class OtherBindingsOrchestrationAnalyzer : OrchestrationAnalyzer<OtherBindingsOrchestrationOrchestrationVisitor>
{
    /// <summary>
    /// Diagnostic ID supported for the analyzer.
    /// </summary>
    public const string DiagnosticId = "DURABLE0008";

    static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "TODO",
        "TODO",
        AnalyzersCategories.Orchestration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Visitor that inspects Durable Functions's method signatures for parameters binding other than OrchestrationTrigger.
    /// </summary>
    public sealed class OtherBindingsOrchestrationOrchestrationVisitor : OrchestrationVisitor
    {
        /// <inheritdoc/>
        public override bool Initialize()
        {
            return this.KnownTypeSymbols.DurableClientAttribute is not null;
        }

        /// <inheritdoc/>
        public override void VisitDurableFunction(SemanticModel sm, MethodDeclarationSyntax methodSyntax, IMethodSymbol methodSymbol, string orchestrationName, Action<Diagnostic> reportDiagnostic)
        {
            foreach (IParameterSymbol parameter in methodSymbol.Parameters)
            {
                IEnumerable<INamedTypeSymbol?> attributesSymbols = parameter.GetAttributes().Select(att => att.AttributeClass);

                if (attributesSymbols.Any(att => this.KnownTypeSymbols.DurableClientAttribute!.Equals(att, SymbolEqualityComparer.Default)))
                {
                    reportDiagnostic(RoslynExtensions.BuildDiagnostic(Rule, parameter));
                }
            }
        }
    }
}
