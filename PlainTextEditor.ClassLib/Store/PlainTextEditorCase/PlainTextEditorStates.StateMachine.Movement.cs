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
        public static PlainTextEditorRecord HandleMovement(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            switch (keyDownEventRecord.Key)
            {
                case KeyboardKeyFacts.MovementKeys.ARROW_LEFT_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_LEFT_KEY:
                    return HandleArrowLeft(focusedPlainTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_DOWN_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_DOWN_KEY:
                    return HandleArrowDown(focusedPlainTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_UP_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_UP_KEY:
                    return HandleArrowUp(focusedPlainTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT_KEY:
                case KeyboardKeyFacts.AlternateMovementKeys.ARROW_RIGHT_KEY:
                    return HandleArrowRight(focusedPlainTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.HOME_KEY:
                    return HandleHome(focusedPlainTextEditorRecord, keyDownEventRecord);
                case KeyboardKeyFacts.MovementKeys.END_KEY:
                    return HandleEnd(focusedPlainTextEditorRecord, keyDownEventRecord);
            }

            return focusedPlainTextEditorRecord;
        }
        
        public static PlainTextEditorRecord HandleArrowLeft(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (keyDownEventRecord.CtrlWasPressed)
            {
                var rememberTokenKey = focusedPlainTextEditorRecord.CurrentTextTokenKey;
                var rememberTokenWasWhitespace = 
                    focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                var targetTokenTuple = GetPreviousTokenTuple(focusedPlainTextEditorRecord);

                while (focusedPlainTextEditorRecord.CurrentTextTokenKey != targetTokenTuple.token.Key)
                {
                    focusedPlainTextEditorRecord = HandleMovement(focusedPlainTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                var currentTokenIsWhitespace = focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                if ((rememberTokenWasWhitespace && currentTokenIsWhitespace) &&
                    (rememberTokenKey != focusedPlainTextEditorRecord.CurrentTextTokenKey))
                {
                    return HandleMovement(focusedPlainTextEditorRecord, keyDownEventRecord);
                }

                return focusedPlainTextEditorRecord;
            }

            var currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            if (currentToken.IndexInPlainText == 0)
            {
                return SetPreviousTokenAsCurrent(focusedPlainTextEditorRecord);
            }
            else
            {
                var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.IndexInPlainText - 1
                };

                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }

            return focusedPlainTextEditorRecord;
        }
        
        public static PlainTextEditorRecord HandleArrowDown(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentRowIndex >= 
                focusedPlainTextEditorRecord.Array.Length - 1)
            {
                return focusedPlainTextEditorRecord;
            }

            var inclusiveStartingColumnIndexOfCurrentToken = 
                CalculateCurrentTokenColumnIndexRespectiveToRow(focusedPlainTextEditorRecord);

            var currentColumnIndexWithIndexInPlainTextAccountedFor = inclusiveStartingColumnIndexOfCurrentToken +
                focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText!.Value;

            var rowBelowKey = focusedPlainTextEditorRecord.Array[focusedPlainTextEditorRecord.CurrentRowIndex + 1];

            var rowBelow = focusedPlainTextEditorRecord.Map[rowBelowKey];

            var tokenInRowBelowTuple = CalculateTokenAtColumnIndexRespectiveToRow(
                focusedPlainTextEditorRecord,
                rowBelow 
                    as PlainTextEditorRow 
                    ?? throw new ApplicationException($"Expected type {nameof(PlainTextEditorRow)}"),
                currentColumnIndexWithIndexInPlainTextAccountedFor);

            while (focusedPlainTextEditorRecord.CurrentTextToken.Key !=
                    tokenInRowBelowTuple.token.Key)
            {
                focusedPlainTextEditorRecord = HandleMovement(focusedPlainTextEditorRecord, new KeyDownEventRecord(
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
                var replacementCurrentToken = focusedPlainTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = currentColumnIndexWithIndexInPlainTextAccountedFor -
                            tokenInRowBelowTuple.inclusiveStartingColumnIndex
                    };

                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }
            else
            {
                var replacementCurrentToken = focusedPlainTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = focusedPlainTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                    };

                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedPlainTextEditorRecord;
        }
        
        public static PlainTextEditorRecord HandleArrowUp(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentRowIndex <= 0)
                return focusedPlainTextEditorRecord;

            var inclusiveStartingColumnIndexOfCurrentToken =
                CalculateCurrentTokenColumnIndexRespectiveToRow(focusedPlainTextEditorRecord);

            var currentColumnIndexWithIndexInPlainTextAccountedFor = inclusiveStartingColumnIndexOfCurrentToken +
                focusedPlainTextEditorRecord.CurrentTextToken
                    .IndexInPlainText!.Value;

            var rowAboveKey = focusedPlainTextEditorRecord.Array[focusedPlainTextEditorRecord.CurrentRowIndex - 1];

            var rowAbove = focusedPlainTextEditorRecord.Map[rowAboveKey];

            var tokenInRowAboveMetaData = CalculateTokenAtColumnIndexRespectiveToRow(
                focusedPlainTextEditorRecord,
                rowAbove
                    as PlainTextEditorRow
                    ?? throw new ApplicationException($"Expected type {nameof(PlainTextEditorRow)}"),
                currentColumnIndexWithIndexInPlainTextAccountedFor);

            while (focusedPlainTextEditorRecord.CurrentTextToken.Key !=
                tokenInRowAboveMetaData.token.Key)
            {
                focusedPlainTextEditorRecord = HandleMovement(focusedPlainTextEditorRecord, new KeyDownEventRecord(
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
                var replacementCurrentToken = focusedPlainTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = currentColumnIndexWithIndexInPlainTextAccountedFor -
                            tokenInRowAboveMetaData.inclusiveStartingColumnIndex
                    };
                    
                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }
            else
            {
                var replacementCurrentToken = focusedPlainTextEditorRecord
                    .GetCurrentTextTokenAs<TextTokenBase>() with
                    {
                        IndexInPlainText = focusedPlainTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                    };
                    
                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedPlainTextEditorRecord;
        }
        
        public static PlainTextEditorRecord HandleArrowRight(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (keyDownEventRecord.CtrlWasPressed)
            {
                var rememberTokenKey = focusedPlainTextEditorRecord.CurrentTextTokenKey;
                var rememberTokenWasWhitespace = 
                    focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                var targetTokenTuple = GetNextTokenTuple(focusedPlainTextEditorRecord);

                while (focusedPlainTextEditorRecord.CurrentTextTokenKey != targetTokenTuple.token.Key)
                {
                    focusedPlainTextEditorRecord = HandleMovement(focusedPlainTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                var currentTokenIsWhitespace = focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Whitespace;

                if ((rememberTokenWasWhitespace && currentTokenIsWhitespace) &&
                    (rememberTokenKey != focusedPlainTextEditorRecord.CurrentTextTokenKey))
                {
                    return HandleMovement(focusedPlainTextEditorRecord, keyDownEventRecord);
                }

                while (focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText != 
                        focusedPlainTextEditorRecord.CurrentTextToken.PlainText.Length - 1)
                {
                    focusedPlainTextEditorRecord = HandleMovement(focusedPlainTextEditorRecord, 
                        keyDownEventRecord with
                        {
                            CtrlWasPressed = false
                        });
                }

                return focusedPlainTextEditorRecord;
            }
            
            var currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            if (currentToken.IndexInPlainText == currentToken.PlainText.Length - 1)
            {
                return SetNextTokenAsCurrent(focusedPlainTextEditorRecord);
            }
            else
            {
                var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.IndexInPlainText + 1
                };

                focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
            }
            
            return focusedPlainTextEditorRecord;
        }
        
        public static PlainTextEditorRecord HandleHome(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            int targetRowIndex = keyDownEventRecord.CtrlWasPressed
                ? 0
                : focusedPlainTextEditorRecord.CurrentRowIndex;

            var currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
    
            focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
            {
                CurrentTokenIndex = 0,
                CurrentRowIndex = targetRowIndex
            };

            currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
        }
        
        public static PlainTextEditorRecord HandleEnd(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            int targetRowIndex = keyDownEventRecord.CtrlWasPressed
                ? focusedPlainTextEditorRecord.Array.Length - 1
                : focusedPlainTextEditorRecord.CurrentRowIndex;

            var currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
    
            var row = focusedPlainTextEditorRecord
                .Map[focusedPlainTextEditorRecord
                    .Array[targetRowIndex]];
    
            focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
            {
                CurrentTokenIndex = row.Array.Length - 1,
                CurrentRowIndex = targetRowIndex
            };

            currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
        }
    }
}
