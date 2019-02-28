using System;
using System.Threading.Tasks;
using Yaapii.Atoms;
using Yaapii.Atoms.Error;

namespace Test.Yaapii.Xive
{
    /// <summary>
    /// A function which is executed in parallel threads.
    /// The result is an indicator of the success of the execution.
    /// It is true when all iterations of the function return true.
    /// <para>An exception will be thrown when a timeout occures.</para>
    /// </summary>
    public sealed class ParallelFunc : IFunc<bool>
    {
        private readonly Func<bool> function;
        private readonly int iterations;
        private readonly int timeout;

        /// <summary>
        /// A function which is executed in parallel threads.
        /// The result is an indicator of the success of the execution.
        /// It is true when all iterations of the function return true.
        /// <para>An exception will be thrown after a timeout of 5s.</para>
        /// </summary>
        /// <param name="function">test function</param>
        /// <param name="count">Number of parallel threads</param>
        /// <exception cref="System.Exception"></exception>
        public ParallelFunc(Func<bool> function) : this(
            function,
            Environment.ProcessorCount << 4,
            5000
        )
        { }

        /// <summary>
        /// A function which is executed in parallel threads.
        /// The result is an indicator of the success of the execution.
        /// It is true when all iterations of the function return true.
        /// <para>An exception will be thrown after a timeout of 5s.</para>
        /// </summary>
        /// <param name="function">test function</param>
        /// <param name="count">Number of parallel threads</param>
        /// <exception cref="System.Exception"></exception>
        public ParallelFunc(Func<bool> function, int iterations) : this(
            function,
            iterations,
            5000
        )
        { }

        /// <summary>
        /// A function which is executed in parallel threads.
        /// The result is an indicator of the success of the execution.
        /// It is true when all iterations of the function return true.
        /// <para>An exception will be thrown after a timeout of 5s.</para>
        /// </summary>
        /// <param name="function">test function</param>
        /// <param name="count">Number of parallel threads</param>
        /// <exception cref="System.Exception"></exception>
        public ParallelFunc(Func<bool> function, int iterations, int millisecondsTimeout)
        {
            this.function = function;
            this.iterations = iterations;
            this.timeout = millisecondsTimeout;
        }

        public bool Invoke()
        {
            var finished = false;
            var failure = false;
            var task =
                Task.Run(() =>
                {
                    Parallel.For(0, this.iterations, (current) =>
                    {
                        if (!this.function.Invoke())
                        {
                            failure = true;
                        }
                    });
                    finished = true;
                });
            task.Wait(this.timeout);

            new FailWhen(() =>
                !finished,
                new TimeoutException($"Timeout after {this.timeout}ms.")
            ).Go();
            return !failure;
        }
    }
}
