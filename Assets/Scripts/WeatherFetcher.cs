using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class WeatherFetcher : MonoBehaviour
{
    [Header("UI Texts")]
    public TextMeshProUGUI cityText;
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI statusText;

    [Header("Weather Settings")]
    public string cityName = "Chicoutimi";
    public float latitude = 48.43f;
    public float longitude = -71.06f;

    [Header("Scene Effects")]
    public GameObject rainEffect;
    public AudioSource rainAudio;
    public Light weatherLight;

    void Start()
    {
        StartCoroutine(GetWeather());
    }

    IEnumerator GetWeather()
    {
        string url =
            $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,weather_code";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erreur API météo : " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;

            Debug.Log("JSON reçu : " + json);

            float temperature = ExtractFloat(json, "temperature_2m");
            int weatherCode = ExtractInt(json, "weather_code");

            string weatherDesc = GetWeatherDescription(weatherCode);

            cityText.text = cityName;
            tempText.text = temperature.ToString("0") + "°C";
            statusText.text = weatherDesc;

            ApplyWeatherEffects(weatherCode);
        }
    }

    float ExtractFloat(string json, string key)
    {
        int index = json.IndexOf(key);
        if (index == -1) return 0;

        int start = json.IndexOf(":", index) + 1;
        int end = json.IndexOf(",", start);

        string value = json.Substring(start, end - start);

        float.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out float result);

        return result;
    }

    int ExtractInt(string json, string key)
    {
        int index = json.IndexOf(key);
        if (index == -1) return 0;

        int start = json.IndexOf(":", index) + 1;
        int end = json.IndexOf(",", start);

        string value = json.Substring(start, end - start);

        int.TryParse(value, out int result);

        return result;
    }

    void ApplyWeatherEffects(int code)
    {
        bool isRain =
            code == 51 || code == 53 || code == 55 ||
            code == 61 || code == 63 || code == 65 ||
            code == 80 || code == 81 || code == 82;

        if (rainEffect != null)
            rainEffect.SetActive(isRain);

        if (rainAudio != null)
        {
            if (isRain)
            {
                if (!rainAudio.isPlaying)
                    rainAudio.Play();
            }
            else
            {
                rainAudio.Stop();
            }
        }

        if (weatherLight != null)
        {
            if (isRain)
            {
                weatherLight.intensity = 0.8f;
                weatherLight.color = new Color(0.8f, 0.85f, 1f);
            }
            else
            {
                weatherLight.intensity = 1.2f;
                weatherLight.color = Color.white;
            }
        }
    }

    string GetWeatherDescription(int code)
    {
        if (code == 0) return "Ciel clair";
        if (code <= 3) return "Nuageux";
        if (code <= 55) return "Bruine";
        if (code <= 65) return "Pluie";
        if (code <= 75) return "Neige";
        if (code <= 82) return "Averses";
        if (code == 95) return "Orage";

        return "Inconnu";
    }
}