using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
// сжать этих брошек девочек потому что они не хотят загружаться в гит
// проблема в шрифте нет знака -
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
    public class SaveData
    {
        public string CurrentDialogId { get; set; }
        public int CurrentLineIndex { get; set; }
        public Stats PlayerStats { get; set; }
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
        public SaveData GetSaveData()
        {
            return new SaveData
            {
                CurrentDialogId = _currentDialog?.Id,
                CurrentLineIndex = _currentLineIndex,
                PlayerStats = _playerStats
            };
        }

        public void LoadSaveData(SaveData data)
        {
            if (data == null || !_dialogs.ContainsKey(data.CurrentDialogId))
                return;

            _currentDialog = _dialogs[data.CurrentDialogId];
            _currentLineIndex = data.CurrentLineIndex;
            _playerStats = data.PlayerStats ?? new Stats();
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
        //метод для разделения строк по пробелам для красивого вывода фраз
        private List<string> WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                float lineWidth = font.MeasureString(testLine).X;

                if (lineWidth > maxLineWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);

                    currentLine = word; // начать новую строку
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }

        enum GameState
        {
            Menu,
            Playing
        }
        GameState _currentState = GameState.Menu;
        private Texture2D _menuBackground;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;
        private Texture2D _Mira;
        private Texture2D _Ada;
        private Texture2D _Aaron;
        private Texture2D _Iris;
        private Texture2D _Alex;
        private SpriteFont _font;
        private DialogManager _dialogManager;
        private Texture2D _textBox;
        private Texture2D _backButtonTexture;
        private List<Rectangle> _optionButtons = new List<Rectangle>();
        private MouseState _prevMouseState;
        // загрузка фонов, которые хранятся в словаре
        private Dictionary<string, Texture2D> _backgrounds = new Dictionary<string, Texture2D>();
        private Texture2D _currentBackground;
        private List<int> _visibleOptionIndices = new List<int>(); // индексы видимых опций
        //для всплывающего сообщения о сохранении игры
        private string _popupMessage = null;
        private double _popupTimer = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        private string savePath => Path.Combine(Environment.CurrentDirectory, "save.json");

        private void SaveGame()
        {
            var saveData = _dialogManager.GetSaveData();
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(savePath, json);
            _popupMessage = "Игра успешно сохранена";
            _popupTimer = 0.5;
        }

        private void LoadGame()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json);
                _dialogManager.LoadSaveData(saveData);
            }
        }

        protected override void Initialize()
        {
            // Устанавливаем полноэкранный режим
            _graphics.IsFullScreen = true;

            // Получаем текущее разрешение экрана
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = displayMode.Width;
            _graphics.PreferredBackBufferHeight = displayMode.Height;

            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _menuBackground = Content.Load<Texture2D>("menu_background"); //бэк для меню
            _Mira = Content.Load<Texture2D>("mira_default"); //мира (потом поменять на словарь)
            _Ada = Content.Load<Texture2D>("ada_default");// ада (потом поменять на словарь)
            _Iris = Content.Load<Texture2D>("iris_default");//ирис
            _Aaron = Content.Load<Texture2D>("aaron_default"); //аарон
            _Alex = Content.Load<Texture2D>("alex_default"); //алекс
            _font = Content.Load<SpriteFont>("Font");
            _textBox = Content.Load<Texture2D>("textBox");
            _backButtonTexture = Content.Load<Texture2D>("back_button"); //кнопка назад
            // Загрузка фонов из ресурсов
            _backgrounds["camp_night"] = Content.Load<Texture2D>("camp_night");
            _backgrounds["hangar_dark"] = Content.Load<Texture2D>("hangar_dark");
            _background = _backgrounds["camp_night"];
            _currentBackground = _background;
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
            if (_currentState == GameState.Menu)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    SaveGame();
                    Exit();
                }
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    _prevMouseState.LeftButton == ButtonState.Released)
                {
                    Rectangle playButtonRect = new Rectangle(540, 300, 200, 60);
                    Rectangle saveButtonRect = new Rectangle(540, 340, 200, 40);
                    Rectangle loadButtonRect = new Rectangle(540, 380, 200, 40);
                    Rectangle exitButtonRect = new Rectangle(540, 420, 200, 40);
                    if (playButtonRect.Contains(mouseState.Position))
                    {
                        _dialogManager.StartDialog("start"); // Начать игру заново
                        _currentState = GameState.Playing;
                    }
                    else if (saveButtonRect.Contains(mouseState.Position))
                    {
                        SaveGame();
                    }
                    else if (loadButtonRect.Contains(mouseState.Position))
                    {
                        LoadGame();
                        _currentState = GameState.Playing;
                    }
                    else if (exitButtonRect.Contains(mouseState.Position))
                    {
                        SaveGame();
                        Exit();
                    }
                }

                _prevMouseState = mouseState;
            }

            if (_currentState == GameState.Playing)
            {

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    SaveGame();
                    Exit();
                }
                if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
                {
                    // Кнопка "назад" — в левом верхнем углу
                    Rectangle backButtonRect = new Rectangle(10, 10, 100, 100);
                    if (backButtonRect.Contains(mouseState.Position))
                    {
                        _currentState = GameState.Menu;
                        _prevMouseState = mouseState;
                        return;
                    }
                }
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
            }
            _prevMouseState = mouseState;
            base.Update(gameTime);
            if (_popupTimer > 0)
            {
                _popupTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_popupTimer <= 0)
                {
                    _popupMessage = null;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            if (_currentState == GameState.Menu)
            {
                
                _spriteBatch.Draw(_menuBackground, GraphicsDevice.Viewport.Bounds, Color.White);
                _spriteBatch.DrawString(_font, "Новая игра", new Vector2(600, 320), Color.Yellow);
                _spriteBatch.DrawString(_font, "Сохранить", new Vector2(600, 360), Color.Yellow);
                _spriteBatch.DrawString(_font, "Продолжить", new Vector2(600, 400), Color.Yellow);
                _spriteBatch.DrawString(_font, "Выход", new Vector2(600, 440), Color.Yellow);

            }
            if (_currentState == GameState.Playing)
            {
                // Рисуем фон
                _spriteBatch.Draw(_currentBackground ?? _background, GraphicsDevice.Viewport.Bounds, Color.White);
                /*var stats = _dialogManager.PlayerStats;
                _spriteBatch.DrawString(_font, $"Харизма: {stats.Charisma}  Интеллект: {stats.Intelligence}  Репутация: {stats.Reputation}", new Vector2(20, 20), Color.Yellow);
                */
                // Кнопка "Назад в меню"
                var backButtonRect = new Rectangle(10, 10, 100, 100);
                _spriteBatch.Draw(_backButtonTexture, backButtonRect, Color.White);

                var currentLine = _dialogManager.GetCurrentLine();
                if (currentLine != null)
                {
                    // Рисуем персонажа
                    Texture2D character = currentLine.CharacterImage switch
                    {
                        "mira_default" => _Mira,
                        "ada_default" => _Ada,
                        "aaron_default"=> _Aaron,
                        "iris_default"=> _Iris,
                        "alex_default"=> _Alex,
                        _ => null
                    };

                    if (character != null)
                    {
                        float scale = 0.8f; // масштаб персонажа
                        var characterX = (GraphicsDevice.Viewport.Width - character.Width*scale) / 2f;
                        var characterY = GraphicsDevice.Viewport.Height - character.Height * scale;

                        _spriteBatch.Draw(character, new Vector2(characterX, characterY), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }

                    // Центрированный текстбокс снизу
                    int textBoxWidth = GraphicsDevice.Viewport.Width - 130;
                    int textBoxHeight = 400;
                    int textBoxX = (GraphicsDevice.Viewport.Width - textBoxWidth) / 2;
                    int textBoxY = GraphicsDevice.Viewport.Height - textBoxHeight - 40;

                    var textBoxRect = new Rectangle(textBoxX, textBoxY, textBoxWidth, textBoxHeight);
                    _spriteBatch.Draw(_textBox, textBoxRect, Color.White);


                    // Отступы
                    float paddingTop = 100f;
                    float paddingSides = 210f;
                    float maxLineWidth = textBoxWidth - 2 * paddingSides;
                    float centerX = textBoxX + textBoxWidth / 2f;

                    // Имя персонажа — в верхнем левом углу текстбокса
                    if (!string.IsNullOrEmpty(currentLine.Speaker))
                    {
                        var speakerText = $"{currentLine.Speaker}";
                        _spriteBatch.DrawString(_font, speakerText,
                            new Vector2(textBoxX + 250f, textBoxY + 67f),
                            Color.White);
                    }

                    // Основной текст с переносом строк
                    var wrappedLines = WrapText(_font, currentLine.Text, maxLineWidth);
                    float lineHeight = _font.LineSpacing;
                    float textStartY = textBoxY + paddingTop + lineHeight;

                    foreach (var line in wrappedLines)
                    {
                        var lineSize = _font.MeasureString(line);
                        _spriteBatch.DrawString(_font, line,
                            new Vector2(centerX - lineSize.X / 2, textStartY), Color.White);
                        textStartY += lineHeight;
                    }


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

                        var optionText = $"{yOffset + 1}. {option.Text}";
                        var optionSize = _font.MeasureString(optionText);

                        float optionX = centerX - optionSize.X / 2;
                        float optionY = textStartY + 20 + yOffset * (lineHeight + 5);

                        var optionRect = new Rectangle((int)optionX, (int)optionY, (int)optionSize.X, (int)optionSize.Y);
                        _optionButtons.Add(optionRect);
                        _visibleOptionIndices.Add(i);

                        _spriteBatch.DrawString(_font, optionText, new Vector2(optionX, optionY), Color.White);
                        yOffset++;
                    }


                }
            }
            if (!string.IsNullOrEmpty(_popupMessage))
            {
                var size = _font.MeasureString(_popupMessage);
                var position = new Vector2(
                    (GraphicsDevice.Viewport.Width - size.X) / 2,
                    GraphicsDevice.Viewport.Height - 100);

                _spriteBatch.DrawString(_font, _popupMessage, position, Color.Lime);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}