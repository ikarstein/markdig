// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Parsers;

namespace Markdig.Syntax
{
    /// <summary>
    /// A block quote (Section 5.1 CommonMark specs)
    /// </summary>
    /// <seealso cref="ContainerBlock" />
    public class HintBlock : ContainerBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HintBlock"/> class.
        /// </summary>
        /// <param name="parser">The parser used to create this block.</param>
        public HintBlock(BlockParser parser) : base(parser)
        {
        }

        /// <summary>
        /// Gets or sets the quote character (usually `&gt;`)
        /// </summary>
        public char HintChar { get; set; }
        public int Level { get; set; }
    }
}