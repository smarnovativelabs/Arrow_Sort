# Avatar editor

Restyled the existing six-avatar editor to match the Make It Yours reference. Separate ProfileEditor atlas sprites provide the cream/gold card, title banner, name plate, captioned avatar cards, selection ribbon, green save button and red close button. Avatar images stay separate and editable. Centered RectTransform pivots are required by WoolRoundedClip. Round name pencil uses WoolNamePencilGraphic and focuses TMP input. Date presentation and date authoring are removed; any legacy date object is hidden.

WoolProgress.MaxPlayerNameLength=15 is the shared input/save cap. TMP characterLimit intentionally stays zero so the validation callback can reject the 16th character and show Maximum 15 characters allowed. Value-change handling also caps pasted/programmatic input. Warning clears when deleting below the limit or reopening the editor. User ID is immutable and separate from entered names. Existing 20-digit ID fallback opens an empty name draft.

Validation in isolated preview: 16 typed characters accepted15 with warning; paste20 truncated15 with warning; deletion clears warning; exactly15 saves; direct save API caps15; avatar selection saves with stable ID; pencil focuses on next Unity frame. Corrected native capture inspected. No Android build. Backups: Docs/Backups/AvatarEditor-20261022.
