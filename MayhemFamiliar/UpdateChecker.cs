using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MayhemFamiliar
{
    internal class UpdateChecker
    {
        private const string owner = "poslogithub";
        private const string repo = "MayhemFamiliar";
        private static readonly string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
        private static readonly string releasePageUrl = $"https://github.com/{owner}/{repo}/releases/latest";
        
        public static async Task<string> CheckForUpdate()
        {
            Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデート確認開始");
            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimStart('v'); ;
            string latestVersion;
            Logger.Instance.Log($"UpdateChecker: {Application.ProductName} の現在のバージョン: {currentVersion}");

            using (var client = new HttpClient())
            {
                // GitHub APIに必要なヘッダーを設定
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                client.DefaultRequestHeaders.Add("User-Agent", $"{Application.ProductName}/{Assembly.GetExecutingAssembly().GetName().Version.ToString()}"); // GitHub APIはUser-Agentを必須とする
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

                try
                {
                    // APIリクエストを送信
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    // JSONをパースしてtag_nameを取得
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject release = JObject.Parse(jsonResponse);
                    string tagName = release["tag_name"]?.ToString();
                    latestVersion = tagName.TrimStart('v');
                    Logger.Instance.Log($"UpdateChecker: {Application.ProductName} の最新のバージョン: {latestVersion}");

                    if (string.IsNullOrEmpty(tagName))
                    {
                        Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデート確認失敗 - tag_nameが見つかりません");
                        return "";
                    }
                }
                catch (Exception ex) {
                    Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデート確認失敗 - 例外が発生しました: {ex}");
                    return "";
                }

                try
                {
                    Version latest = new Version(latestVersion);
                    Version current = new Version(currentVersion);
                    if (latest > current)
                    {
                        Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデートあり - 最新バージョン: {latestVersion}, 現在のバージョン: {currentVersion}");
                        return releasePageUrl;
                    }
                    else
                    {
                        Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデートなし - 現在のバージョン: {currentVersion}");
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"UpdateChecker: {Application.ProductName} アップデート確認失敗 - 例外が発生しました: {ex}");
                    return "";
                }
            }
        }
    }
}
