using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlainTextEditor.ClassLib.Keyboard;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
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
        public static PlainTextEditorRecord MergeTokensIfApplicable(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentTextToken.Kind != TextTokenKind.Default)
                return focusedPlainTextEditorRecord;
            
            var nextTokenTuple = GetNextTokenTuple(focusedPlainTextEditorRecord);

            if (nextTokenTuple.token.Kind != TextTokenKind.Default ||
                nextTokenTuple.token.Key == focusedPlainTextEditorRecord.CurrentTextTokenKey)
            {
                return focusedPlainTextEditorRecord;
            }

            var replacementToken = new DefaultTextToken()
            {
                Content = focusedPlainTextEditorRecord.CurrentTextToken.PlainText +
                    nextTokenTuple.token.PlainText,
                IndexInPlainText = focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText
            };

            var currentRow = focusedPlainTextEditorRecord
                .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

            var replacementRow = currentRow
                .With()
                .Remove(nextTokenTuple.token.Key)
                .Remove(focusedPlainTextEditorRecord.CurrentTextTokenKey)
                .Insert(focusedPlainTextEditorRecord.CurrentTokenIndex, replacementToken)
                .Build();

            var nextRowList = focusedPlainTextEditorRecord.List.Replace(currentRow,
                replacementRow);

            return focusedPlainTextEditorRecord with
            {
                List = nextRowList
            };
        }
    }
}
