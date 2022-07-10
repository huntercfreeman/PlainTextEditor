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
        public static RichTextEditorRecord HandleMovement(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            switch (keyDownEventRecord.Key)
            {
                case KeyboardKeyFacts.MovementKeys.ARROW_LEFT_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_LEFT_KEY:
                    return HandleArrowLeft(focusedRichTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_DOWN_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_DOWN_KEY:
                    return HandleArrowDown(focusedRichTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_UP_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_UP_KEY:
                    return HandleArrowUp(focusedRichTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_RIGHT_KEY:
                    return HandleArrowRight(focusedRichTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.HOME_KEY:
                    return HandleHome(focusedRichTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.END_KEY:
                    return HandleEnd(focusedRichTextEditorRecord, keyDownEventRecord);
            }

            return focusedRichTextEditorRecord;
        }
        
        public static RichTextEditorRecord HandleArrowLeft(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (keyDownEventRecord.CtrlWasPressed)
            {
                var rememberTokenKey = focusedRichTextEditorRecord.CurrentTextTokenKey;
                var rememberTokenWasWhitespace = 
                    focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                var targetTokenTuple = GetPreviousTokenTuple(focusedRichTextEditorRecord);

                while (focusedRichTextEditorRecord.CurrentTextTokenKey != targetTokenTuple.token.Key)
                {
                    focusedRichTextEditorRecord = HandleMovement(focusedRichTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                var currentTokenIsWhitespace = focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                if ((rememberTokenWasWhitespace && currentTokenIsWhitespace) &&
                    (rememberTokenKey != focusedRichTextEditorRecord.CurrentTextTokenKey))
                {
                    return HandleMovement(focusedRichTextEditorRecord, keyDownEventRecord);
                }

                return focusedRichTextEditorRecord;
            }

            var currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            if (currentToken.IndexInPlainText == 0)
            {
                return SetPreviousTokenAsCurrent(focusedRichTextEditorRecord);
            }
            else
            {
                var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.IndexInPlainText - 1
                };

                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }

            return focusedRichTextEditorRecord;
        }
        
        public static RichTextEditorRecord HandleArrowDown(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedRichTextEditorRecord.CurrentRowIndex >= 
                focusedRichTextEditorRecord.Array.Length - 1)
            {
                return focusedRichTextEditorRecord;
            }

            var inclusiveStartingColumnIndexOfCurrentToken = 
                CalculateCurrentTokenColumnIndexRespectiveToRow(focusedRichTextEditorRecord);

            var currentColumnIndexWithIndexInPlainTextAccountedFor = inclusiveStartingColumnIndexOfCurrentToken +
                focusedRichTextEditorRecord.CurrentTextToken.IndexInPlainText!.Value;

            var rowBelowKey = focusedRichTextEditorRecord.Array[focusedRichTextEditorRecord.CurrentRowIndex + 1];

            var rowBelow = focusedRichTextEditorRecord.Map[rowBelowKey];

            var tokenInRowBelowTuple = CalculateTokenAtColumnIndexRespectiveToRow(
                focusedRichTextEditorRecord,
                rowBelow 
                    as RichTextEditorRow 
                    ?? throw new ApplicationException($"Expected type {nameof(RichTextEditorRow)}"),
                currentColumnIndexWithIndexInPlainTextAccountedFor);

            while (focusedRichTextEditorRecord.CurrentTextToken.Key !=
                    tokenInRowBelowTuple.token.Key)
            {
                focusedRichTextEditorRecord = HandleMovement(focusedRichTextEditorRecord, new KeyDownEventRecord(
                    KeyboardKeyFacts.MovementKeys.ARROW_RIGHT_KEY,
                    KeyboardKeyFacts.MovementKeys.ARROW_RIGHT_KEY,
                    false,
                    keyDownEventRecord.ShiftWasPressed,
                    false
                ));
            }

            if (currentColumnIndexWithIndexInPlainTextAccountedFor <
                tokenInRowBelowTuple.exclusiveEndingColumnIndex)
            {
                var replacementCurrentToken = focusedRichTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = currentColumnIndexWithIndexInPlainTextAccountedFor -
                            tokenInRowBelowTuple.inclusiveStartingColumnIndex
                    };

                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }
            else
            {
                var replacementCurrentToken = focusedRichTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = focusedRichTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                    };

                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedRichTextEditorRecord;
        }
        
        public static RichTextEditorRecord HandleArrowUp(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedRichTextEditorRecord.CurrentRowIndex <= 0)
                return focusedRichTextEditorRecord;

            var inclusiveStartingColumnIndexOfCurrentToken =
                CalculateCurrentTokenColumnIndexRespectiveToRow(focusedRichTextEditorRecord);

            var currentColumnIndexWithIndexInPlainTextAccountedFor = inclusiveStartingColumnIndexOfCurrentToken +
                focusedRichTextEditorRecord.CurrentTextToken
                    .IndexInPlainText!.Value;

            var rowAboveKey = focusedRichTextEditorRecord.Array[focusedRichTextEditorRecord.CurrentRowIndex - 1];

            var rowAbove = focusedRichTextEditorRecord.Map[rowAboveKey];

            var tokenInRowAboveMetaData = CalculateTokenAtColumnIndexRespectiveToRow(
                focusedRichTextEditorRecord,
                rowAbove
                    as RichTextEditorRow
                    ?? throw new ApplicationException($"Expected type {nameof(RichTextEditorRow)}"),
                currentColumnIndexWithIndexInPlainTextAccountedFor);

            while (focusedRichTextEditorRecord.CurrentTextToken.Key !=
                tokenInRowAboveMetaData.token.Key)
            {
                focusedRichTextEditorRecord = HandleMovement(focusedRichTextEditorRecord, new KeyDownEventRecord(
                    KeyboardKeyFacts.MovementKeys.ARROW_LEFT_KEY,
                    KeyboardKeyFacts.MovementKeys.ARROW_LEFT_KEY,
                    false,
                    keyDownEventRecord.ShiftWasPressed,
                    false
                ));
            }

            if (currentColumnIndexWithIndexInPlainTextAccountedFor <
                tokenInRowAboveMetaData.exclusiveEndingColumnIndex)
            {
                var replacementCurrentToken = focusedRichTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = currentColumnIndexWithIndexInPlainTextAccountedFor -
                            tokenInRowAboveMetaData.inclusiveStartingColumnIndex
                    };
                    
                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }
            else
            {
                var replacementCurrentToken = focusedRichTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = focusedRichTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                    };
                    
                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedRichTextEditorRecord;
        }
        
        public static RichTextEditorRecord HandleArrowRight(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (keyDownEventRecord.CtrlWasPressed)
            {
                var rememberTokenKey = focusedRichTextEditorRecord.CurrentTextTokenKey;
                var rememberTokenWasWhitespace = 
                    focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                var targetTokenTuple = GetNextTokenTuple(focusedRichTextEditorRecord);

                while (focusedRichTextEditorRecord.CurrentTextTokenKey != targetTokenTuple.token.Key)
                {
                    focusedRichTextEditorRecord = HandleMovement(focusedRichTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                var currentTokenIsWhitespace = focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                if ((rememberTokenWasWhitespace && currentTokenIsWhitespace) &&
                    (rememberTokenKey != focusedRichTextEditorRecord.CurrentTextTokenKey))
                {
                    return HandleMovement(focusedRichTextEditorRecord, keyDownEventRecord);
                }

                while (focusedRichTextEditorRecord.CurrentTextToken.IndexInPlainText != 
                        focusedRichTextEditorRecord.CurrentTextToken.PlainText.Length - 1)
                {
                    focusedRichTextEditorRecord = HandleMovement(focusedRichTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                return focusedRichTextEditorRecord;
            }
            
            var currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            if (currentToken.IndexInPlainText == currentToken.PlainText.Length - 1)
            {
                return SetNextTokenAsCurrent(focusedRichTextEditorRecord);
            }
            else
            {
                var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.IndexInPlainText + 1
                };

                focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedRichTextEditorRecord;
        }
        
        // Note originally I thought I had to 'walk' the cursor the entire distance
        // inorder to highlight any text traveled over.
        //
        // When doing highlighting instead keep track of the before 
        // movement document index (the character index within the document itself)
        // and the document index of the ending position. This is not the exact word
        // for word solution but just a general reminder of the idea.
        public static RichTextEditorRecord HandleHome(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            int targetRowIndex = keyDownEventRecord.CtrlWasPressed
                ? 0
                : focusedRichTextEditorRecord.CurrentRowIndex;

            var currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
    
            focusedRichTextEditorRecord = focusedRichTextEditorRecord with
            {
                CurrentTokenIndex = 0,
                CurrentRowIndex = targetRowIndex
            };

            currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
        }
        
        public static RichTextEditorRecord HandleEnd(RichTextEditorRecord focusedRichTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            int targetRowIndex = keyDownEventRecord.CtrlWasPressed
                ? focusedRichTextEditorRecord.Array.Length - 1
                : focusedRichTextEditorRecord.CurrentRowIndex;

            var currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
    
            var row = focusedRichTextEditorRecord
                .Map[focusedRichTextEditorRecord
                    .Array[targetRowIndex]];
    
            focusedRichTextEditorRecord = focusedRichTextEditorRecord with
            {
                CurrentTokenIndex = row.Array.Length - 1,
                CurrentRowIndex = targetRowIndex
            };

            currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
        }
    }
}
