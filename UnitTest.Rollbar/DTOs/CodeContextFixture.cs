namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory("CodeContextFixture")]
    public class CodeContextFixture
    {
        private CodeContext _codeContext;

        [TestInitialize]
        public void SetupFixture()
        {
            this._codeContext = new CodeContext
            {
                Pre = new[]
                {
                    "public CodeContextFixture() {",
                    "this._codeContext = new CodeContext {",
                },
                Post = new[]
                {
                    "};",
                    "}",
                }
            };
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void ContextSerializesReasonably()
        {
            var json = JsonConvert.SerializeObject(_codeContext);
            var jobj = JObject.Parse(json);
            var pre = jobj["pre"];
            var post = jobj["post"];

            Assert.IsInstanceOfType(pre, typeof(JArray));
            Assert.IsInstanceOfType(post, typeof(JArray));

            Assert.AreEqual(this._codeContext.Pre.Length, pre.Values<string>().Count());
            Assert.AreEqual(this._codeContext.Pre.Length, post.Values<string>().Count());

            int i = 0;
            while(i < this._codeContext.Pre.Length)
            {
                Assert.AreEqual(this._codeContext.Pre[i], pre.Values<string>().ToArray()[i]);
                i++;
            }

            int j = 0;
            while (j < this._codeContext.Pre.Length)
            {
                Assert.AreEqual(this._codeContext.Post[j], post.Values<string>().ToArray()[j]);
                j++;
            }
        }
    }
}
