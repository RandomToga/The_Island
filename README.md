# The Island 

**The Island** — это визуальная новелла, созданная на фреймворке [MonoGame](https://www.monogame.net/). Игра сочетает в себе повествовательные элементы, выбор игрока и уникальную атмосферу, вдохновлённую классическими и современными VN-проектами.

## Описание

Вы оказываетесь на загадочном острове, полном тайн, решений и неожиданных последствий. Игроку предстоит исследовать сюжет, взаимодействовать с персонажами и делать выборы, влияющие на развитие истории.

## Технологии

- **Язык:** C#
- **Фреймворк:** MonoGame
- **Жанр:** Визуальная новелла
- **Платформы:** Windows

## Структура проекта
  
The_Island/  
├── Content/  
│   ├── Content.mgcb  
│   ├── Data/  
│    │   └── dialogs.json  
│   ├── Fonts/  
│    │   └── Font.spritefont  
│   └── Images/  
│        ├── Backgrounds  
│        |   ├── camp_night.jpg  
│        |   ├── cockpit_red.png  
│        |   └── hangar_dark.jpg  
│        ├── Characters  
│        |   ├── aaron_default.png  
│        |   ├── ada_default.png  
│        |   ├── alex_default.png  
│        |   ├── author_default.png  
│        |   ├── iris_default.png  
│        |   └── mira_default.png  
│        └── UI  
│            └──button_menu.png  
│  
├── Core/  
│   ├── Character.cs  
│   ├── DialogManager.cs  
│   ├── SceneManager.cs  
│   └── ScreenUtils.cs  
│  
├── Scenes/  
│   ├── IScene.cs  
│   └── DialogScene.cs  
│  
├── UI/Elements  
│   └── TextBox.cs  
│  
├── Program.cs  
├── GameMain.cs  
├── The_Island.csproj  
└── README.md  

## Скриншоты из игры
<img width="1433" height="896" alt="image" src="https://github.com/user-attachments/assets/7dbe6d26-42eb-4506-93ea-a0bc7a507bb7" />

<img width="1428" height="892" alt="mira_game" src="https://github.com/user-attachments/assets/c5904a17-70f0-462c-840c-e6e3676e081e" />
