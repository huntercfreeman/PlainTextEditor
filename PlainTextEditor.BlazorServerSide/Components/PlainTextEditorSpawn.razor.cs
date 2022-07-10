using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PlainTextEditor.ClassLib.Services;
using PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

namespace PlainTextEditor.BlazorServerSide.Components;

public partial class PlainTextEditorSpawn : ComponentBase, IDisposable
{
    [Inject]
    private IPlainTextEditorService PlainTextEditorService { get; set; } = null!;

    private PlainTextEditorKey _plainTextEditorKey = PlainTextEditorKey.NewPlainTextEditorKey();
    private bool _plainTextEditorWasInitialized;
    
    protected override void OnInitialized()
    {
        _ = Task.Run(async () => 
            {
                await PlainTextEditorService
                    .ConstructPlainTextEditorAsync(_plainTextEditorKey, 
                        async () => 
                        {
                            _plainTextEditorWasInitialized = true;
                            await InvokeAsync(StateHasChanged);
                        });
            });

        base.OnInitialized();
    }

    public void Dispose()
    {
        PlainTextEditorService.DeconstructPlainTextEditor(_plainTextEditorKey);
    }
}