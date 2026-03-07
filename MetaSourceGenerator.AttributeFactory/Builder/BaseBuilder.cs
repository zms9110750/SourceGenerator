using System.CodeDom.Compiler;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder;

abstract class BaseBuilder(IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic)
{
    public IndentedTextWriter Writer { get; } = writer ?? throw new ArgumentNullException(nameof(writer));
    public Action<Diagnostic> ReportDiagnostic { get; } = reportDiagnostic ?? throw new ArgumentNullException(nameof(reportDiagnostic));
}
