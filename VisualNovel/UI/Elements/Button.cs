using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace The_Island.UI.Elements
{
    class Button
    {
        public static SoundEffect ClickSound; //звук нажатия кнопки для всех одинаковый
        public static SpriteFont font; //шрифт текста кнопки
        private Texture2D background; //фон для кнопки
        private string text; //текст на кнопке
        private Vector2 position; //позиция кнопки
        private bool hovered; //ну типа флаг наведена/не наведена мышь
        private Rectangle size; //границы кнопки
        private bool useBackground; //с фоном или без фона кнопка

        public event EventHandler Click;

        //конструктор
        public Button(Texture2D background, Vector2 position, string text, bool useBackground = true)
        {
            this.background = background;
            this.position = position;
            this.text = text;
            this.useBackground = useBackground;

            if (useBackground)
            {
                size = new Rectangle((int)position.X, (int)position.Y, background.Width, background.Height);
            }
            else
            {
                Vector2 textsize = font.MeasureString(text); //размер текста
                size = new Rectangle((int)position.X, (int)position.Y, (int)textsize.X + 10, (int)textsize.Y + 5);
            }
        }

        private MouseState prevMouse;

        public void Update()
        {
            MouseState mouse = Mouse.GetState();
            hovered = size.Contains(mouse.Position);

            if (hovered && mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
            {
                ClickSound?.Play();
                Click?.Invoke(this, EventArgs.Empty);
            }

            prevMouse = mouse;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (useBackground && background != null)
            {
                Color color = hovered ? Color.Gray : Color.White;
                spriteBatch.Draw(background, size, color);
            }
            if (text != null) //проверяем нужно ли рисовать текст
            {
                Vector2 textSize = font.MeasureString(text);
                Vector2 textPosition = new Vector2(
                    size.X + (size.Width - textSize.X) / 2,
                    size.Y + (size.Height - textSize.Y) / 2
                );

                // Если нет фона — делаем текст серым при наведении
                Color textColor = hovered ? Color.DarkGray : Color.White;
                spriteBatch.DrawString(font, text, textPosition, textColor);
            }
        }
    }
}
