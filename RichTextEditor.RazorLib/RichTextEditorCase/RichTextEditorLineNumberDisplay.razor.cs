using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace RichTextEditor.RazorLib.RichTextEditorCase;

public partial class RichTextEditorLineNumberDisplay : ComponentBase
{
    [CascadingParameter(Name="RowIndex")]
    public int RowIndex { get; set; }

    [Parameter, EditorRequired]
    public int MostDigitsInARowNumber { get; set; }

    private int CountOfDigitsInRowNumber => (RowIndex + 1).ToString().Length;
    private string WidthStyleCss => $"width: {MostDigitsInARowNumber}ch;";
    private string PaddingLeftStyleCss => $"padding-left: {MostDigitsInARowNumber - CountOfDigitsInRowNumber}ch;";
}
