using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace The_Island.Core
{
    public record class DialogOption
    {
        public string Text { get; set; }
        public string NextDialog { get; set; }
    }

    public record class DialogLineData
    {
        public string Id { get; set; }
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string CharacterImage { get; set; }
        public string BackgroundImage { get; set; }
        public string NextDialog { get; set; }
        public List<DialogOption> Options { get; set; } = new();
    }
    public class DialogManager
    {
        private Dictionary<string, DialogLineData> _dialogs = new();
        private DialogLineData _currentLine;

        public event Action OnDialogEnd;

        public void LoadDialogs(string json)
        {
            try
            {
                var dialogLines = JsonConvert.DeserializeObject<List<DialogLineData>>(json);
                if (dialogLines == null)
                    throw new Exception("Dialog list is null");

                _dialogs = dialogLines
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
            if (_dialogs.TryGetValue(dialogId, out var line))
            {
                _currentLine = line;
            }
        }

        public DialogLineData GetCurrentLine() => _currentLine;

        public void NextLine()
        {
            if (_currentLine != null && !string.IsNullOrEmpty(_currentLine.NextDialog))
            {
                StartDialog(_currentLine.NextDialog);
                return;
            }

            OnDialogEnd?.Invoke();
        }

        public void SelectOption(int optionIndex)
        {
            if (_currentLine?.Options != null &&
                optionIndex >= 0 &&
                optionIndex < _currentLine.Options.Count)
            {
                var option = _currentLine.Options[optionIndex];
                if (option.NextDialog != _currentLine.Id)
                    StartDialog(option.NextDialog);
            }
        }
    }
}
