using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;

namespace RichTextEditor.RazorLib.RichTextEditorCase;

public partial class TextTokenDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public ITextToken TextToken { get; set; } = null!;
}
