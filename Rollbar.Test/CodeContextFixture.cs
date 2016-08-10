using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test 
{
    public class CodeContextFixture 
    {
        private CodeContext _codeContext;

        public CodeContextFixture() 
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

        [Fact]
        public void Context_serializes_reasonably() 
        {
            var json = JsonConvert.SerializeObject(_codeContext);
            var jobj = JObject.Parse(json);
            var pre = Assert.IsType<JArray>(jobj["pre"]).Select(j => j.Value<string>()).ToArray();
            Assert.Equal(2, pre.Length);
            Assert.Equal(_codeContext.Pre, pre);
            var post = Assert.IsType<JArray>(jobj["post"]).Select(j => j.Value<string>()).ToArray();
            Assert.Equal(2, post.Length);
            Assert.Equal(_codeContext.Post, post);
        }
    }
}
