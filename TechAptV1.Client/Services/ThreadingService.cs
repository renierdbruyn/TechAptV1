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

    public int _stopLimit = 10_000; //10_000_000
    public int _evenStart = 2_500; //2_500_000
    public bool _startedProcessing = false;
    [CascadingParameter]
    private List<Number> _numbersList { get; set; } = new();

    [ThreadStatic]
    private static Random _random = new();

    private readonly object _lock = new();
    private static Mutex mut = new Mutex();

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
                if (_numbersList.Count >= _evenStart)
                {
                    logger.LogInformation($"Even Numbers has started generating");
                    logger.LogDebug($"_numbersList.Count: {_numbersList.Count}");
                    break;
                }
            }
            var EvenRng = new Thread(new ThreadStart(GenerateRandomEvenNumbers));
            EvenRng.Start();

            OddRng.Join();
            PrimeRng.Join();
            EvenRng.Join();
            logger.LogDebug($"_numbersList.Count: {_numbersList.Count}");
            SortNumberList();
        });

        return;
    }

    private void SortNumberList()
    {
        _numbersList = _numbersList.OrderBy(x => x.Value).ToList();
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
        await dataService.Save(_numbersList);
    }

    /// <summary>
    /// A method that generates Odd Numbers
    /// </summary>
    private void GenerateRandomOddNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        {
            mut.WaitOne();
            try
            {
                if (_numbersList.Count >= _stopLimit) { break; }
                int randomNumber = _random.Next(_stopLimit / 2) * 2 + 1;
                _numbersList.Add(new() { Value = randomNumber, IsPrime = IsPrime(randomNumber) });
                _oddNumbers++;
                _totalNumbers++;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }
    }

    private void GenerateRandomEvenNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        {
            mut.WaitOne();
            try
            {
                if (_numbersList.Count >= _stopLimit) { break; }

                int randomNumber = _random.Next(_stopLimit / 2) * 2;
                _numbersList.Add(new() { Value = randomNumber, IsPrime = IsPrime(randomNumber) });
                _evenNumbers++;
                _totalNumbers++;
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }
    }

    private void GeneratePrimeNumbers()
    {
        EnsureRandomInstantiated();

        while (_numbersList.Count < _stopLimit)
        {
            mut.WaitOne();
            try
            {
                if (_numbersList.Count >= _stopLimit) { break; }
                int randomNumber = _random.Next(_stopLimit);
                if (IsPrime(randomNumber))
                {
                    _numbersList.Add(new() { Value = randomNumber, IsPrime = true });
                    _primeNumbers++;
                    _totalNumbers++;
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }
    }

    private static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2 || number == 3 || number == 5) return true;
        if (number % 2 == 0 || number % 3 == 0 || number % 5 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));

        // You can do less work by observing that at this point, all primes 
        // other than 2 and 3 leave a remainder of either 1 or 5 when divided by 6. 
        // The other possible remainders have been taken care of.
        int i = 6; // start from 6, since others below have been handled.
        while (i <= boundary)
        {
            if (number % (i + 1) == 0 || number % (i + 5) == 0)
            {
                return false;
            }
            i += 6;
        }

        return true;
    }
}
