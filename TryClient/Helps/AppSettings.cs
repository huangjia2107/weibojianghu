using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TryClient
{
    public class AppSettings
    {
        // Our settings
        public ApplicationDataContainer localSettings;
        StorageFolder localFolder;

        // The key names of our settings
        const string DownloadInternetTypeKeyName = "DownloadInternetType";
        const string DownloadResolutionKeyName = "DownloadResolution";
        const string PlayResolutionKeyName = "PlayResolution";
        const string DataFlushWhileStartKeyName = "DataFlushFrequency";
        const string IsFirstStartKeyName = "IsFirstStart";
        const string MediaPlayOrderKeyName = "MediaPlayOrder";

        // The default value of our settings
        const bool DownloadInternetTypeDefault = false;
        const bool DownloadResolutionDefault = true;
        const bool PlayResolutionDefault = false;
        const bool DataFlushWhileStartDefault = true;
        const bool IsFirstStartDefault = true;
        const bool MediaPlayOrderDefault = true;

        public AppSettings()
        {
            localSettings = ApplicationData.Current.LocalSettings;
            localFolder = ApplicationData.Current.LocalFolder;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (localSettings.Values.ContainsKey(Key))
            {
                // If the value has changed
                if (localSettings.Values[Key] != value)
                {
                    // Store the new value
                    localSettings.Values[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                localSettings.Values.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (localSettings.Values.ContainsKey(Key))
            {
                value = (T)localSettings.Values[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public bool DownloadInternetType
        {
            get
            {
                return GetValueOrDefault<bool>(DownloadInternetTypeKeyName, DownloadInternetTypeDefault);
            }
            set
            {
                AddOrUpdateValue(DownloadInternetTypeKeyName, value);
            }
        }

        public bool DownloadResolution
        {
            get
            {
                return GetValueOrDefault<bool>(DownloadResolutionKeyName, DownloadResolutionDefault);
            }
            set
            {
                AddOrUpdateValue(DownloadResolutionKeyName, value);
            }
        }

        public bool PlayResolution
        {
            get
            {
                return GetValueOrDefault<bool>(PlayResolutionKeyName, PlayResolutionDefault);
            }
            set
            {
                AddOrUpdateValue(PlayResolutionKeyName, value);
            }
        }

        public bool DataFlushWhileStart
        {
            get
            {
                return GetValueOrDefault<bool>(DataFlushWhileStartKeyName, DataFlushWhileStartDefault);
            }
            set
            {
                AddOrUpdateValue(DataFlushWhileStartKeyName, value);
            }
        }

        public bool IsFirstStart
        {
            get
            {
                return GetValueOrDefault<bool>(IsFirstStartKeyName, IsFirstStartDefault);
            }
            set
            {
                AddOrUpdateValue(IsFirstStartKeyName, value);
            }
        }

        public bool MediaPlayOrder
        {
            get
            {
                return GetValueOrDefault<bool>(MediaPlayOrderKeyName, MediaPlayOrderDefault);
            }
            set
            {
                AddOrUpdateValue(MediaPlayOrderKeyName, value);
            }
        }

    }
}
