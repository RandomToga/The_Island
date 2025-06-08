using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using The_Island.Core;
using The_Island.Scenes;

namespace The_Island
{
    public class GameMain : Game
    //Game — встроенный базовый класс из MonoGame: Microsoft.Xna.Framework.Game
    // ссылка на документацию: https://docs.monogame.net/api/Microsoft.Xna.Framework.Game.html
    {
        private GraphicsDeviceManager _graphics; //отвечает за разрешение, полноэкранный режим и т.д.
        private SpriteBatch _spriteBatch; //основной инструмент для отрисовки изображений и текста

        private DialogScene _dialogScene; //экран с диалогами

        private SceneManager sceneManager;

        public GameMain() //Конструктор игры по умолчанию
        {
            _graphics = new GraphicsDeviceManager(this); //инициализация рендера
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; //адаптация по размерам экрана по ширине
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height; //адаптация по размерам экрана по высоте
            _graphics.IsFullScreen = true; //полноэкранный режим
            _graphics.ApplyChanges(); //применить изменения
            Content.RootDirectory = "Content"; //путь, где лежат .xnb файлы (после сборки контента)
            //Content — встроенное свойство Game, представляет ContentManager — менеджер ресурсов
            IsMouseVisible = true; //показывать курсор мыши в игре
        }

        protected override void Initialize()
        {
            sceneManager = new SceneManager(); // Создаём менеджер сцен

            _dialogScene = new DialogScene(); // Создаём сцену
            sceneManager.AddScene("dialog", _dialogScene); // Добавляем её в менеджер

            sceneManager.SwitchTo("dialog"); // Переключаемся на неё

            base.Initialize(); // Вызываем базовый метод
        }

        protected override void LoadContent() //Вызывается один раз для загрузки всех ресурсов
        //Внутри используется ContentManager.Load<T>() — загружает.xnb файлы
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice); // Создаём "кисть" для рисования
            _dialogScene.Load(Content, GraphicsDevice); // Загружаем текстуры и шрифты
        }

        protected override void Update(GameTime gameTime) //Этот Update вызывается системой MonoGame каждый кадр. Вызывает логику сцены.
        //Слово override говорит: "используй мой Update, а не стандартный". переопределение виртуального метода
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // https://docs.monogame.net/api/Microsoft.Xna.Framework.Input.Keyboard.html Выход по ESC
                Exit();

            sceneManager.Update(gameTime); // Обновляем текущую сцену

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); // Очищаем экран
            _spriteBatch.Begin(); // Начинаем рисовать
            sceneManager.Draw(_spriteBatch); // Рисуем текущую сцену
            _spriteBatch.End(); // Заканчиваем рисование

            base.Draw(gameTime); //вызов базовой (родительской) реализации метода Draw() из Game. крч лучше не трогать, пусть будет.
        }
    }
}
