namespace zms9110750.StaticMethodAsExtensionGenerator.Builder.Helper;

static class IndentedTextWriterHelpers
{
    extension(IndentedTextWriter writer)
    {
        public void WriteLine(int offset, string value)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));

            writer.Indent += offset;
            writer.WriteLine(value);
            writer.Indent -= offset;
        }

        public void WriteSources(string code)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));
            _ = code ?? throw new System.ArgumentNullException(nameof(code));
            foreach (var line in code.Split('\n'))
            {
                switch (line.Trim())
                {
                    case "{":
                        writer.AppendOpenBracket();
                        break;
                    case "}":
                        writer.AppendCloseBracket();
                        break;
                    default:
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            writer.WriteLine(line.Trim());
                        }
                        break;
                }
            }
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));
            _ = lines ?? throw new System.ArgumentNullException(nameof(lines));

            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
        public void WriteJoin(string separator, IEnumerable<string> lines)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));
            _ = separator ?? throw new System.ArgumentNullException(nameof(separator));
            _ = lines ?? throw new System.ArgumentNullException(nameof(lines));

            var enumerator = lines.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }
            writer.Write(enumerator.Current);

            while (enumerator.MoveNext())
            {
                writer.Write(separator);
                writer.Write(enumerator.Current);
            }
        }

        public void AppendOpenBracket()
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));

            writer.WriteLine("{");
            writer.Indent++;
        }

        public void AppendOpenBracket(DeferredActionScope deferredActionScope)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));
            _ = deferredActionScope ?? throw new System.ArgumentNullException(nameof(deferredActionScope));
            writer.WriteLine("{");
            writer.Indent++;
            deferredActionScope.Defer(writer.AppendCloseBracket);
        }

        public void AppendCloseBracket()
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));

            writer.Indent--;
            writer.WriteLine("}");
        }

        public void UnwindOpenedBrackets()
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));

            while (writer.Indent != 0)
            {
                AppendCloseBracket(writer);
            }
        }
    }
}
