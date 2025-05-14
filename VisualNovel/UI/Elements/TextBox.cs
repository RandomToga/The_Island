using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace The_Island.UI
{
    public class TextBox
    {
        private Texture2D _textbox;
        private SpriteFont _font;

        public Vector2 Position { get; set; }
        public float Scale { get; set; } = 1f;
        public string Text { get; set; } = "";
        public string Character { get; set; } = "";
        public Color TextColor { get; set; } = Color.Black;
        public Vector2 Padding { get; set; } = new Vector2(55, -55); //for character name

        // Конструктор
        public TextBox(Texture2D background, SpriteFont font, Vector2 position)
        {
            _textbox = background;
            _font = font;
            Position = position;
        }

        public Vector2 textPos()
        {
            return new Vector2(Position.X + 200, Position.Y + 120);
        }
        // Метод для отрисовки текстбокса
        public void Draw(SpriteBatch spriteBatch)
        {
            // Отрисовка текстбокса
            spriteBatch.Draw(_textbox, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            // Отрисовка имени персонажа
            spriteBatch.DrawString(_font, Character, textPos()+ Padding, TextColor);
            // Отрисовка текста 
            spriteBatch.DrawString(_font, Text, textPos(), TextColor);
        }
        // Метод для получения размеров текстбокса с учетом масштаба
        public Vector2 GetSize()
        {
            return new Vector2(_textbox.Width * Scale, _textbox.Height * Scale);
        }
        // Метод для центрации текстбокса по нижней части экрана
        public void CenterBottom(GraphicsDevice graphicsDevice)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float textboxWidth = GetSize().X;
            float screenHeight = graphicsDevice.Viewport.Height;
            float textboxHeight = GetSize().Y;
            Position = new Vector2((screenWidth - textboxWidth) / 2, screenHeight - textboxHeight - Padding.Y * Scale);
        }
    }
}
