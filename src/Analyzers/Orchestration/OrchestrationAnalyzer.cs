// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.DurableTask.Analyzers.Helpers;

namespace Microsoft.DurableTask.Analyzers.Orchestration;

public abstract class OrchestrationAnalyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(context =>
        {
            var knownSymbols = new KnownTypeSymbols(context.Compilation);

            if (knownSymbols.OrchestrationTriggerAttribute == null || knownSymbols.FunctionAttribute == null)
            {
                return;
            }

            ConcurrentDictionary<IMethodSymbol, ConcurrentBag<OrchestrationMethod>> orchestrationsByMethod = new(SymbolEqualityComparer.Default);

            context.RegisterSyntaxNodeAction(ctx =>
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                // Checks whether the declared method is an orchestration, if not, returns
                if (ctx.ContainingSymbol is not IMethodSymbol methodSymbol ||
                    !methodSymbol.ContainsAttributeInAnyMethodArguments(knownSymbols.OrchestrationTriggerAttribute) ||
                    !methodSymbol.TryGetSingleValueFromAttribute(knownSymbols.FunctionAttribute, out string functionName))
                {
                    return;
                }

                var orchestration = new OrchestrationMethod(functionName, methodSymbol);
                bool added = orchestrationsByMethod.TryAdd(methodSymbol, new ConcurrentBag<OrchestrationMethod>([orchestration])); // the orchestration is considered reachable by itself, aka the root
                Debug.Assert(added, "an orchestration method declaration must not be visited twice");

                var methodSyntax = (MethodDeclarationSyntax)ctx.Node;
                FindAndAddInvokedMethods(ctx.SemanticModel, methodSyntax, orchestrationsByMethod, orchestration);
            }, SyntaxKind.MethodDeclaration);

            // allows concrete implementations to register specific actions/analysis and then compare against methodsInvokedByOrchestrations
            this.RegisterAdditionalCompilationStartAction(context, orchestrationsByMethod);
        });
    }

    static void FindAndAddInvokedMethods(
        SemanticModel semanticModel, MethodDeclarationSyntax callerSyntax,
        ConcurrentDictionary<IMethodSymbol, ConcurrentBag<OrchestrationMethod>> orchestrationsByMethod,
        OrchestrationMethod rootOrchestration)
    {
        foreach (InvocationExpressionSyntax invocation in callerSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            IOperation? calleOperation = semanticModel.GetOperation(invocation);
            if (calleOperation == null || calleOperation is not IInvocationOperation calleInvocation)
            {
                continue;
            }

            IMethodSymbol calleeSymbol = calleInvocation.TargetMethod;
            if (calleeSymbol == null)
            {
                continue;
            }

            ConcurrentBag<OrchestrationMethod> orchestrations = orchestrationsByMethod.GetOrAdd(calleeSymbol, []);
            
            // avoid infinite recursion
            if (orchestrations.Contains(rootOrchestration))
            {
                continue;
            }
            orchestrations.Add(rootOrchestration);

            // iterating over multiple syntax references is needed because the same method can be declared in multiple places (e.g. partial classes)
            IEnumerable<MethodDeclarationSyntax> calleeSyntaxes = calleeSymbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax calleeSyntax in calleeSyntaxes)
            {
                FindAndAddInvokedMethods(semanticModel, calleeSyntax, orchestrationsByMethod, rootOrchestration);
            }
        }
    }

    /// <summary>
    /// Register additional actions to be executed after the compilation has started.
    /// It is expected from a concrete implementation of <see cref="OrchestrationAnalyzer"/> to register a
    /// <see cref="CompilationStartAnalysisContext.RegisterCompilationEndAction"/>
    /// and then compare that any discovered violations happened in any of the methods in <paramref name="orchestrationsByMethod"/>.
    /// </summary>
    /// <param name="context">Context originally provided by <see cref="AnalysisContext.RegisterCompilationAction"/></param>
    /// <param name="orchestrationsByMethod">Collection of Orchestration methods or methods that are invoked by them</param>
    protected abstract void RegisterAdditionalCompilationStartAction(CompilationStartAnalysisContext context, ConcurrentDictionary<IMethodSymbol, ConcurrentBag<OrchestrationMethod>> orchestrationsByMethod);

    [DebuggerDisplay("[{FunctionName}] {OrchestrationMethodSymbol.Name}")]
    protected sealed record OrchestrationMethod
    {
        public string FunctionName { get; }
        public IMethodSymbol OrchestrationMethodSymbol { get; }

        public OrchestrationMethod(string functionName, IMethodSymbol orchestrationMethodSymbol)
        {
            this.FunctionName = functionName;
            this.OrchestrationMethodSymbol = orchestrationMethodSymbol;
        }
    }
}
