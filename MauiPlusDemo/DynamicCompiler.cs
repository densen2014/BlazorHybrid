using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

/// <summary>
/// 代码来动态编译和加载 C# 源代码
/// </summary>
public class DynamicCompiler
{
    /// <summary>
    /// 编译并加载代码
    /// </summary>
    /// <param name="code"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static object? CompileAndLoad(string code, string typeName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create("DynamicAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }

            return null;
        }
        else
        {
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            var type = assembly.GetType(typeName);
            return Activator.CreateInstance(type!);
        }
    }
}
