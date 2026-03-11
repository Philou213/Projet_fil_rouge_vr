using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherFetcher : MonoBehaviour
{
    [Header("UI Texts")]
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI statusText;

    [Header("Weather Settings")]
    public string cityName = "Chicoutimi";
    public float latitude = 48.43f;
    public float longitude = -71.06f;

    [Header("Scene Effects")]
    public GameObject rain;

    void Start()
    {
        StartCoroutine(GetWeather());
    }

    IEnumerator GetWeather()
    {
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            WeatherData data = JsonUtility.FromJson<WeatherData>(request.downloadHandler.text);
            float temp = data.current_weather.temperature;
            int code = data.current_weather.weathercode;
            tempText.text = temp + "°C";
            statusText.text = GetWeatherDescription(code);
            ApplyWeatherEffects(code);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    void ApplyWeatherEffects(int code)
    {
        bool isRain = code >= 61 && code <= 99;
        rain.SetActive(isRain);
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

[System.Serializable]
public class WeatherData
{
    public CurrentWeather current_weather;
}
[System.Serializable]
public class CurrentWeather
{
    public float temperature;
    public int weathercode;
}