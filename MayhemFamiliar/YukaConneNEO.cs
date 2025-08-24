using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class YukaConneNEO
    {
        //private string YukaConneNEOUrl = "http://127.0.0.1:15520/api/input?text=";
        public async Task SendTextToYukarinetAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            try
            {
                using (var client = new HttpClient())
                {
                    var url = $"http://127.0.0.1:{Program._config.YukaConneNEOSettings.Port}/api/input?text={Uri.EscapeDataString(text)}";
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Log($"{typeof(YukaConneNEO).Name}: ゆかコネNEOがエラー応答: {url} {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{typeof(YukaConneNEO).Name}: ゆかコネNEOにテキストを送信失敗: {text} {ex.Message}");
            }
        }

    }
}
