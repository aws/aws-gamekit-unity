// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;
using System.IO;

// AWS GameKit
using AWS.GameKit.Common;

// Third Party
using Newtonsoft.Json;

namespace AWS.GameKit.Runtime.Utils
{
    public static class AwsGameKitPersistentSettings
    {
        // Note: Application.dataPath is not available before scriptable objects have initialized
        private static readonly string SETTINGS_FILE_DIRECTORY = GameKitPaths.Get().ASSETS_EDITOR_RESOURCES_FULL_PATH;
        private static readonly string SETTINGS_FILE_PATH = Path.Combine(SETTINGS_FILE_DIRECTORY, "AwsGameKitPersistentSettings.json");

        private static Dictionary<string, string> _settings = new Dictionary<string, string>();

        static AwsGameKitPersistentSettings()
        {
#if UNITY_EDITOR
            if (File.Exists(SETTINGS_FILE_PATH))
            {
                string toImport = File.ReadAllText(SETTINGS_FILE_PATH);
                _settings = JsonConvert.DeserializeObject<Dictionary<string, string>> (toImport);
            }
#endif
        }

        public static void Delete(string key)
        {
            _settings.Remove(key);

            SaveToFile();
        }

        public static void SaveString(string key, string value)
        {
            _settings[key] = value;

            SaveToFile();
        }

        public static void SaveBool(string key, bool value)
        {
            SaveString(key, value.ToString());
        }

        public static void SaveInt(string key, int value)
        {
            SaveString(key, value.ToString());
        }

        public static string LoadString(string key, string defaultValue)
        {
            string value;
            if (_settings.TryGetValue(key, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static bool LoadBool(string key, bool defaultValue)
        {
            return bool.Parse(LoadString(key, defaultValue.ToString()));
        }

        public static int LoadInt(string key, int defaultValue)
        {
            return int.Parse(LoadString(key, defaultValue.ToString()));
        }

        private static void SaveToFile()
        {
#if UNITY_EDITOR
            string toExport = JsonConvert.SerializeObject(_settings, Formatting.Indented);

            if (!Directory.Exists(SETTINGS_FILE_DIRECTORY))
            {
                Directory.CreateDirectory(SETTINGS_FILE_DIRECTORY);
            }

            File.WriteAllText(SETTINGS_FILE_PATH, toExport);
#endif
        }
    }
}
