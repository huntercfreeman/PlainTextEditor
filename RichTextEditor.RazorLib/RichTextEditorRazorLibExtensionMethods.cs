using Microsoft.Extensions.DependencyInjection;
using RichTextEditor.ClassLib;

namespace RichTextEditor.RazorLib;

public static class RichTextEditorRazorLibExtensionMethods
{
    public static IServiceCollection AddRichTextEditorRazorLibServices(this IServiceCollection services)
    {
        return services
            .AddRichTextEditorClassLibServices();
    }
}
