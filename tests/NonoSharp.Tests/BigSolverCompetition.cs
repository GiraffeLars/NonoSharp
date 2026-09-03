using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace NonoSharp.Tests
{
    public class SolverFixture
    {
        private Random rng;
        internal readonly Grid[] grids = new Grid[5];
        public readonly bool[] baseCases = new bool[5];
        public int totalSolvable;

        public SolverFixture() 
        {
            rng = new();
            for (int i = 0; i < baseCases.Length; i++)
            {
                bool res = CreateRandomResult(i);
                baseCases[i] = res;
                if (res)
                {
                    totalSolvable++;
                }
            }
        }

        private bool CreateRandomResult(int iter)
        {
            int width = 20;
            int height = 20;
            HashSet<CellPosition> sol = [];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // Random between guaranteed to add to 1 in 10
                    //int fillRate = Random.Shared.Next(0, 9);
                    if (rng.Next(3) != 0)
                    {
                        sol.Add(new(i, j));
                    }
                }
            }

            Grid g = new(width, height, sol);
            grids[iter] = g;
            return Solver.IsSolvable(g, new UniqueQueueStrategy());
        }
    }

    public class BigSolverCompetition : IClassFixture<SolverFixture>
    {
        SolverFixture fixture;
        private readonly ITestOutputHelper _output;

        public BigSolverCompetition(SolverFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task PerformanceImmediateDFSTest()
        {
            int iterations = fixture.baseCases.Length;
            bool[] results = new bool[iterations];

            var tasks = Enumerable.Range(0, iterations)
                .Select(async iteration =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    await Task.Run(() =>
                    {
                        bool res = Solver.IsSolvable(
                            fixture.grids[iteration], new ImmediateDFSStrategy());
                        results[iteration] = res;
                    }
                    );
                    stopwatch.Stop();
                    Assert.Equal(fixture.baseCases[iteration], results[iteration]);
                    return stopwatch.Elapsed;
                });

            var timings = await Task.WhenAll(tasks);
            var average = timings.Average(x => x.TotalSeconds);
            _output.WriteLine($"Immediate average {average}");
            _output.WriteLine($"Total solvable: {fixture.totalSolvable}");
        }

        [Fact]
        public async Task PerformanceCompleteLineTest()
        {
            int iterations = fixture.baseCases.Length;
            bool[] results = new bool[iterations];

            var tasks = Enumerable.Range(0, iterations)
                .Select(async iteration =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    await Task.Run(() =>
                    {
                        bool res = Solver.IsSolvable(
                            fixture.grids[iteration], new UniqueQueueStrategy());
                        results[iteration] = res;
                    }
                    );
                    stopwatch.Stop();
                    Assert.Equal(fixture.baseCases[iteration], results[iteration]);
                    return stopwatch.Elapsed;
                });

            var timings = await Task.WhenAll(tasks);
            var average = timings.Average(x => x.TotalSeconds);
            _output.WriteLine($"Average complete line {average}");
            _output.WriteLine($"Total solvable: {fixture.totalSolvable}");
        }

        [Fact]
        public async Task PerformanceBaseTest()
        { 
            int iterations = fixture.baseCases.Length;
            bool[] results = new bool[iterations];

            var tasks = Enumerable.Range(0, iterations)
                .Select(async iteration =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    await Task.Run(() =>
                    {
                        bool res = Solver.IsSolvable(
                            fixture.grids[iteration], new OldSolverStrategy());
                        results[iteration] = res;
                    }
                    );
                    stopwatch.Stop();
                    Assert.Equal(fixture.baseCases[iteration], results[iteration]);
                    return stopwatch.Elapsed;
                });

            var timings = await Task.WhenAll(tasks);
            var average = timings.Average(x => x.TotalSeconds);
            _output.WriteLine($"Average base {average}");
        }
    }
}
