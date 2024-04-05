// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
//using Microsoft.Azure.Functions.Worker;
using Microsoft.CodeAnalysis;

namespace Microsoft.DurableTask.Analyzers.Helpers;

sealed class KnownTypeSymbols(Compilation compilation)
{
    readonly Compilation compilation = compilation;

    Cached<INamedTypeSymbol?> orchestrationTriggerAttribute;
    public INamedTypeSymbol? OrchestrationTriggerAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.OrchestrationTriggerAttribute", ref this.orchestrationTriggerAttribute);

    Cached<INamedTypeSymbol?> functionAttribute;
    public INamedTypeSymbol? FunctionAttribute => this.GetOrResolveFullyQualifiedType("Microsoft.Azure.Functions.Worker.FunctionAttribute", ref this.functionAttribute);


    INamedTypeSymbol ? GetOrResolveType(Type type, ref Cached<INamedTypeSymbol?> field)
    {
        return this.GetOrResolveFullyQualifiedType(type.FullName, ref field);
    }

    INamedTypeSymbol? GetOrResolveFullyQualifiedType(string fullyQualifiedName, ref Cached<INamedTypeSymbol?> field)
    {
        if (field.HasValue)
        {
            return field.Value;
        }

        INamedTypeSymbol? type = this.compilation.GetTypeByMetadataName(fullyQualifiedName);
        field = new(type);
        return type;
    }

    // We could use Lazy<T> here, but because we need to use the `compilation` variable instance,
    // that would require us to initiate the Lazy<T> lambdas in the constructor.
    // Because not all analyzers use all symbols, we would be allocating unnecessary lambdas.
    readonly struct Cached<T>(T value)
    {
        public readonly bool HasValue = true;
        public readonly T Value = value;
    }
}
