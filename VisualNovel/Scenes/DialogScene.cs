using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using The_Island.Core;
using The_Island.UI;
using The_Island.UI.Elements;

namespace The_Island.Scenes
{
    public class DialogScene : IScene
    {

        private SpriteFont dialogFont;
        //текстбокс
        private TextBox textBox;
        private Texture2D textboxTexture;
        // Используем словарь для хранения персонажей
        private Dictionary<string, Character> characters = new();
        // Используем словарь для хранения фонов
        private Dictionary<string, Texture2D> backgrounds = new();
        private Texture2D currentBackground;



        private DialogManager dialogManager;
        private KeyboardState prevKeyboard;
        private MouseState prevMouse;

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            Console.WriteLine("Инициализация диалоговой сцены...");
            dialogFont = content.Load<SpriteFont>("Fonts/Font");
            textboxTexture = content.Load<Texture2D>("Images/UI/textBox");
            //подгружаем звук для кнопок и устанавливаем шрифт
            Button.ClickSound = content.Load<SoundEffect>("Sounds/click");
            Button.font = dialogFont;
            // Создаём персонажей и добавляем их в словарь
            characters.Add("mira_default", new Character("Mira", content.Load<Texture2D>("Images/Characters/mira_default"), Vector2.Zero));
            characters.Add("aaron_default", new Character("Aaron", content.Load<Texture2D>("Images/Characters/aaron_default"), new Vector2(200, 200)));
            characters.Add("ada_default", new Character("Ada", content.Load<Texture2D>("Images/Characters/ada_default"), new Vector2(150, 150)));
            characters.Add("alex_default", new Character("Alex", content.Load<Texture2D>("Images/Characters/alex_default"), new Vector2(180, 180)));
            characters.Add("iris_default", new Character("Iris", content.Load<Texture2D>("Images/Characters/iris_default"), new Vector2(170, 170)));
            characters.Add("author_default", new Character("...", content.Load<Texture2D>("Images/Characters/author_default"), new Vector2(100, 100)));

            // подгружаем фоны
            backgrounds.Add("cockpit_red", content.Load<Texture2D>("Images/Backgrounds/cockpit_red"));
            backgrounds.Add("camp_night", content.Load<Texture2D>("Images/Backgrounds/camp_night"));
            backgrounds.Add("hangar_dark", content.Load<Texture2D>("Images/Backgrounds/hangar_dark"));
            backgrounds.Add("menu", content.Load<Texture2D>("Images/Backgrounds/menu_background"));

            // Установка фонового изображения по умолчанию
            currentBackground = backgrounds["cockpit_red"];

            dialogManager = new DialogManager();

            // Загружаем текст диалогов из JSON-файла
            string json = System.IO.File.ReadAllText("Content/Data/dialogs.json");
            dialogManager.LoadDialogs(json);
            dialogManager.StartDialog("start"); // ID первой сцены
            UpdateBackground();

            // Создаём TextBox

            Vector2 fixedTextBoxPosition = new Vector2(100, 800);

            textBox = new TextBox(textboxTexture, dialogFont, fixedTextBoxPosition);

        }
        //метод для обновления заднего фона
        private void UpdateBackground()
        {
            var currentLine = dialogManager.GetCurrentLine();
            if (currentLine != null && !string.IsNullOrEmpty(currentLine.BackgroundImage))
            {
                if (backgrounds.ContainsKey(currentLine.BackgroundImage))
                    currentBackground = backgrounds[currentLine.BackgroundImage];
            }
        }

        public void Update(GameTime gameTime)
        {
            // Получаем текущее состояние мыши и клавиатуры
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            // Получаем текущую строку диалога
            var currentLine = dialogManager.GetCurrentLine();

            // Обновляем задний фон (если есть анимации или логика)
            UpdateBackground();

            // Обработка клика по вариантам ответа, если они есть
            if (currentLine?.Options != null && currentLine.Options.Count > 0)
            {
                textBox.HandleInput(mouseState, currentLine, dialogManager);
            }
            // Если нет вариантов – переходим к следующей строке по одиночному нажатию пробела или клику мыши
            else if ((keyboardState.IsKeyDown(Keys.Space) && prevKeyboard.IsKeyUp(Keys.Space)) ||
                     (mouseState.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released))
            {
                dialogManager.NextLine();
            }

            // Сохраняем предыдущее состояние мыши и клавиатуры
            prevMouse = mouseState;
            prevKeyboard = keyboardState;
        }




        public void Draw(SpriteBatch spriteBatch)
        {
            // Получаем текущую строку диалога
            var currentLine = dialogManager.GetCurrentLine();
            if (currentLine == null)
                return; // Нет текущей строки — выходим, чтобы не рисовать ничего

            //Чтобы масштабировать изображение, нужно использовать перегрузку Draw, в которой можно указать scale
            //spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, effects, layerDepth);

            // Для фона: растягиваем фон с сохранением пропорций, центрируем его по экрану
            var (bgPos, bgScale) = ScreenUtils.ScaleToCover(currentBackground, spriteBatch.GraphicsDevice);
            spriteBatch.Draw(currentBackground, bgPos, null, Color.White, 0f, Vector2.Zero, bgScale, SpriteEffects.None, 0f);

            // Проверяем, есть ли персонаж в словаре по имени
            if (!string.IsNullOrEmpty(currentLine.CharacterImage))
            {
                if (characters.ContainsKey(currentLine.CharacterImage))
                {
                    var character = characters[currentLine.CharacterImage];
                    float scale = 0.7f;
                    float charPosX = ScreenUtils.CenterX(character.Image, spriteBatch.GraphicsDevice, scale);
                    float charPosY = spriteBatch.GraphicsDevice.Viewport.Height - character.Image.Height * scale;
                    Vector2 charPos = new Vector2(charPosX, charPosY);
                    spriteBatch.Draw(character.Image, charPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }

            

            if (currentLine != null)
            {
                // Устанавливаем текст и имя персонажа для текстбокса
                textBox.Text = currentLine.Text;
                textBox.Character = currentLine.Speaker;

                // Отрисовка самого текстбокса
                textBox.Draw(spriteBatch);

                // Отрисовка вариантов выбора (если они есть)
                if (currentLine.Options != null && currentLine.Options.Count > 0)
                {
                    int screenCenterX = spriteBatch.GraphicsDevice.Viewport.Width / 2;
                    int textStartY = (int)textBox.Position.Y + 180;

                    textBox.DrawOptions(spriteBatch, currentLine, dialogManager, screenCenterX, textStartY);
                }
            }

        }
    }
}
