using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RichTextEditor.RazorLib;

public partial class RichTextEditorInitializer : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JsRuntime.InvokeVoidAsync("richTextEditor.initializeIntersectionObserver");
        }

        return base.OnAfterRenderAsync(firstRender);
    }
    
    public void Dispose()
    {
        JsRuntime.InvokeVoidAsync("richTextEditor.disposeIntersectionObserver");
    }
}
