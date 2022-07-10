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
        public static RichTextEditorRecord HandleDefaultInsert(RichTextEditorRecord focusedRichTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Default)
            {
                var previousDefaultToken = focusedRichTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

                var content = previousDefaultToken.Content
                    .Insert(previousDefaultToken.IndexInPlainText!.Value + 1, keyDownEventRecord.Key);

                var nextDefaultToken = previousDefaultToken with
                {
                    Content = content,
                    IndexInPlainText = previousDefaultToken.IndexInPlainText + 1
                };
                
                return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, nextDefaultToken);
            }
            else
            {
                var nextTokenTuple = GetNextTokenTuple(focusedRichTextEditorRecord);

                if (nextTokenTuple.rowIndex == focusedRichTextEditorRecord.CurrentRowIndex &&
                    nextTokenTuple.token.Kind == TextTokenKind.Default)
                {
                    focusedRichTextEditorRecord = SetNextTokenAsCurrent(focusedRichTextEditorRecord);
                    
                    var previousDefaultToken = focusedRichTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

                    var content = previousDefaultToken.Content
                        .Insert(0, keyDownEventRecord.Key);

                    var nextDefaultToken = previousDefaultToken with
                    {
                        Content = content,
                        IndexInPlainText = previousDefaultToken.IndexInPlainText
                    };
                    
                    return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, nextDefaultToken);
                }
                else
                {
                    var rememberToken = focusedRichTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>();

                    if (rememberToken.IndexInPlainText!.Value != rememberToken.PlainText.Length - 1)
                    {
                        return SplitCurrentToken(
                            focusedRichTextEditorRecord, 
                            new DefaultTextToken
                            {
                                Content = keyDownEventRecord.Key,
                                IndexInPlainText = 0
                            }
                        );
                    }
                    else
                    {
                        var replacementCurrentToken = focusedRichTextEditorRecord
                            .GetCurrentTextTokenAs<TextTokenBase>() with
                            {
                                IndexInPlainText = null
                            };

                        focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);

                        var defaultTextToken = new DefaultTextToken
                        {
                            Content = keyDownEventRecord.Key,
                            IndexInPlainText = 0
                        };
                        
                        return InsertNewCurrentTokenAfterCurrentPosition(focusedRichTextEditorRecord,
                            defaultTextToken);
                    }
                }
                
            }
        }
        
        public static RichTextEditorRecord HandleDefaultBackspace(RichTextEditorRecord focusedRichTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord)
        {
            var previousDefaultTextToken = focusedRichTextEditorRecord.GetCurrentTextTokenAs<DefaultTextToken>();

            var firstSplitContent = previousDefaultTextToken.Content
                .Substring(0, previousDefaultTextToken.IndexInPlainText!.Value);

            var secondSplitContent = string.Empty;

            if (previousDefaultTextToken.IndexInPlainText != previousDefaultTextToken.Content.Length - 1)
            {
                secondSplitContent = previousDefaultTextToken.Content
                    .Substring(previousDefaultTextToken.IndexInPlainText!.Value + 1);
            }

            var nextDefaultToken = previousDefaultTextToken with
                {
                    Content = firstSplitContent + secondSplitContent,
                    IndexInPlainText = previousDefaultTextToken.IndexInPlainText - 1
                };

            if (nextDefaultToken.Content.Length == 0)
                return RemoveCurrentToken(focusedRichTextEditorRecord);

            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, nextDefaultToken);

            if (nextDefaultToken.IndexInPlainText == -1)
                return SetPreviousTokenAsCurrent(focusedRichTextEditorRecord);

            return focusedRichTextEditorRecord;
        }
    }
}
