using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace The_Island.Core
{
    public class Character
    {
        public string Name { get; set; }
        public Texture2D Image { get; set; }
        public Vector2 Position { get; set; }
        public float Scale { get; set; } = 1f;

        public Character(string name, Texture2D image, Vector2 position, float scale = 1f)
        {
            Name = name;
            Image = image;
            Position = position;
            Scale = scale;
        }

        // Метод для рисования персонажа
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}
