using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using RichTextEditor.ClassLib.Keyboard;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;

namespace RichTextEditor.ClassLib.Store.KeyDownEventCase;

public record KeyDownEventAction(RichTextEditorKey FocusedRichTextEditorKey, KeyDownEventRecord KeyDownEventRecord);
