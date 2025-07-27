using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Globalization;

namespace MayhemFamiliar
{
    internal class CardData : IDisposable
    {
        private readonly string _dbFilePath;
        private readonly string _uiCulture;
        private readonly SQLiteConnection _connection;
        private bool _disposed;

        public CardData(string dbFilePath)
        {
            _dbFilePath = dbFilePath ?? throw new ArgumentNullException(nameof(dbFilePath));
            _uiCulture = CultureInfo.CurrentUICulture.Name.Replace("-", "");
            _connection = new SQLiteConnection($"Data Source={_dbFilePath};Version=3;");
            _connection.Open();
        }

        public string? GetCardNameByGrpId(int grpId)
        {
            string? loc = null;
            string tableName = $"Localizations_{_uiCulture}";
            string locColumnName = "Loc";
            string sql = $"SELECT l.Loc FROM Cards c JOIN {tableName} l ON c.TitleId = l.LocId WHERE c.GrpId = @GrpId AND l.Formatted = 1";

            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@GrpId", grpId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        loc = reader[locColumnName]?.ToString();
                    }
                }
            }

            if (loc is not null)
            {
                loc = RemoveBrackets(loc);
            }

            return loc;
        }

        public string? GetCardNameByLocId(int locId)
        {
            string? loc = null;

            // 変数4: Localizations_変数2 テーブルから Loc を取得
            string tableName = $"Localizations_{_uiCulture}";
            string sql = $"SELECT Loc FROM {tableName} WHERE LocId = @LocId AND Formatted = 1";
            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@LocId", locId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        loc = reader["Loc"]?.ToString();
                    }
                }
            }

            if (loc is not null)
            {
                loc = RemoveBrackets(loc);
            }

            return loc;
        }

        private static string RemoveBrackets(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 半角括弧、全角括弧、<～>を正規表現でマッチ
            string pattern = @"(\([^\)]*\)|（[^）]*）)";
            return Regex.Replace(text, pattern, string.Empty);
        }
        private static string RemoveTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // <～> を正規表現でマッチ
            string pattern = @"<[^>]*>";
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
