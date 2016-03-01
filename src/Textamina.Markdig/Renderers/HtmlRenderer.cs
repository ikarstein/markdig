using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Textamina.Markdig.Helpers;
using Textamina.Markdig.Renderers.Html;
using Textamina.Markdig.Renderers.Html.Inlines;
using Textamina.Markdig.Syntax;
using Textamina.Markdig.Syntax.Inlines;

namespace Textamina.Markdig.Renderers
{
    public class HtmlRenderer : RendererBase
    {
        // TODO: Move this code to HTMLHelper
        private const string HexCharacters = "0123456789ABCDEF";
        private static readonly char[] EscapeHtmlCharacters = { '&', '<', '>', '"' };
        private static readonly bool[] UrlSafeCharacters =
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false,
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false,
            false, true, false, true, true, true, false, false, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true, true, true, false, true, false, true,
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, true,
            false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false
        };

        private readonly TextWriter textWriter;
        private bool previousWasLine;
        private char[] buffer;

        public HtmlRenderer(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));
            this.textWriter = textWriter;

            buffer = new char[1024];

            // Default block renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new HardlineBreakInlineRenderer());
            ObjectRenderers.Add(new SoftlineBreakInlineRenderer());
            ObjectRenderers.Add(new HtmlInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());

            EnableHtmlForInline = true;
            // We assume that we are starting as if we had previously a newline
            previousWasLine = true;
        }

        public bool EnableHtmlForInline { get; set; }

        public bool ImplicitParagraph { get; set; }

        public HtmlRenderer EnsureLine()
        {
            if (!previousWasLine)
            {
                WriteLine();
            }
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer Write(string content)
        {
            previousWasLine = false;
            textWriter.Write(content);
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer Write(ref StringSlice slice)
        {
            if (slice.Start > slice.End)
            {
                return this;
            }
            return Write(slice.Text, slice.Start, slice.Length);
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer Write(char content)
        {
            previousWasLine = content == '\n';
            textWriter.Write(content);
            return this;
        }

        public HtmlRenderer Write(string content, int offset, int length)
        {
            previousWasLine = false;
            if (offset == 0 && content.Length == length)
            {
                textWriter.Write(content);
            }
            else
            {
                if (length > buffer.Length)
                {
                    buffer = content.ToCharArray(offset, length);
                }
                else
                {
                    content.CopyTo(offset, buffer, 0, length);
                }

                textWriter.Write(buffer, 0, length);
            }
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer WriteLine()
        {
            textWriter.WriteLine();
            previousWasLine = true;
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer WriteLine(string content)
        {
            previousWasLine = true;
            textWriter.WriteLine(content);
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer WriteEscape(string content)
        {
            if (string.IsNullOrEmpty(content))
                return this;

            WriteEscape(content, 0, content.Length);
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer WriteEscape(ref StringSlice slice)
        {
            if (slice.Start > slice.End)
            {
                return this;
            }
            return WriteEscape(slice.Text, slice.Start, slice.Length);
        }

        public HtmlRenderer WriteEscape(string content, int offset, int length)
        {
            previousWasLine = false;
            if (string.IsNullOrEmpty(content) || length == 0)
                return this;

            var end = offset + length;
            int previousOffset = offset;
            for (;offset < end;  offset++)
            {
                switch (content[offset])
                {
                    case '<':
                        Write(content, previousOffset, offset - previousOffset);
                        Write("&lt;");
                        previousOffset = offset + 1;
                        break;
                    case '>':
                        Write(content, previousOffset, offset - previousOffset);
                        Write("&gt;");
                        previousOffset = offset + 1;
                        break;
                    case '&':
                        Write(content, previousOffset, offset - previousOffset);
                        Write("&amp;");
                        previousOffset = offset + 1;
                        break;
                    case '"':
                        Write(content, previousOffset, offset - previousOffset);
                        Write("&quot;");
                        previousOffset = offset + 1;
                        break;
                }
            }

            Write(content, previousOffset, end - previousOffset);
            return this;
        }

        public HtmlRenderer WriteEscapeUrl(string content)
        {
            if (content == null)
                return this;

            int previousPosition = 0;
            int len = content.Length;

            for (var i = 0; i < len; i++)
            {
                var c = content[i];

                if (c == '&')
                {
                    Write(content, previousPosition, i - previousPosition);
                    previousPosition = i + 1;
                    Write("&amp;");
                }
                else if (c < 128 && !UrlSafeCharacters[c])
                {
                    Write(content, previousPosition, i - previousPosition);
                    previousPosition = i + 1;

                    Write("%").Write(HexCharacters[c / 16]).Write(HexCharacters[c % 16]);
                }
                else if (c > 127)
                {
                    Write(content, previousPosition, i - previousPosition);
                    previousPosition = i + 1;

                    byte[] bytes;
                    if (c >= '\ud800' && c <= '\udfff' && len != previousPosition)
                    {
                        bytes = Encoding.UTF8.GetBytes(new[] { c, content[previousPosition] });
                        previousPosition = ++i + 1;
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes(new[] { c });
                    }

                    for (var j = 0; j < bytes.Length; j++)
                    {
                        Write('%').Write(HexCharacters[bytes[j] / 16]).Write(HexCharacters[bytes[j] % 16]);
                    }
                }
            }

            Write(content, previousPosition, len - previousPosition);
            return this;
        }

        [MethodImpl(MethodImplOptionPortable.AggressiveInlining)]
        public HtmlRenderer WriteLeafInline(LeafBlock leafBlock)
        {
            var inline = (Inline)leafBlock.Inline;
            if (inline != null)
            {
                while (inline != null)
                {
                    Write(inline);
                    inline = inline.NextSibling;
                }
            }
            return this;
        }

        public HtmlRenderer WriteLeafRawLines(LeafBlock leafBlock, bool writeEndOfLines, bool escape)
        {
            if (leafBlock.Lines != null)
            {
                var lines = leafBlock.Lines;
                var slices = lines.Lines;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (!writeEndOfLines && i > 0)
                    {
                        WriteLine();
                    }
                    if (escape)
                    {
                        WriteEscape(ref slices[i].Slice);
                    }
                    else
                    {
                        Write(ref slices[i].Slice);
                    }
                    if (writeEndOfLines)
                    {
                        WriteLine();
                    }
                }
            }
            return this;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return textWriter;
        }
    }
}