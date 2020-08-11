// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Syntax;

namespace Markdig.Renderers.Html
{
    /// <summary>
    /// A HTML renderer for a <see cref="QuoteBlock"/>.
    /// </summary>
    /// <seealso cref="HtmlObjectRenderer{QuoteBlock}" />
    public class HintBlockRenderer : HtmlObjectRenderer<HintBlock>
    {
        protected override void Write(HtmlRenderer renderer, HintBlock obj)
        {
            renderer.EnsureLine();
            if (renderer.EnableHtmlForBlock)
            {
                renderer.Write($"<div class='hint hintlvl{obj.Level}' data-level='{obj.Level}'").WriteAttributes(obj).WriteLine(">");
            }
            var savedImplicitParagraph = renderer.ImplicitParagraph;
            renderer.ImplicitParagraph = false;
            renderer.WriteChildren(obj);
            renderer.ImplicitParagraph = savedImplicitParagraph;
            if (renderer.EnableHtmlForBlock)
            {
                renderer.WriteLine("</div>");
            }
            renderer.EnsureLine();
        }
    }
}