using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

//using System.Reflection.Metadata;
using The_Island.Core;
using The_Island.UI;
using The_Island.UI.Elements;
//using static System.Formats.Asn1.AsnWriter;

namespace The_Island.Scenes
{
    public class DialogScene : IScene
    {
        
        private SpriteFont dialogFont;
        //текстбокс
        private TextBox textBox;
        private Texture2D textboxTexture;
        //кнопки
        Button button;
        // Используем словарь для хранения персонажей
        private Dictionary<string, Character> characters = new();
        //// Используем словарь для хранения фонов
        private Dictionary<string, Texture2D> backgrounds = new();
        private Texture2D currentBackground;


        private DialogManager dialogManager;
        private KeyboardState previousKeyboard;

        public void Load(ContentManager content)
        {
            Console.WriteLine("Инициализация диалоговой сцены...");
            dialogFont = content.Load<SpriteFont>("Fonts/Font");
            textboxTexture = content.Load<Texture2D>("Images/UI/textBox");
            //подгружаем звук для кнопок и устанавливаем шрифт
            Button.ClickSound = content.Load<SoundEffect>("Sounds/click");
            Button.font = dialogFont;
            // Создаём персонажей и добавляем их в словарь
            characters.Add("mira_default", new Character("Mira", content.Load<Texture2D>("Images/Characters/mira_default"), new Vector2(100, 100)));
            characters.Add("aaron_default", new Character("Aaron", content.Load<Texture2D>("Images/Characters/aaron_default"), new Vector2(200, 200))); // Пример второго персонажа
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
            var textboxPos = new Vector2(50, 500);
            textBox = new TextBox(textboxTexture, dialogFont, textboxPos)
            {
                Scale = 1f,
                TextColor = Color.White
            };
        }
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
            var state = Keyboard.GetState();


            // Обработка выбора
            var line = dialogManager.GetCurrentLine();
            if (line != null && line.Options != null)
            {
                for (int i = 0; i < line.Options.Count; i++)
                {
                    Keys key = Keys.D1 + i;
                    if (state.IsKeyDown(key) && previousKeyboard.IsKeyUp(key))
                    {
                        dialogManager.SelectOption(i);
                        UpdateBackground();
                    }
                }
            }
            if (state.IsKeyDown(Keys.Space) && previousKeyboard.IsKeyUp(Keys.Space))
            {
                dialogManager.NextLine();
                UpdateBackground();
            }

            previousKeyboard = state;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var current = dialogManager.GetCurrentLine();
            if (current == null)
                return; // Нет текущей строки — выходим, чтобы не рисовать ничего

            //Чтобы масштабировать изображение, нужно использовать перегрузку Draw, в которой можно указать scale
            //spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, effects, layerDepth);

            // Для фона: растягиваем фон с сохранением пропорций, центрируем его по экрану
            var (bgPos, bgScale) = ScreenUtils.ScaleToCover(currentBackground, spriteBatch.GraphicsDevice);
            spriteBatch.Draw(currentBackground, bgPos, null, Color.White, 0f, Vector2.Zero, bgScale, SpriteEffects.None, 0f);

            // Проверяем, есть ли персонаж в словаре по имени
            if (!string.IsNullOrEmpty(current.CharacterImage))
            {
                if (characters.ContainsKey(current.CharacterImage))
                {
                    var character = characters[current.CharacterImage];
                    float scale = 0.7f;
                    float charPosX = ScreenUtils.CenterX(character.Image, spriteBatch.GraphicsDevice, scale);
                    float charPosY = spriteBatch.GraphicsDevice.Viewport.Height - character.Image.Height * scale;
                    Vector2 charPos = new Vector2(charPosX, charPosY);
                    spriteBatch.Draw(character.Image, charPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }

            // Диалог
            if (current != null)
            {
                // Устанавливаем текст для текстбокса
                textBox.Text = dialogManager.GetCurrentLine().Text;
                textBox.Character = dialogManager.GetCurrentLine().Speaker;

                // Центрируем текстбокс по нижнему краю экрана
                textBox.CenterBottom(spriteBatch.GraphicsDevice);

                // Рисуем текстбокс
                textBox.Draw(spriteBatch);
                // Рисуем выборы (если есть)
                if (current.Options != null)
                {
                    for (int i = 0; i < current.Options.Count; i++)
                    {
                        string text = $">>{current.Options[i].Text}";
                        Vector2 pos = textBox.textPos() + new Vector2(20, 50 + i * 30);
                        spriteBatch.DrawString(dialogFont, text, pos, Color.Yellow);
                    }
                }
            }
        }

    }
}
