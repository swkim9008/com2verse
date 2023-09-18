using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace Com2Verse.HttpHelper.Tests
{
    public class MultipleDownloadTest
    {
        private static readonly string TestFileUrl = "https://github.com/yourkin/fileupload-fastapi/raw/a85a697cab2f887780b3278059a0dd52847d80f3/tests/data/test-5mb.bin"; //"http://ipv4.download.thinkbroadband.com/10MB.zip";

        // [UnityTest]
        public IEnumerator TestMultipleDownloads()
        {
            if (!EditorUtility.DisplayDialog("안내", "테스트 진행에 많은 시간이 소요됩니다.\n(5MB 파일 10개 다운로드)\n진행 하시겠습니까?", "확인", "취소"))
                yield break;

            var cts = new CancellationTokenSource();
            int testCount = 10;
            int success = 0;
            int failed = 0;
            int done = 0;
            var callbacks = new Callbacks
            {
                OnDownloadStart = async () =>
                {
                    await UniTask.SwitchToMainThread();
                    if (done == testCount)
                        EditorUtility.ClearProgressBar();
                    else
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("다운로드 중", $"({done + 1}/{testCount}) 성공: {success}, 실패: {failed}", done / (float) testCount))
                        {
                            if (cts is {IsCancellationRequested: false})
                            {
                                cts.Cancel();
                                cts.Dispose();
                                cts = null;
                            }

                            EditorUtility.ClearProgressBar();
                        }
                    }

                    await UniTask.SwitchToThreadPool();
                },
                OnDownloadProgress = (read, totalRead, totalSize) =>
                {
                },
                OnComplete = (stream, totalSize) =>
                {
                    success++;
                    C2VDebug.Log($"SUCCESS ({done + 1}/{testCount}), S: {success}, F: {failed}");
                },
                OnFailed = (status) =>
                {
                    failed++;
                    C2VDebug.Log($"FAILED ({done + 1}/{testCount}), S: {success}, F: {failed}");
                },
                OnFinally = () =>
                {
                    done++;
                },
            };

            EditorUtility.ClearProgressBar();
            yield return UniTask.ToCoroutine(async () =>
            {
                var tasks = new List<UniTask>();

                for (var i = 0; i < testCount; ++i)
                {
                    var request = await Client.Request.CreateRequestWithCallbackAsync(HttpRequestBuilder.Generate(new HttpRequestMessageInfo
                    {
                        RequestMethod = Client.eRequestType.GET,
                        Url = TestFileUrl,
                    }), callbacks, cts);

                    tasks.Add(request.SendAsync());
                }
                await UniTask.WhenAll(tasks);

                Assert.IsTrue(done == testCount);
                Assert.IsTrue(success == testCount);
                Assert.IsFalse(failed > 0);
            });
        }
    }
}