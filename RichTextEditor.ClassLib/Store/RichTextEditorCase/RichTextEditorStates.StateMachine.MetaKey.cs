using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RichTextEditor.ClassLib.Keyboard;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    private partial class StateMachine
    {
        public static RichTextEditorRecord HandleMetaKey(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            switch (keyDownEventRecord.Key)
            {
                case KeyboardKeyFacts.MetaKeys.BACKSPACE_KEY:
                    return HandleBackspaceKey(focusedRichTextEditorRecord, keyDownEventRecord);
                default:
                   return focusedRichTextEditorRecord;
            }
        }

        public static RichTextEditorRecord HandleBackspaceKey(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Default)
            {
                return HandleDefaultBackspace(focusedRichTextEditorRecord, keyDownEventRecord);
            }

            if (focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.StartOfRow &&
                focusedRichTextEditorRecord.GetCurrentRichTextEditorRowAs<RichTextEditorRow>().Array.Length > 1)
            {
                focusedRichTextEditorRecord = MoveCurrentRowToEndOfPreviousRow(focusedRichTextEditorRecord);
            }
            else
            {
                focusedRichTextEditorRecord = RemoveCurrentToken(focusedRichTextEditorRecord);
            }

            focusedRichTextEditorRecord = MergeTokensIfApplicable(focusedRichTextEditorRecord);

            return focusedRichTextEditorRecord;
        }
    }
}
