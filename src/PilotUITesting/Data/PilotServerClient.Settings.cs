using System;
using Ascon.Pilot.DataClasses;
using PilotUITesting.Uia;

namespace PilotUITesting.Data
{
    /// <summary>Общие настройки базы («Управление общими настройками»): чтение и запись значения ключа для оргединицы.</summary>
    public sealed partial class PilotServerClient
    {
        /// <summary>Оргединица общих настроек «для всей базы».</summary>
        public const int WholeDatabase = 0;

        /// <summary>Значение общей настройки <paramref name="key"/>, заданное для оргединицы (не унаследованное); null - не задано.</summary>
        public string GetCommonSetting(string key, int orgUnitId = WholeDatabase)
        {
            RequireKey(key);
            DSettings settings = ServerApi.GetCommonSettings();
            return settings?.CommonSettings != null
                   && settings.CommonSettings.TryGetValue(orgUnitId, out DSettingsCollection collection)
                   && collection?.Values != null
                   && collection.Values.TryGetValue(key, out string value)
                ? value
                : null;
        }

        /// <summary>
        /// Записать общую настройку <paramref name="key"/> для оргединицы (подразделение - для всех его должностей) и дождаться,
        /// что сервер её вернул. value null - убрать настройку. Нужны права администратора базы.
        /// </summary>
        public void SetCommonSetting(string key, string value, int orgUnitId = WholeDatabase, TimeSpan? timeout = null)
        {
            RequireKey(key);
            PilotLog.Action($"общая настройка {key} для оргединицы {orgUnitId}: {(value == null ? "убрать" : "«" + value + "»")}");
            ServerApi.ChangeSettings(new DSettingsChange
            {
                Identity = Guid.NewGuid(),
                OrgUnitId = orgUnitId,
                PersonId = DSettingsChange.UNDEFINED,
                Key = key,
                Value = value,
            });
            string actual = null;
            if (!Wait.TryUntil(() => (actual = GetCommonSetting(key, orgUnitId)) == value, timeout ?? DefaultDataTimeout))
            {
                throw new InvalidOperationException($"Сервер не вернул общую настройку {key} для оргединицы {orgUnitId}: ожидалось {Show(value)}, прочитано {Show(actual)}.");
            }
        }

        private static void RequireKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Нужен ключ настройки", nameof(key));
            }
        }

        private static string Show(string value)
        {
            return value == null ? "«не задана»" : "«" + value + "»";
        }
    }
}
