using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotUML.CLI.Diagram;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace DotUML.CLI.Analyzers;

public static class TypeSyntaxAnalyzer
{

    public static Diagram.TypeInfo GetTypeInfo(TypeSyntax typeSyntax)
    {
        var isPrimitive = typeSyntax is PredefinedTypeSyntax predefinedType && (
                          predefinedType.Keyword.IsKind(SyntaxKind.IntKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.BoolKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.DoubleKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.FloatKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.CharKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.ByteKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.DecimalKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.LongKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.ShortKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.UIntKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.ULongKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.UShortKeyword) ||
                          predefinedType.Keyword.IsKind(SyntaxKind.SByteKeyword));
        if (isPrimitive)
        {
            return new PrimitiveType(typeSyntax.ToString());
        }
        if (typeSyntax is ArrayTypeSyntax array)
        {
            return new AggregateType(typeSyntax.ToString(), GetTypeInfo(array.ElementType));
        }

        if (typeSyntax is NullableTypeSyntax nullableType)
        {
            return new NullableType(nullableType.ToString(), GetTypeInfo(nullableType.ElementType));
        }
        if (typeSyntax is GenericNameSyntax genericName &&
                           (genericName.Identifier.Text == "IEnumerable" ||
                            genericName.Identifier.Text == "List" ||
                            genericName.Identifier.Text == "ICollection" ||
                            genericName.Identifier.Text == "IList" ||
                            genericName.Identifier.Text == "HashSet"))
        {
            var type = GetTypeInfo(genericName.TypeArgumentList.Arguments.First());
            return new AggregateType(typeSyntax.ToString(), type);
        }
        return new CreatedType(typeSyntax.ToString());
    }
}
