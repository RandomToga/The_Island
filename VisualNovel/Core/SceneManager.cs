using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using The_Island.Scenes;

namespace The_Island.Core
{
    public class SceneManager
    {
        private Dictionary<string, IScene> _scenes = new Dictionary<string, IScene>();
        private IScene _currentScene;

        public void AddScene(string name, IScene scene)
        {
            _scenes[name] = scene;
        }

        public void SwitchTo(string name)
        {
            if (_scenes.ContainsKey(name))
                _currentScene = _scenes[name];
        }

        public void Update(GameTime gameTime)
        {
            _currentScene?.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(spriteBatch);
        }
    }
}
