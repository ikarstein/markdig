// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System.Net.Security;
using System.Net.Sockets;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers
{
    /// <summary>
    /// A block parser for a <see cref="HintBlock"/>.
    /// </summary>
    /// <seealso cref="BlockParser" />
    public class HintBlockParser : BlockParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteBlockParser"/> class.
        /// </summary>
        public HintBlockParser()
        {
            OpeningCharacters = new[] {'!'};
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            var line = processor.Line;
            char openingChar = line.CurrentChar;
            int count = line.CountAndSkipChar(openingChar);

            if (!processor.PeekChar(count).IsSpaceOrTab())
                return BlockState.None;

            var column = processor.Column;
            var sourcePosition = processor.Start;

            // 5.1 Block quotes 
            // A block quote marker consists of 0-3 spaces of initial indent, plus (a) the character > together with a following space, or (b) a single character > not followed by a space.
            var quoteChar = processor.CurrentChar;

            processor.NewBlocks.Push(new HintBlock(this)
            {
                HintChar = quoteChar,
                Level = count,
                Column = column,
                Span = new SourceSpan(sourcePosition, processor.Line.End)
            });

            processor.GoToColumn(column + count + 1);

            return BlockState.Continue;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            var quote = (HintBlock)block;
            int count = quote.Level;
            char matchChar = quote.HintChar;

            int column = processor.Column;
            var line = processor.Line;
            int startPosition = line.Start;
            count -= line.CountAndSkipChar(matchChar);

            if (count == 0 && !processor.IsCodeIndent)
            {
                line.TrimStart();
                quote.UpdateSpanEnd(line.End);
                processor.GoToColumn(column + quote.Level + 1);
                return BlockState.Continue;
            }
            else
            {
                processor.Close(quote);
                processor.GoToColumn(processor.ColumnBeforeIndent);
                return BlockState.Break;
            }
        }
    }
}