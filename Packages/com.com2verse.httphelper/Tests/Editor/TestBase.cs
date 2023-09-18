using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Com2Verse.HttpHelper.Tests
{
    public class TestBase
    {
        protected async UniTask RunTestAsync(Func<UniTask> test)
        {
            try
            {
                await test.Invoke();
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }
    }
}
