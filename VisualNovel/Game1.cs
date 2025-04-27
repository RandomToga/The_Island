using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace VisualNovel
{
    public class Stats
    {
        public int Charisma { get; set; } = 0;
        public int Intelligence { get; set; } = 0;
        public int Reputation { get; set; } = 0;

        public void ApplyChanges(Dictionary<string, int> changes)
        {
            foreach (var kvp in changes)
            {
                switch (kvp.Key.ToLower())
                {
                    case "charisma": Charisma += kvp.Value; break;
                    case "intelligence": Intelligence += kvp.Value; break;
                    case "reputation": Reputation += kvp.Value; break;
                }
            }
        }
        public bool MeetsRequirements(Dictionary<string, int> requirements)
        {
            foreach (var kvp in requirements)
            {
                int current = kvp.Key.ToLower() switch
                {
                    "charisma" => Charisma,
                    "intelligence" => Intelligence,
                    "reputation" => Reputation,
                    _ => 0
                };

                if (current < kvp.Value)
                    return false;
            }
            return true;
        }
    }
    public class DialogOption
    {
        public string Text { get; set; }
        public string NextDialog { get; set; }
        public Dictionary<string, int> StatChanges { get; set; } = new(); //статы
        public Dictionary<string, int> Requirements { get; set; } = new(); // условия
    }

    public class DialogLineData
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string CharacterImage { get; set; }
        public string BackgroundImage { get; set; }
        public string NextDialog { get; set; }
        public List<DialogOption> Options { get; set; } = new List<DialogOption>();
    }

    public class DialogData
    {
        public string Id { get; set; }
        public List<DialogLineData> Lines { get; set; } = new List<DialogLineData>();
    }

    public class DialogCollection
    {
        public List<DialogData> Dialogs { get; set; } = new List<DialogData>();
    }

    public class DialogManager
    {
        private Dictionary<string, DialogData> _dialogs = new Dictionary<string, DialogData>();
        private DialogData _currentDialog;
        private int _currentLineIndex;
        //статы
        private Stats _playerStats = new();
        public Stats PlayerStats => _playerStats;

        public event Action OnDialogEnd;

        public void LoadDialogs(string json)
        {
            try
            {
                var collection = JsonConvert.DeserializeObject<DialogCollection>(json);
                _dialogs = collection.Dialogs.ToDictionary(d => d.Id);
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
            var currentLine = GetCurrentLine(); // Сначала получаем текущую строку

            if (currentLine != null && !string.IsNullOrEmpty(currentLine.NextDialog))
            {
                StartDialog(currentLine.NextDialog); // Переход в другую сцену
                return;
            }

            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialog.Lines.Count)
            {
                OnDialogEnd?.Invoke(); // Конец диалога
            }

        }
        public void SelectOption(int optionIndex)
        {
            var currentLine = GetCurrentLine();
            if (currentLine?.Options != null && optionIndex < currentLine.Options.Count)
            {
                var option = currentLine.Options[optionIndex];

                if (option.StatChanges != null)
                    _playerStats.ApplyChanges(option.StatChanges);

                StartDialog(option.NextDialog);
            }
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;
        private Texture2D _character;
        private Texture2D _characterMan;
        private SpriteFont _font;
        private DialogManager _dialogManager;
        private Texture2D _textBox;
        private List<Rectangle> _optionButtons = new List<Rectangle>();
        private MouseState _prevMouseState;
        // загрузка фонов, которые хранятся в словаре
        private Dictionary<string, Texture2D> _backgrounds = new Dictionary<string, Texture2D>();
        private Texture2D _currentBackground;
        private List<int> _visibleOptionIndices = new List<int>(); // индексы видимых опций

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _character = Content.Load<Texture2D>("character_girl");
            _characterMan = Content.Load<Texture2D>("character_man");
            _font = Content.Load<SpriteFont>("Font");
            _textBox = Content.Load<Texture2D>("textBox");
            // Загрузка фонов из ресурсов
            _backgrounds["background_1"] = Content.Load<Texture2D>("background_1");
            _backgrounds["background_2"] = Content.Load<Texture2D>("background_2");

            _dialogManager = new DialogManager();

            string jsonPath = Path.Combine(Content.RootDirectory, "Data", "dialogs.json");
            string json = File.ReadAllText(jsonPath);
            _dialogManager.LoadDialogs(json);
            _dialogManager.StartDialog("start");
        }
        private void UpdateBackground(DialogLineData line)
        {
            if (line != null && !string.IsNullOrEmpty(line.BackgroundImage))
            {
                if (_backgrounds.TryGetValue(line.BackgroundImage, out var bg))
                {
                    _currentBackground = bg;
                }
            }
        }
        protected override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            var currentLine = _dialogManager.GetCurrentLine();
            UpdateBackground(currentLine);
            if (currentLine?.Options != null && currentLine.Options.Count > 0)
            {
                // Есть варианты выбора
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    _prevMouseState.LeftButton == ButtonState.Released)
                {
                    for (int i = 0; i < _optionButtons.Count; i++)
                    {
                        if (_optionButtons[i].Contains(mouseState.Position))
                        {
                            int actualOptionIndex = _visibleOptionIndices[i]; 
                            _dialogManager.SelectOption(actualOptionIndex);
                            break;
                        }
                    }

                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                     (mouseState.LeftButton == ButtonState.Pressed &&
                      _prevMouseState.LeftButton == ButtonState.Released))
            {
                _dialogManager.NextLine();
            }

            _prevMouseState = mouseState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            // Рисуем фон
            _spriteBatch.Draw(_currentBackground ?? _background, GraphicsDevice.Viewport.Bounds, Color.White);
            /*var stats = _dialogManager.PlayerStats;
            _spriteBatch.DrawString(_font, $"Харизма: {stats.Charisma}  Интеллект: {stats.Intelligence}  Репутация: {stats.Reputation}", new Vector2(20, 20), Color.Yellow);
            */
            var currentLine = _dialogManager.GetCurrentLine();
            if (currentLine != null)
            {
                // Рисуем персонажа
                Texture2D character = currentLine.CharacterImage switch
                {
                    "character_girl" => _character,
                    "character_man" => _characterMan,
                    _ => null
                };

                if (character != null)
                {
                    _spriteBatch.Draw(character, new Vector2(300, 200), Color.White);
                }

                // Рисуем текстовое окно
                _spriteBatch.Draw(_textBox, new Rectangle(100, 500, 1080, 200), Color.White);

                // Рисуем текст
                if (!string.IsNullOrEmpty(currentLine.Speaker))
                {
                    _spriteBatch.DrawString(_font, $"{currentLine.Speaker}:",
                        new Vector2(120, 520), Color.White);
                }

                _spriteBatch.DrawString(_font, currentLine.Text,
                    new Vector2(120, 550), Color.Black);

                // Рисуем варианты ответа
                _optionButtons.Clear();
                _visibleOptionIndices.Clear();

                var stats = _dialogManager.PlayerStats;
                int yOffset = 0;

                for (int i = 0; i < currentLine.Options.Count; i++)
                {
                    var option = currentLine.Options[i];
                    if (option.Requirements != null && !stats.MeetsRequirements(option.Requirements))
                        continue;

                    var optionRect = new Rectangle(120, 600 + yOffset * 40, 400, 30);
                    _optionButtons.Add(optionRect);
                    _visibleOptionIndices.Add(i); // Сохраняем индекс исходной опции

                    _spriteBatch.DrawString(_font, $"{yOffset + 1}. {option.Text}",
                        new Vector2(optionRect.X, optionRect.Y), Color.White);

                    yOffset++;
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}