﻿using Fx.Core.Crypto.Abstract;
using Fx.Core.Crypto.Settings.Settings;
using FX.Core.Common.Settings.Implementations;
using FX.Core.Crypto.Settings.Abstract;
using FX.Core.Crypto.Settings.Models;
using FX.Core.Storage.Serialization.Abstract;
using Microsoft.Extensions.Options;

namespace FX.Core.Crypto.Settings.Implementations
{
    public class BasicEncryptedSettings<TSettings> : BasicSettings<TSettings>, IEncryptedSettings<TSettings>
        where TSettings : class, IEncryptedSettingsObject<TSettings>
    {
        protected readonly IEncryptor _encryptor;
        protected string _key;

        public BasicEncryptedSettings(IDataSerializer dataSaver, 
            IOptions<BasicEncryptedSettingsConfig> options,
            IEncryptor encryptor): base(dataSaver, options)
        {
            _encryptor = encryptor;
        }

        public override void LoadSettings()
        {
            Settings = _dataSaver.GetData<TSettings>(_config.SettingsPath).GetAwaiter().GetResult();
            if (Settings == null)
                throw new System.Exception("Cannot read settings");

            // validate
            Settings.Validate();

            // decrypt
            if (!Settings.IsCorrectKey(_encryptor, _key))
                throw new System.Exception("Cannot decrypt settings, invalid key");

            Settings = Settings.Decrypt(_encryptor, _key);
        }

        public override void SaveSettings()
        {
            if (Settings == null)
                throw new System.Exception("Settings not initialized, cannot save");

            // validate
            Settings.Validate();

            // encrypt
            var encrypted = Settings.Encrypt(_encryptor, _key);
            _dataSaver.SaveDataAsync(encrypted, _config.SettingsPath).GetAwaiter().GetResult();
        }

        public void SetKey(string key)
        {
            _key = key;
        }
    }
}
