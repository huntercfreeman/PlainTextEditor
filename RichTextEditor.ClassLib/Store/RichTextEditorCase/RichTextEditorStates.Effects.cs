using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using RichTextEditor.ClassLib.Keyboard;
using RichTextEditor.ClassLib.Store.KeyDownEventCase;
using RichTextEditor.ClassLib.WebAssemblyFix;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    public class RichTextEditorStatesEffects
    {
        private int _counter = 0;

        [EffectMethod]
        public async Task HandleWebAssemblyFixDelayAction(WebAssemblyFixDelayAction webAssemblyFixDelayAction,
            IDispatcher dispatcher)
        {
            Console.WriteLine($"HandleWebAssemblyFixDelayAction {_counter++}");
            await Task.Delay(1);

            dispatcher.Dispatch(webAssemblyFixDelayAction.Action);
        }
    }
}

