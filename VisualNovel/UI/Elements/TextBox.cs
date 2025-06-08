using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using The_Island.Core;

namespace The_Island.UI
{
    public class TextBox
    {
        private Texture2D _background;
        private SpriteFont _font;
        private MouseState _prevMouseState;

        private List<Rectangle> _optionButtons = new();
        private List<int> _visibleOptionIndices = new();

        private const int lineHeight = 35;
        private readonly Vector2 _textOffset = new(300, 120);
        private readonly Vector2 _padding = new(25, -55); // фиксированный отступ для имени персонажа
        private const float Scale = 1.1f; // фиксированный масштаб

        public Vector2 Position { get; set; }
        public string Text { get; set; } = "";
        public string Character { get; set; } = "";
        public Color TextColor { get; } = Color.White; // только геттер, цвет фиксирован

        public TextBox(Texture2D background, SpriteFont font, Vector2 position)
        {
            _background = background;
            _font = font;
            Position = position;
        }

        // Метод для разделения текста на строки по ширине
        private List<string> WrapText(SpriteFont font, string text)
        {
            float maxLineWidth = 1200f;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_background, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, Character, Position + _textOffset + _padding, TextColor);

            var wrappedLines = WrapText(_font, Text);

            Vector2 startPos = Position + _textOffset;

            for (int i = 0; i < wrappedLines.Count; i++)
            {
                Vector2 linePos = new(startPos.X, startPos.Y + i * lineHeight);
                spriteBatch.DrawString(_font, wrappedLines[i], linePos, TextColor);
            }
        }

        public void DrawOptions(SpriteBatch spriteBatch, DialogLineData currentLine, DialogManager dialogManager, int centerX, int textStartY)
        {
            _optionButtons.Clear();
            _visibleOptionIndices.Clear();

            if (currentLine?.Options == null || currentLine.Options.Count == 0)
                return;

            for (int i = 0; i < currentLine.Options.Count; i++)
            {
                var option = currentLine.Options[i];
                var optionText = $"{i + 1}. {option.Text}";
                var optionSize = _font.MeasureString(optionText);

                float optionX = centerX - optionSize.X / 2;
                float optionY = textStartY + 20 + i * (lineHeight + 5);

                var optionRect = new Rectangle((int)optionX, (int)optionY, (int)optionSize.X, (int)optionSize.Y);
                _optionButtons.Add(optionRect);
                _visibleOptionIndices.Add(i);

                spriteBatch.DrawString(_font, optionText, new Vector2(optionX, optionY), Color.White);
            }
        }

        public void HandleInput(MouseState mouseState, DialogLineData currentLine, DialogManager dialogManager)
        {
            if (currentLine?.Options == null || currentLine.Options.Count == 0)
                return;

            if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < _optionButtons.Count; i++)
                {
                    if (_optionButtons[i].Contains(mouseState.Position))
                    {
                        dialogManager.SelectOption(_visibleOptionIndices[i]);
                        break;
                    }
                }
            }

            _prevMouseState = mouseState;
        }
    }
}
