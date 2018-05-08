#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.DTOs
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory(nameof(FrameFixture))]
    public class FrameFixture
    {
        [TestMethod]
        public void FrameFromFilenameLeavesEverythingElseNull()
        {
            var frame = new Frame("ThisFile.cs");
            Assert.AreEqual("ThisFile.cs", frame.FileName);
            Assert.IsNull(frame.LineNo);
            Assert.IsNull(frame.ColNo);
            Assert.IsNull(frame.Method);
        }

        [TestMethod]
        public void FrameFromStackFrameFillsOutEverythign()
        {
            var frame = new Frame(GetFrame());
            Assert.IsTrue(frame.FileName.EndsWith("FrameFixture.cs") || frame.FileName.EndsWith("FrameFixture"));
            Assert.IsNotNull(frame.LineNo);
            Assert.IsNotNull(frame.ColNo);
            Assert.IsNotNull(frame.Method);
        }

        [TestMethod]
        public void FrameFromStackFrameSerializesCorrectly()
        {
            var frame = new Frame(GetFrame());
            var json = JsonConvert.SerializeObject(frame);
            Assert.IsTrue(json.Contains(string.Format("\"filename\":\"{0}\"", frame.FileName.Replace("\\", "\\\\"))));
            Assert.IsTrue(Regex.IsMatch(json, "\"lineno\":\\d+"));
            Assert.IsTrue(Regex.IsMatch(json, "\"colno\":\\d+"));
            Assert.IsTrue(json.Contains("\"method\":\"UnitTest.Rollbar.DTOs.FrameFixture.GetFrame()\""));
        }

        [TestMethod]
        public void FrameCanHaveCode()
        {
            var frame = new Frame("ThisFile.cs")
            {
                Code = "        CallThisMethod(arg1, myObject2);",
            };
            Assert.IsTrue(JsonConvert.SerializeObject(frame).Contains("\"code\":\"        CallThisMethod(arg1, myObject2);\""));
        }

        [TestMethod]
        public void FrameCanHaveContext()
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
            Assert.IsTrue(json.Contains("\"code\":\"        CallThisMethod(arg1, myObject2);\""));
            Assert.IsTrue(json.Contains("\"context\":{"));
            Assert.IsTrue(json.Contains("\"pre\":["));
            Assert.IsTrue(json.Contains("\"post\":["));
        }

        [TestMethod]
        public void FrameCanHaveArgs()
        {
            var frame = new Frame("ThisFile.cs")
            {
                Args = new[]
                {
                    "1", "\"Test\"", "1.5",
                },
            };
            var json = JsonConvert.SerializeObject(frame);
            Assert.IsTrue(json.Contains("\"args\":[\"1\",\"\\\"Test\\\"\",\"1.5\"]"));
            JObject obj = JObject.Parse(json);
            var args = obj["args"].Value<JArray>().Select(x => x.Value<string>()).ToArray();
            int i = 0;
            while (i < frame.Args.Length)
            {
                Assert.AreEqual(args[i], frame.Args[i]);
                i++;
            }
        }

        [TestMethod]
        public void FrameCanHaveKwargs()
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
            Assert.IsTrue(json.Contains("\"kwargs\":{"));
            Assert.IsTrue(json.Contains("\"One\":1"));
            Assert.IsTrue(json.Contains("\"String\":\"Hi There\""));
            Assert.IsTrue(json.Contains("\"Arr\":[]"));
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
