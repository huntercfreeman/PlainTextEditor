using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RichTextEditor.ClassLib.Services;

namespace RichTextEditor.ClassLib;

public static class RichTextEditorClassLibExtensionMethods
{
    public static IServiceCollection AddRichTextEditorClassLibServices(this IServiceCollection services)
    {
        return services
            .AddFluxor(options => options
                .ScanAssemblies(typeof(RichTextEditorClassLibExtensionMethods).Assembly))
            .AddRichTextEditorService();
    }
    
    private static IServiceCollection AddRichTextEditorService(this IServiceCollection services)
    {
        return services.AddScoped<IRichTextEditorService, RichTextEditorService>();
    }
}
