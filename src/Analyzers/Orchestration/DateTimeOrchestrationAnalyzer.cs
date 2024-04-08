﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.DurableTask.Analyzers.Helpers;

namespace Microsoft.DurableTask.Analyzers.Orchestration;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DateTimeOrchestrationAnalyzer : OrchestrationAnalyzer
{
    public const string DiagnosticId = "DF1101";

    static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "DateTime calls must be deterministic inside an orchestration function",
        "The method '{0}' uses '{1}' that may cause non-deterministic behavior when is invoked from Orchestration Function '{2}'",
        AnalyzersCategories.Orchestration,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterAdditionalCompilationStartAction(CompilationStartAnalysisContext context, OrchestrationAnalysisResult orchestrationAnalysisResult)
    {
        INamedTypeSymbol systemDateTimeSymbol = context.Compilation.GetSpecialType(SpecialType.System_DateTime);

        ConcurrentBag<(ISymbol method, IPropertyReferenceOperation operation)> dateTimeUsage = [];

        // search for usages of DateTime.Now, DateTime.UtcNow, DateTime.Today and store them
        context.RegisterOperationAction(ctx =>
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();

            var operation = (IPropertyReferenceOperation)ctx.Operation;
            IPropertySymbol property = operation.Property;

            if (property.ContainingSymbol.Equals(systemDateTimeSymbol, SymbolEqualityComparer.Default) &&
                property.Name is nameof(DateTime.Now) or nameof(DateTime.UtcNow) or nameof(DateTime.Today))
            {
                ISymbol method = ctx.ContainingSymbol;
                dateTimeUsage.Add((method, operation));
            }
        }, OperationKind.PropertyReference);

        // compare whether the found DateTime usages occur in methods invoked by orchestrations
        context.RegisterCompilationEndAction(ctx =>
        {
            foreach ((ISymbol symbol, IPropertyReferenceOperation operation) in dateTimeUsage)
            {
                if (symbol is IMethodSymbol method)
                {
                    if (orchestrationAnalysisResult.OrchestrationsByMethod.TryGetValue(method, out ConcurrentBag<OrchestrationMethod> orchestrations))
                    {
                        string methodName = symbol.Name;
                        string dateTimePropertyName = operation.Property.ToString();
                        string functionsNames = string.Join(", ", orchestrations.Select(o => o.FunctionName).OrderBy(n => n));

                        // e.g.: "The method 'Method' uses 'System.Date.Now' that may cause non-deterministic behavior when is invoked from Orchestration Function 'Run'"
                        ctx.ReportDiagnostic(Rule, operation, methodName, dateTimePropertyName, functionsNames);
                    }
                }
            }
        });
    }
}
