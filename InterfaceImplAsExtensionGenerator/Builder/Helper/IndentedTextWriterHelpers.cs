using System.CodeDom.Compiler;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

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

        /// <summary>
        /// 创建一个新的代码块，写入一个左括号，并增加缩进。需要配合 <see cref="AppendCloseBracket"/> 使用，以确保括号正确闭合。
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AppendOpenBracket()
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));

            writer.WriteLine("{");
            writer.Indent++;
        }

        /// <summary>
        /// 创建一个新的代码块，写入一个左括号，并增加缩进。创建一个回调，在<see cref="DeferredActionScope"/>结束时调用，以确保括号正确闭合。
        /// </summary>
        /// <param name="deferredActionScope">用于延迟执行关闭括号操作的作用域。</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AppendOpenBracket(DeferredActionScope deferredActionScope)
        {
            _ = writer ?? throw new System.ArgumentNullException(nameof(writer));
            _ = deferredActionScope ?? throw new System.ArgumentNullException(nameof(deferredActionScope));
            writer.WriteLine("{");
            writer.Indent++;
            deferredActionScope.Defer(writer.AppendCloseBracket);
        }

        /// <summary>
        /// 结束当前代码块，写入一个右括号，并减少缩进。需要配合 <see cref="AppendOpenBracket(IndentedTextWriter)"/> 使用，以确保括号正确闭合。
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
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