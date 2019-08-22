namespace UnitTest.Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class TestCase.
    /// </summary>
    /// <typeparam name="TIn">The type of the t in.</typeparam>
    /// <typeparam name="TOut">The type of the t out.</typeparam>
    public class TestCase<TIn, TOut>
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="TestCase{TIn, TOut}"/> class from being created.
        /// </summary>
        private TestCase()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCase{TIn, TOut}"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expectedOutput">The expected output.</param>
        public TestCase(TIn input, TOut expectedOutput)
        {
            this.Input = input;
            this.ExpectedOutput = expectedOutput;
        }

        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <value>The input.</value>
        public TIn Input { get; }
        /// <summary>
        /// Gets the expected output.
        /// </summary>
        /// <value>The expected output.</value>
        public TOut ExpectedOutput { get; }
    }
}
