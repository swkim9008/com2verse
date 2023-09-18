using System;
using Cysharp.Threading.Tasks;

namespace Com2Verse.TTS
{
    public static class TTSSecretary
    {
        public static void TTS(string ment, string audioPath, string audioName, Action callback)
        {
            ProcessAI(ment, audioPath, audioName, callback).Forget();
        }
        
        private static async UniTask ProcessAI(string ment, string audioPath, string audioName, Action callback)
        {
            await Secretary.TTS(ment, audioPath, $"{audioName}.mp3");

            if (callback != null)
                callback();
        }
    }
}