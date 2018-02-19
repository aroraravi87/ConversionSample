

namespace AVSToJVSConversion.Common
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using Model;

    public static class Helpers
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="func"></param>
        public static void ForEachNext<T>(this IList<T> collection, Action<T, T> func)
        {
            for (int i = 0; i < collection.Count - 1; i++)
                func(collection[i], collection[i + 1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSetting(string key, string type)
        {
            return type.Equals("App") ? ConfigurationManager.AppSettings[key] : ConfigurationManager.ConnectionStrings[key].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objSettingModel"></param>
        /// <returns></returns>
        public static bool UpdateAppSettingFile(SettingModel objSettingModel)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
                config.AppSettings.Settings["basePath"].Value = (string.IsNullOrWhiteSpace(objSettingModel.LogPath) ? Helpers.GetSetting("basePath", "App") : objSettingModel.LogPath);
                config.AppSettings.Settings["ExcelPath"].Value = (string.IsNullOrWhiteSpace(objSettingModel.ExcelPath) ? Helpers.GetSetting("ExcelPath", "App") : objSettingModel.ExcelPath);
                config.ConnectionStrings.ConnectionStrings["dbconnection"].ConnectionString = (string.IsNullOrWhiteSpace(objSettingModel.ConnectionPath) ? Helpers.GetSetting("dbconnection", "Web") : objSettingModel.ConnectionPath);
                config.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
          
        }
    }
}
