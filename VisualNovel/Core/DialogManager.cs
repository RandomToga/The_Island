using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace The_Island.Core
{
    public class DialogOption
    {
        public string Text { get; set; }
        public string NextDialog { get; set; }
    }

    public class DialogLineData
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string CharacterImage { get; set; }
        public string BackgroundImage { get; set; }
        public string NextDialog { get; set; }
        public List<DialogOption> Options { get; set; } = new();
    }

    public class DialogData
    {
        public string Id { get; set; }
        public List<DialogLineData> Lines { get; set; } = new();
    }

    public class DialogCollection
    {
        public List<DialogData> Dialogs { get; set; } = new();
    }

    public class DialogManager
    {
        private Dictionary<string, DialogData> _dialogs = new();
        private DialogData _currentDialog;
        private int _currentLineIndex;

        public event Action OnDialogEnd;

        public void LoadDialogs(string json)
        {
            try
            {
                var collection = JsonConvert.DeserializeObject<DialogCollection>(json);
                if (collection?.Dialogs == null)
                    throw new Exception("Dialogs section missing or null");

                _dialogs = collection.Dialogs
                    .Where(d => !string.IsNullOrWhiteSpace(d.Id))
                    .ToDictionary(d => d.Id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading dialogs: " + ex.Message);
            }
        }

        public void StartDialog(string dialogId)
        {
            if (_dialogs.TryGetValue(dialogId, out var dialog))
            {
                _currentDialog = dialog;
                _currentLineIndex = 0;
            }
        }

        public DialogLineData GetCurrentLine()
        {
            if (_currentDialog == null || _currentLineIndex >= _currentDialog.Lines.Count)
                return null;

            return _currentDialog.Lines[_currentLineIndex];
        }

        public void NextLine()
        {
            var currentLine = GetCurrentLine();

            if (currentLine != null && !string.IsNullOrEmpty(currentLine.NextDialog))
            {
                StartDialog(currentLine.NextDialog);
                return;
            }

            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialog.Lines.Count)
            {
                OnDialogEnd?.Invoke();
            }
        }

        public void SelectOption(int optionIndex)
        {
            var currentLine = GetCurrentLine();
            if (currentLine?.Options != null && optionIndex >= 0 && optionIndex < currentLine.Options.Count)
            {
                var option = currentLine.Options[optionIndex];
                StartDialog(option.NextDialog);
            }
        }
    }
}
