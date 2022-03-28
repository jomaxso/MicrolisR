﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MicrolisR.Data.SourceGenerator;

[Generator]
public class MappableGenerator : IIncrementalGenerator
{
    private const string MappableAttribute = "MicrolisR.Data.Mapping.MappableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) =>
                    IsSyntaxTargetFromGeneration(syntaxNode),
                transform: static (ctx, _) =>
                    GetSemanticTargetForGeneration(ctx))
            .Where(static classDeclaration => classDeclaration is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetFromGeneration(SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax {AttributeLists: {Count: > 0}};

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;

        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;

                if (attributeSymbol is null)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == MappableAttribute)
                    return classDeclarationSyntax;
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        var distinctClasses = classes.Distinct();
        
        var parser = new Parser(compilation, context.ReportDiagnostic, context.CancellationToken);
        IReadOnlyList<MappableClass> mappableClasses = parser.GetMappableClasses(distinctClasses);

        if (mappableClasses.Count <= 0)
            return;

        var emitter = new Emitter();

        foreach (var mappableClass in mappableClasses)
        {
            var result = emitter.Emit(mappableClass, context.CancellationToken);
            context.AddSource($"{mappableClass.FullName}.Mappers.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }
}