/*using Newtonsoft.Json;
using System;
using System.IO;

namespace The_Island.Core
{
    public class SaveSystem
    {
        private readonly string _savePath;

        public SaveSystem()
        {
            _savePath = Path.Combine(Environment.CurrentDirectory, "save.json");
        }

        public void Save(SaveData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_savePath, json);
        }

        public SaveData Load()
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            return null;
        }
    }
}*/