using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;

namespace Com2Verse.TTS
{
    public static class Secretary
    {
        // Reference: https://www.c-sharpcorner.com/article/retrieve-access-token-for-google-service-account-form-json-or-p12-key-in-c-sharp/
        private static async UniTask<string> GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath,
            params string[] scopes)
        {
            return await GoogleCredential
                .FromFile(jsonKeyFilePath) // Loads key file  
                .CreateScoped(scopes) // Gathers scopes requested  
                .UnderlyingCredential // Gets the credentials
                .GetAccessTokenForRequestAsync(); // Gets the Access Token
        }

        public struct LanguageInfo
        {
            public string LanguageCode { get; set; }
            public string VoiceType { get; set; }
            public string Name { get; set; }
            public static LanguageInfo Kor => new LanguageInfo { LanguageCode = "ko-KR", VoiceType = "Wavenet-A", Name = "ko-KR-Wavenet-A" };
            public static LanguageInfo Us => new LanguageInfo { LanguageCode = "en-US", VoiceType = "Wavenet-E",Name = "en-US-Wavenet-E" };
        }

        private static LanguageInfo _currentLanguage = LanguageInfo.Kor;
        public static string CurrentLanuageName => _currentLanguage.Name = $"{_currentLanguage.LanguageCode}-{_currentLanguage.VoiceType}";

        public static void SetCurrentLanguage(CultureInfo info)
        {
            switch (info.Name)
            {
                case "ko-KR":
                    _currentLanguage = LanguageInfo.Kor;
                    break;
                case "en-US":
                    _currentLanguage = LanguageInfo.Us;
                    break;
                default:
                    break;
            }
            
            VoiceInit(_currentLanguage.LanguageCode);
        }

        public static void SetLanguage(string lanuageCode)
        {
            _currentLanguage.LanguageCode = lanuageCode;

            VoiceInit(_currentLanguage.LanguageCode);
        }

        private static void VoiceInit(string country)
        {
            if (country.Equals("ko-KR"))
                _currentLanguage.VoiceType = "Wavenet-A";
            else if(country.Equals("en-US"))
                _currentLanguage.VoiceType = "Wavenet-E";
        }

        public static void SetVoiceType(int type)
        {
            if (_currentLanguage.LanguageCode.Equals("ko-KR"))
            {
                if (type == 1)
                    _currentLanguage.VoiceType = "Wavenet-A";
                else if (type == 2)
                    _currentLanguage.VoiceType = "Wavenet-B";
                else if (type == 3)
                    _currentLanguage.VoiceType = "Wavenet-C";
                else if (type == 4)
                    _currentLanguage.VoiceType = "Wavenet-D";
            }
            else if(_currentLanguage.LanguageCode.Equals("en-US"))
            {
                if (type == 1)
                    _currentLanguage.VoiceType = "Wavenet-E";
                else if (type == 2)
                    _currentLanguage.VoiceType = "Wavenet-F";
                else if (type == 3)
                    _currentLanguage.VoiceType = "Wavenet-A";
                else if (type == 4)
                    _currentLanguage.VoiceType = "Wavenet-B";
            }
        }

        public static async UniTask TTS(string text, string audioPath, string audioName)
        {
            var request = new TTSRequest
            {
                input = new TTSInput
                {
                    text = text
                },
                voice = new TTSVoice
                {
                    languageCode = _currentLanguage.LanguageCode,
                    name = CurrentLanuageName
                },
                audioConfig = new TTSAudioConfig
                {
                    audioEncoding = "MP3"
                }
            };

            try
            {
                var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(request));
                var httpWebRequest =
                    (HttpWebRequest)WebRequest.Create("https://texttospeech.googleapis.com/v1/text:synthesize");
                httpWebRequest.Method = WebRequestMethods.Http.Post;
                var token = await GetAccessTokenFromJSONKeyAsync(
                    Application.streamingAssetsPath + "/com2verse-metaverse-dev-tts.json",
                    "https://www.googleapis.com/auth/cloud-platform");
                httpWebRequest.Headers.Add("Authorization", "Bearer " + token);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.ContentLength = bytes.Length;

                using (var requestStream = httpWebRequest.GetRequestStream())
                {
                    await requestStream.WriteAsync(bytes, 0, bytes.Length);
                    await requestStream.FlushAsync();
                    requestStream.Close();
                }

                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var reader = new StreamReader(httpWebResponse.GetResponseStream());
                var response = JsonUtility.FromJson<TTSResponse>(await reader.ReadToEndAsync());
                var audioBytes = Convert.FromBase64String(response.audioContent);

                if (!Directory.Exists(audioPath))
                {
                    Directory.CreateDirectory(audioPath);
                }

                File.WriteAllBytes($"{audioPath}/{audioName}", audioBytes);
                C2VDebug.Log("Secretary audio file generated");
            }
            catch (Exception e)
            {
                C2VDebug.LogError(e);
            }
        }

        [Serializable]
        public class TTSRequest
        {
            public TTSInput input;
            public TTSVoice voice;
            public TTSAudioConfig audioConfig;
        }

        [Serializable]
        public class TTSInput
        {
            public string text;
        }

        [Serializable]
        public class TTSVoice
        {
            public string languageCode;
            public string name;
        }

        [Serializable]
        public class TTSAudioConfig
        {
            public string audioEncoding;
        }

        [Serializable]
        public class TTSResponse
        {
            public string audioContent;
        }
    }
}
