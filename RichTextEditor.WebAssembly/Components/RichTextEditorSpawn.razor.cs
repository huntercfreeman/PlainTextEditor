using Microsoft.AspNetCore.Components;
using RichTextEditor.ClassLib.Services;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;

namespace RichTextEditor.WebAssembly.Components;

public partial class RichTextEditorSpawn : ComponentBase, IDisposable
{
    [Inject]
    private IRichTextEditorService RichTextEditorService { get; set; } = null!;

    private RichTextEditorKey _richTextEditorKey = RichTextEditorKey.NewRichTextEditorKey();
    private bool _richTextEditorWasInitialized;
    
    protected override void OnInitialized()
    {
        _ = Task.Run(async () => 
            {
                await RichTextEditorService
                    .ConstructRichTextEditorAsync(_richTextEditorKey, 
                        async () => 
                        {
                            _richTextEditorWasInitialized = true;
                            await InvokeAsync(StateHasChanged);
                        });
            });

        base.OnInitialized();
    }

    public void Dispose()
    {
        RichTextEditorService.DeconstructRichTextEditor(_richTextEditorKey);
    }
}