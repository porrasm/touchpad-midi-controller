using RawInput.Touchpad.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInput.Touchpad {
    public class ApplicationState {
        public string selectedMidiDevice = "None";
        public string selectedConfiguration = "None";
        public Dictionary<string, TouchpadConfig> configurations = new();


        private const string CONFIGURATIONS_FILE = "./configurations.json";

        public static ApplicationState Instance { get; private set; }

        public static Action OnConfigChanged { get; set; }

        public static ApplicationState GetDefaultState() {
            return new ApplicationState() {
                selectedConfiguration = TouchpadConfigs.DefaultConfig().name,
                configurations = TouchpadConfigs.GetDefaultConfigs().ToDictionary(c => c.name, c => c),
            };
        }

        private string GetUniqueConfigName() {
            int i = 1;

            while (true) {
                string name = "New Configuration " + i;

                if (!configurations.ContainsKey(name)) {
                    return name;
                }

                i++;
            }
        }

        public void AddConfig(TouchpadConfig config) {
            config.name = GetUniqueConfigName();
            configurations.Add(config.name, config);
            OnConfigChanged?.Invoke();
            selectedConfiguration = config.name;
        }

        public void RemoveConfig(string name, bool addDefault = true) {
            if (configurations.ContainsKey(name)) {
                configurations.Remove(name);
                OnConfigChanged?.Invoke();
            }

            if (configurations.Count == 0 && addDefault) {
                var defaultState = GetDefaultState();
                configurations = defaultState.configurations;
                selectedConfiguration = defaultState.selectedConfiguration;
            }

            if (configurations.Count > 0) {
                selectedConfiguration = configurations.First().Key;
            } else {
                selectedConfiguration = "None";
            }
        }

        public void EditConfig(string prevName, TouchpadConfig config) {
            RemoveConfig(prevName, false);

            configurations.Add(config.name, config);
            OnConfigChanged?.Invoke();
            selectedConfiguration = config.name;
        }

        public static ApplicationState Load() {
            try {
                if (System.IO.File.Exists(CONFIGURATIONS_FILE)) {
                    string json = System.IO.File.ReadAllText(CONFIGURATIONS_FILE);
                    Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
                    settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                    settings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationState>(json, settings);
                } else {
                    Instance = GetDefaultState();
                }

                return Instance;
            } catch (Exception e) {
                Console.WriteLine("Error loading the application state: " + e.Message);
                Instance = GetDefaultState();
            }

            OnConfigChanged?.Invoke();
            return Instance;
        }

        public static void Save(ApplicationState state = null) {
            Instance = state ?? Instance ?? GetDefaultState();

            try {
                Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                settings.Formatting = Newtonsoft.Json.Formatting.Indented;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(Instance, settings);
                System.IO.File.WriteAllText(CONFIGURATIONS_FILE, json);
            } catch (Exception e) {
                Console.WriteLine("Error saving the application state: " + e.Message);
            }

            OnConfigChanged?.Invoke();
        }

        public TouchpadConfig GetSelectedConfig() {
            if (!configurations.ContainsKey(selectedConfiguration)) {
                var config = TouchpadConfigs.DefaultConfig();
                selectedConfiguration = config.name;
                return config;
            }

            return configurations[selectedConfiguration];
        }
    }
}
