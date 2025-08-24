using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Globalization;
using System;
using System.Collections.Generic;

namespace MayhemFamiliar
{
    internal class CardData : IDisposable
    {
        private const string LocColumnName = "Loc";
        private const string RarityColumnName = "Rarity";
        private readonly string _dbFilePath;
        private readonly string _uiCulture;
        private readonly string _localizationsTableName;
        private readonly SQLiteConnection _connection;
        private bool _disposed;

        public CardData(string dbFilePath)
        {
            _dbFilePath = dbFilePath ?? throw new ArgumentNullException(nameof(dbFilePath));
            _uiCulture = CultureInfo.CurrentUICulture.Name.Replace("-", "");
            _localizationsTableName = $"Localizations_{_uiCulture}";
            _connection = new SQLiteConnection($"Data Source={_dbFilePath};Version=3;");
            _connection.Open();
        }

        public List<string> GetAllCardNames()
        {
            List<string> cardNames = new List<string>();
            string sql = $"SELECT DISTINCT l.Loc FROM Cards c JOIN {_localizationsTableName} l ON c.TitleId = l.LocId WHERE l.Formatted = 1";
            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string loc = reader.GetString(0);
                        loc = RemoveBrackets(loc);
                        cardNames.Add(loc);
                    }
                }
            }
            return cardNames;
        }

        public string GetCardNameByGrpId(int grpId)
        {
            string loc = null;
            string sql = $"SELECT l.Loc FROM Cards c JOIN {_localizationsTableName} l ON c.TitleId = l.LocId WHERE c.GrpId = @GrpId AND l.Formatted = 1";

            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@GrpId", grpId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        loc = reader[LocColumnName]?.ToString();
                    }
                }
            }

            if (!String.IsNullOrEmpty(loc))
            {
                loc = RemoveBrackets(loc);
            }

            return loc;
        }

        public string GetCardNameByLocId(int locId)
        {
            string loc = null;

            // 変数4: Localizations_変数2 テーブルから Loc を取得
            string sql = $"SELECT Loc FROM {_localizationsTableName} WHERE LocId = {locId} AND Formatted = 1";
            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        loc = reader[LocColumnName]?.ToString();
                    }
                }
            }

            if (!String.IsNullOrEmpty(loc))
            {
                loc = RemoveBrackets(loc);
            }

            return loc;
        }

        public int GetRarityByGrpId(int grpId)
        {
            string sql = $"SELECT Rarity FROM Cards WHERE GrpId = {grpId}";
            int rarity = -1;
            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rarity = (int)reader[RarityColumnName];
                    }
                }
            }

            return rarity;
        }

        private static string RemoveBrackets(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 半角括弧、全角括弧、山括弧を正規表現でマッチ
            string pattern = @"(\([^\)]*\)|（[^）]*）|<[^>]+>)";
            return Regex.Replace(text, pattern, string.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~CardData()
        {
            Dispose(false);
        }
    }
}
