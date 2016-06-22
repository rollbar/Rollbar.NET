using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RollbarDotNet.Test 
{
    public class FrameFixture 
    {
        [Fact]
        public void Frame_from_filename_leaves_everything_else_null() 
        {
            var frame = new Frame("ThisFile.cs");
            Assert.Equal("ThisFile.cs", frame.FileName);
            Assert.Null(frame.LineNo);
            Assert.Null(frame.ColNo);
            Assert.Null(frame.Method);
        }

        [Fact]
        public void Frame_from_stackframe_fills_out_everythign() 
        {
            var frame = new Frame(GetFrame());
            Assert.EndsWith("FrameFixture.cs", frame.FileName);
            Assert.NotNull(frame.LineNo);
            Assert.NotNull(frame.ColNo);
            Assert.NotNull(frame.Method);
        }

        [Fact]
        public void Frame_from_stack_frame_serializes_correctly() 
        {
            var frame = new Frame(GetFrame());
            var json = JsonConvert.SerializeObject(frame);
            Assert.Contains(string.Format("\"filename\":\"{0}\"", frame.FileName.Replace("\\", "\\\\")), json);
            Assert.Matches("\"lineno\":\\d+", json);
            Assert.Matches("\"colno\":\\d+", json);
            Assert.Contains("\"method\":\"RollbarDotNet.Test.FrameFixture.GetFrame()\"", json);
        }

        [Fact]
        public void Frame_can_have_code() 
        {
            var frame = new Frame("ThisFile.cs") 
            {
                Code = "        CallThisMethod(arg1, myObject2);",
            };
            Assert.Contains("\"code\":\"        CallThisMethod(arg1, myObject2);\"", JsonConvert.SerializeObject(frame));
        }

        [Fact]
        public void Frame_can_have_context() 
        {
            var frame = new Frame("ThisFile.cs") 
            {
                Code = "        CallThisMethod(arg1, myObject2);",
                Context = new CodeContext 
                {
                    Pre = new[] 
                    {
                        "        var arg1 = new Whatever();",
                        "        var myObject2 = new Whatever();",
                    },
                    Post = new[] 
                    {
                        "        Console.WriteLine(\"Whatever\", arg1);",
                        "    }",
                    },
                }
            };
            var json = JsonConvert.SerializeObject(frame);
            Assert.Contains("\"code\":\"        CallThisMethod(arg1, myObject2);\"", json);
            Assert.Contains("\"context\":{", json);
            Assert.Contains("\"pre\":[", json);
            Assert.Contains("\"post\":[", json);
        }

        [Fact]
        public void Frame_can_have_args() 
        {
            var frame = new Frame("ThisFile.cs") 
            {
                Args = new[] 
                {
                    "1", "\"Test\"", "1.5",
                },
            };
            var json = JsonConvert.SerializeObject(frame);
            Assert.Contains("\"args\":[\"1\",\"\\\"Test\\\"\",\"1.5\"]", json);
            JObject obj = JObject.Parse(json);
            Assert.Equal(obj["args"].Value<JArray>().Select(x => x.Value<string>()), frame.Args);
        }

        [Fact]
        public void Frame_can_have_kwargs() 
        {
            var frame = new Frame("ThisFile.cs") 
            {
                Kwargs = new Dictionary<string, object> 
                {
                    { "One", 1 },
                    { "String", "Hi There" },
                    { "Arr", new object[0] },
                },
            };
            var json = JsonConvert.SerializeObject(frame);
            Assert.Contains("\"kwargs\":{", json);
            Assert.Contains("\"One\":1", json);
            Assert.Contains("\"String\":\"Hi There\"", json);
            Assert.Contains("\"Arr\":[]", json);
        }

        private static StackFrame GetFrame() 
        {
            try 
            {
                throw new InvalidOperationException("I'm afraid I can't do that HAL");
            }
            catch (System.Exception e) 
            {
                return new StackTrace(e, true).GetFrames()[0];
            }
        }
    }
}
