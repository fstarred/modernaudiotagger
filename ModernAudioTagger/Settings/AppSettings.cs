using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ModernAudioTagger.Settings
{
    public class AppSettings : ApplicationSettingsBase
    {

        [UserScopedSettingAttribute()]
        public string SelectedPath
        {
            get { return (string)(this["SelectedPath"]); }
            set { this["SelectedPath"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("LASTFM")]
        public ViewModel.MusicDbViewModel.DB_SITE DbSite
        {
            get { return (ViewModel.MusicDbViewModel.DB_SITE)(this["DbSite"]); }
            set { this["DbSite"] = value; }
        }

        #region SettingsAppearanceViewModel

        [UserScopedSettingAttribute()]
        public Color SelectedAccentColor
        {
            get
            {
                if (this["SelectedAccentColor"] == null)
                    return AppearanceManager.Current.AccentColor;
                return (Color)(this["SelectedAccentColor"]);
            }
            set { this["SelectedAccentColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("1")]
        public int SelectedThemeIndex
        {
            get { return (int)(this["SelectedThemeIndex"]); }
            set { this["SelectedThemeIndex"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("large")]
        public string SelectedFontSize
        {
            get { return (string)(this["SelectedFontSize"]); }
            set { this["SelectedFontSize"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("INTERNAL")]
        public string OpenMediaMode
        {
            get { return (string)(this["OpenMediaMode"]); }
            set { this["OpenMediaMode"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("")]
        public string ExternalApplicationPath
        {
            get { return (string)(this["ExternalApplicationPath"]); }
            set { this["ExternalApplicationPath"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("%f")]
        public string CommandArguments
        {
            get { return (string)(this["CommandArguments"]); }
            set { this["CommandArguments"] = value; }
        }

        #endregion
    }
}
