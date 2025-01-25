// Copyright © 2025 Always Active Technologies PTY Ltd

using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using TechAptV1.Client.Models;

namespace TechAptV1.Client.Services;

/// <summary>
/// Default constructor providing DI Logger and Data Service
/// </summary>
/// <param name="logger"></param>
/// <param name="dataService"></param>
public sealed class ThreadingService(ILogger<ThreadingService> logger, DataService dataService)
{
    private int _oddNumbers = 0;
    private int _evenNumbers = 0;
    private int _primeNumbers = 0;
    private int _totalNumbers = 0;

    public int _stopLimit = 200_000; //10_000_000
    public int _evenStart = 50_000; //2_500_000
    public bool _startedProcessing = false;
    [CascadingParameter]
    public List<Number> _numbersList { get; set; } = new();
    //private List<int> _numbersList { get; set; } = new();

    [ThreadStatic]
    private static Random _random = new();

    private readonly object _lock = new();

    public int GetOddNumbers() => _oddNumbers;
    public int GetEvenNumbers() => _evenNumbers;
    public int GetPrimeNumbers() => _primeNumbers;
    public int GetTotalNumbers() => _totalNumbers;
    public int GettopLimit() => _stopLimit;

    /// <summary>
    /// Start the random number generation process
    /// </summary>
    public async Task Start()
    {
        logger.LogInformation("Start");
        _startedProcessing = true;
        var OddRng = new Thread(new ThreadStart(GenerateRandomOddNumbers));
        var PrimeRng = new Thread(new ThreadStart(GeneratePrimeNumbers));
        ThreadPool.QueueUserWorkItem((state) =>
        {

            OddRng.Start();
            PrimeRng.Start();

            while (true)
            {
                lock (_lock)
                {
                    if (_numbersList.Count >= _evenStart)
                    {
                        break;
                    }
                }
            }
            logger.LogDebug($"_numbersList.Count: {_numbersList.Count}");
            var EvenRng = new Thread(new ThreadStart(GenerateRandomEvenNumbers));
            EvenRng.Start();

            OddRng.Join();
            PrimeRng.Join();
            EvenRng.Join();
            logger.LogDebug($"_numbersList.Count: {_numbersList.Count}");
        });

        SortNumberList();
        return;
    }

    private void SortNumberList()
    {
        _numbersList.OrderBy(x => x.Value);
    }
    private static void EnsureRandomInstantiated()
    {
        if (_random == null)
        {
            _random = new Random();
        }
    }

    /// <summary>
    /// Persist the results to the SQLite database
    /// </summary>
    public async Task Save()
    {
        logger.LogInformation("Save");
        throw new NotImplementedException();
    }

    private void GenerateRandomOddNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        //while (_numbersList.Length < _stopLimit)
        {
            lock (_lock)
            {
                //Random random = new Random();
                int ans = _random.Next(_stopLimit);
                if (ans % 2 == 1)
                {
                    _numbersList.Add(new() { Value = ans, IsPrime = IsPrime(ans) });
                    //_numbersList.Add(ans);
                    _oddNumbers++;
                    _totalNumbers++;
                }
            }
        }
    }

    private void GenerateRandomEvenNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        //while (_numbersList.Length < _stopLimit)
        {
            lock (_lock)
            {
                //Random random = new Random();
                int ans = _random.Next(_stopLimit);
                if (ans % 2 == 0)
                {
                    _numbersList.Add(new() { Value = ans, IsPrime = IsPrime(ans) });
                    //_numbersList.Add(ans);
                    _evenNumbers++;
                    _totalNumbers++;
                }
            }
        }
    }

    private void GeneratePrimeNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        //while (_numbersList.Length < _stopLimit)
        {
            lock (_lock)
            {
                //Random random = new Random();
                int ans = _random.Next(_stopLimit);
                if (IsPrime(ans))
                {
                    _numbersList.Add(new() { Value = ans, IsPrime = true });
                    //_numbersList.Add(ans);
                    _primeNumbers++;
                    _totalNumbers++;
                }
            }
        }
    }

    private static bool IsPrime(int n)
    {
        if (n > 1)
        {
            return Enumerable.Range(1, n).Where(x => n % x == 0)
                             .SequenceEqual(new[] { 1, n });
        }
        return false;
    }
}
