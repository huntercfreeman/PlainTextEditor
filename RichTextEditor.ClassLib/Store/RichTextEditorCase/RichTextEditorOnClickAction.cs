namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public record RichTextEditorOnClickAction(RichTextEditorKey FocusedRichTextEditorKey,
    int RowIndex,
    int TokenIndex,
    int? CharacterIndex);
