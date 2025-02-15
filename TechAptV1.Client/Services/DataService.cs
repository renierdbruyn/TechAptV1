﻿// Copyright © 2025 Always Active Technologies PTY Ltd

using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using TechAptV1.Client.Models;

namespace TechAptV1.Client.Services;

/// <summary>
/// Data Access Service for interfacing with the SQLite Database
/// </summary>
public sealed class DataService
{
    private readonly ILogger<DataService> _logger;
    private readonly IConfiguration _configuration;
    private static SQLiteConnection sqlite;

    private bool _isSaving = false;

    /// <summary>
    /// Default constructor providing DI Logger and Configuration
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    public DataService(ILogger<DataService> logger, IConfiguration configuration)
    {
        this._logger = logger;
        this._configuration = configuration;
        var connectionString = new SqliteConnectionStringBuilder(configuration.GetConnectionString("Default")).ToString();
        sqlite = new SQLiteConnection(connectionString);
    }

    public bool IsSaving() => _isSaving;

    /// <summary>
    /// Save the list of data to the SQLite Database
    /// </summary>
    /// <param name="dataList"></param>
    public async Task Save(List<Number> dataList)
    {
        sqlite.Open();
        try
        {
            using (var transaction = sqlite.BeginTransaction())
            {
                SQLiteCommand dbCommand = new SQLiteCommand(sqlite);
                dbCommand.CommandText = "INSERT INTO Number (Value, IsPrime) VALUES(@number, @isPrime)";
                //Define parameters for reuse.
                var numberParameter = dbCommand.CreateParameter();
                numberParameter.ParameterName = "@number";
                var isPrimeParameter = dbCommand.CreateParameter();
                isPrimeParameter.ParameterName = "@isPrime";
                dbCommand.Parameters.Add(numberParameter);
                dbCommand.Parameters.Add(isPrimeParameter);

                foreach (var number in dataList)
                {
                    numberParameter.Value = number.Value;
                    isPrimeParameter.Value = number.IsPrime;
                    dbCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            sqlite.Close();
        }
        catch (Exception ex)
        {
            sqlite.Close();
        }

    }

    /// <summary>
    /// Fetch N records from the SQLite Database where N is specified by the count parameter
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Number>> GetAsync(int count)
    {
        this._logger.LogInformation("Get");
        var numberList = new List<Number>();
        try
        {

            sqlite.Open();
            SQLiteCommand dbCommand = new SQLiteCommand(sqlite);
            dbCommand.CommandText = "Select * from Number limit @limit";
            dbCommand.Parameters.AddWithValue("@limit", count);

            // populate numberList
            await ExtractNumberValues(numberList, dbCommand, _logger);

            sqlite.Close();

        }
        catch (Exception ex)
        {

        }
        finally
        {
            sqlite.Close();
        }
        return numberList;

    }

    /// <summary>
    /// Fetch All the records from the SQLite Database
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Number>> GetAllAsync()
    {
        this._logger.LogInformation("GetAll");
        var numberList = new List<Number>();
        try
        {
            sqlite.Open();
            SQLiteCommand dbCommand = new SQLiteCommand(sqlite);
            dbCommand.CommandText = "Select * from Number";

            await ExtractNumberValues(numberList, dbCommand, _logger);

            sqlite.Close();

            //return new List<Number>() { new() { Value = 1, IsPrime = false } };
        }
        catch (Exception ex)
        {

        }
        finally
        {
            sqlite.Close();
        }
        return numberList;
    }
    /// <summary>
    /// Populates the List of Numbers
    /// </summary>
    /// <param name="numberList"></param>
    /// <param name="dbCommand"></param>
    private static async Task ExtractNumberValues(List<Number> numberList, SQLiteCommand dbCommand, ILogger<DataService> logger)
    {
        using (SQLiteDataReader reader = dbCommand.ExecuteReader())
        {
            while (await reader.ReadAsync())
            {
                numberList.Add(new() { Value = reader.GetInt32(0), IsPrime = reader.GetBoolean(1) });
            }
        }
    }


}
