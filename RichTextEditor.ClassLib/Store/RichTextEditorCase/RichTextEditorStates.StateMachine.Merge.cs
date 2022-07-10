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
        // Given:
        //
        // 'z Bill'
        //   ^
        //    (Remove Whitespace)
        //
        // tokens: 'z' and 'Bill' must be
        // merged to make the token: 'zBill'
        public static RichTextEditorRecord MergeTokensIfApplicable(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            if (focusedRichTextEditorRecord.CurrentTextToken.Kind != TextTokenKind.Default)
                return focusedRichTextEditorRecord;
            
            var nextTokenTuple = GetNextTokenTuple(focusedRichTextEditorRecord);

            if (nextTokenTuple.token.Kind != TextTokenKind.Default ||
                nextTokenTuple.token.Key == focusedRichTextEditorRecord.CurrentTextTokenKey)
            {
                return focusedRichTextEditorRecord;
            }

            var replacementToken = new DefaultTextToken()
            {
                Content = focusedRichTextEditorRecord.CurrentTextToken.PlainText +
                    nextTokenTuple.token.PlainText,
                IndexInPlainText = focusedRichTextEditorRecord.CurrentTextToken.IndexInPlainText
            };

            var currentRow = focusedRichTextEditorRecord
                .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

            var replacementRow = currentRow
                .With()
                .Remove(nextTokenTuple.token.Key)
                .Remove(focusedRichTextEditorRecord.CurrentTextTokenKey)
                .Insert(focusedRichTextEditorRecord.CurrentTokenIndex, replacementToken)
                .Build();

            var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                focusedRichTextEditorRecord.Map
            );

            nextRowMap[replacementRow.Key] = replacementRow;

            return focusedRichTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary()
            };
        }
    }
}
