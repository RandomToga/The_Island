using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace The_Island.Core
{
    public static class ScreenUtils
    {
        // Центрирование по оси X
        public static float CenterX(Texture2D texture, GraphicsDevice graphicsDevice, float scale = 1f)
        {
            var screenWidth = graphicsDevice.Viewport.Width;
            return (screenWidth - texture.Width * scale) / 2f;
        }

        // Центрирование по оси Y
        public static float CenterY(Texture2D texture, GraphicsDevice graphicsDevice, float scale = 1f)
        {
            var screenHeight = graphicsDevice.Viewport.Height;
            return (screenHeight - texture.Height * scale) / 2f;
        }

        // Растяжение фона на весь экран с сохранением пропорций (cover)
        public static (Vector2 position, float scale) ScaleToCover(Texture2D texture, GraphicsDevice graphicsDevice)
        {
            var screenWidth = graphicsDevice.Viewport.Width;
            var screenHeight = graphicsDevice.Viewport.Height;

            float scaleX = (float)screenWidth / texture.Width;
            float scaleY = (float)screenHeight / texture.Height;

            // Выбираем больший масштаб — чтобы полностью покрыть экран
            float scale = Math.Max(scaleX, scaleY);

            // Центрируем изображение (часть может быть за пределами экрана)
            float x = (screenWidth - texture.Width * scale) / 2f;
            float y = (screenHeight - texture.Height * scale) / 2f;

            return (new Vector2(x, y), scale);
        }
    }
}
