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
            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialog.Lines.Count)
            {
                var currentLine = GetCurrentLine();
                if (currentLine != null && !string.IsNullOrEmpty(currentLine.NextDialog))
                {
                    StartDialog(currentLine.NextDialog);
                }
                else
                {
                    OnDialogEnd?.Invoke();
                }
            }
        }

        public void SelectOption(int optionIndex)
        {
            var currentLine = GetCurrentLine();
            if (currentLine?.Options != null && optionIndex < currentLine.Options.Count)
            {
                StartDialog(currentLine.Options[optionIndex].NextDialog);
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
            _background = Content.Load<Texture2D>("background");
            _character = Content.Load<Texture2D>("character_girl");
            _characterMan = Content.Load<Texture2D>("character_man");
            _font = Content.Load<SpriteFont>("Font");
            _textBox = Content.Load<Texture2D>("textBox");

            _dialogManager = new DialogManager();

            string jsonPath = Path.Combine(Content.RootDirectory, "Data", "dialogs.json");
            string json = File.ReadAllText(jsonPath);
            _dialogManager.LoadDialogs(json);
            _dialogManager.StartDialog("start");
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var currentLine = _dialogManager.GetCurrentLine();
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
                            _dialogManager.SelectOption(i);
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
            _spriteBatch.Draw(_background, GraphicsDevice.Viewport.Bounds, Color.White);

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
                    new Vector2(120, 550), Color.White);

                // Рисуем варианты ответа
                if (currentLine.Options != null)
                {
                    _optionButtons.Clear();
                    for (int i = 0; i < currentLine.Options.Count; i++)
                    {
                        var optionRect = new Rectangle(120, 600 + i * 40, 400, 30);
                        _optionButtons.Add(optionRect);
                        _spriteBatch.DrawString(_font, $"{i + 1}. {currentLine.Options[i].Text}",
                            new Vector2(optionRect.X, optionRect.Y), Color.White);
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}