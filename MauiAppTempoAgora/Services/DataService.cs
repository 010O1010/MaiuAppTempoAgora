using MauiAppTempoAgora.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MauiAppTempoAgora.Services
{
    public class DataService
    {
        private const string API_KEY = "6135072afe7f6cec1537d5cb08a5a1a2";
        private const string BASE_URL = "https://api.openweathermap.org/data/2.5/weather";

        public static async Task<Tempo?> GetPrevisao(string cidade)
        {
            try
            {
                string url = $"{BASE_URL}?q={cidade}&units=metric&appid={API_KEY}";

                using (HttpClient client = new HttpClient())
                {
                    // Verificar conexão com a internet
                    if (!await CheckInternetConnectionAsync(client))
                    {
                        throw new HttpRequestException("Sem conexão com a internet.");
                    }

                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verificar o status code da resposta
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new HttpRequestException($"Cidade '{cidade}' não encontrada.");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Erro ao acessar a API: {response.StatusCode}");
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var rascunho = JObject.Parse(json);

                    // Converter os timestamps para DateTime
                    DateTime time = DateTime.UnixEpoch;
                    DateTime sunrise = time.AddSeconds((double)rascunho["sys"]["sunrise"]).ToLocalTime();
                    DateTime sunset = time.AddSeconds((double)rascunho["sys"]["sunset"]).ToLocalTime();

                    return new Tempo
                    {
                        lat = (double)rascunho["coord"]["lat"],
                        lon = (double)rascunho["coord"]["lon"],
                        description = (string)rascunho["weather"][0]["description"],
                        main = (string)rascunho["weather"][0]["main"],
                        temp_min = (double)rascunho["main"]["temp_min"],
                        temp_max = (double)rascunho["main"]["temp_max"],
                        speed = (double)rascunho["wind"]["speed"],
                        visibility = (int)rascunho["visibility"],
                        sunrise = sunrise.ToString("HH:mm"),
                        sunset = sunset.ToString("HH:mm"),
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                await HandleHttpRequestException(ex);
                return null;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
                return null;
            }
        }

        private static async Task<bool> CheckInternetConnectionAsync(HttpClient httpClient)
        {
            try
            {
                using var response = await httpClient.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static async Task HandleHttpRequestException(HttpRequestException ex)
        {
            if (ex.Message.Contains("Sem conexão com a internet"))
            {
                await App.Current.MainPage.DisplayAlert("Erro de Conexão", "Você está sem conexão com a internet. Por favor, verifique sua rede.", "OK");
            }
            else if (ex.Message.Contains("não encontrada"))
            {
                await App.Current.MainPage.DisplayAlert("Cidade Não Encontrada", ex.Message, "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Erro na API", ex.Message, "OK");
            }
        }
    }
}