﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Verification.Infrastructure
{
    public interface IVerificationService
    {
        Task<string> GetVerificationDataCsv(short academicYear, byte collectionPeriod, bool populateEarnings,
            DateTime startDateTime, DateTime endDateTime);

        Task<decimal?> GetTheNumber(short academicYear, byte collectionPeriod, bool populateEarnings,
            DateTime startDateTime, DateTime endDateTime);

        Task<string> GetDataStoreCsv(short academicYear, byte collectionPeriod, List<long> ukprns);

        Task<DateTimeOffset?> GetLastActivityDate(List<long> ukprns);
    }

    public class VerificationService : IVerificationService
    {
        private readonly Configuration configuration;

        public VerificationService(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> GetVerificationDataCsv(short academicYear, byte collectionPeriod, bool populateEarnings,
            DateTime startDateTime, DateTime endDateTime)
        {
            var sql = Scripts.ScriptHelpers.GetSqlScriptText("VerificationQuery.sql");
            sql += " order by 1,2";
            using (SqlConnection connection = GetPaymentsConnectionString())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = TimeSpan.FromSeconds(120).Seconds;
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@academicYear", academicYear);
                    cmd.Parameters.AddWithValue("@collectionPeriod", collectionPeriod);
                    cmd.Parameters.AddWithValue("@populateEarnings", populateEarnings);
                    cmd.Parameters.AddWithValue("@StartTime", startDateTime);
                    cmd.Parameters.AddWithValue("@EndTime", endDateTime);
                    connection.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        return CreateCsvFromDataReader(reader);
                    }
                }
            }
        }

        public async Task<string> GetDataStoreCsv(short academicYear, byte collectionPeriod, List<long> ukprns)
        {
            var sql = Scripts.ScriptHelpers.GetSqlScriptText("DataStoreQuery.sql");
            sql = sql.Replace("<<ukprnList>>",string.Join(",", ukprns));
           
            using (SqlConnection connection = GetDataStoreConnectionString(academicYear))
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = TimeSpan.FromSeconds(120).Seconds;
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@collectionPeriod", collectionPeriod);
                    
                    connection.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        return CreateCsvFromDataReader(reader);
                    }
                }
            }
        }


        public async Task<DateTimeOffset?> GetLastActivityDate(List<long> ukprns)
        {
            var ukprnsString = String.Join(",", ukprns);

            String sql = $@"select max(CreationDate) from (
						select ukprn, CreationDate from Payments2.EarningEvent
						union all
						select ukprn, CreationDate from Payments2.DataLockEvent
						union all
						select ukprn, CreationDate from Payments2.RequiredPaymentEvent
						union all
						select ukprn, CreationDate from Payments2.FundingSourceEvent
					) as d
					where ukprn in ({ukprnsString})";


            using (SqlConnection connection = GetPaymentsConnectionString())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = TimeSpan.FromSeconds(120).Seconds;
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    connection.Open();
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.IsDBNull(result) ? (DateTimeOffset?)null : (DateTimeOffset?)result;
                }

            }
        }


        public async Task<decimal?> GetTheNumber(short academicYear, byte collectionPeriod, bool populateEarnings,
            DateTime startDateTime, DateTime endDateTime)
        {
            var sql = @"select
           
            CASE WHEN SUM([Earnings YTD (audit)]) = 0 THEN 0 ELSE 
            SUM([Missing Required Payments]) / sum([Earnings YTD (audit)]) *100
            END AS [The Number]
            FROM(
            ";

            sql +=   Scripts.ScriptHelpers.GetSqlScriptText("VerificationQuery.sql");
            sql += @"
                ) as sq
            where[Transaction Type] in (1, 2, 3)";

            using (SqlConnection connection = GetPaymentsConnectionString())
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@academicYear", academicYear);
                    cmd.Parameters.AddWithValue("@collectionPeriod", collectionPeriod);
                    cmd.Parameters.AddWithValue("@populateEarnings", populateEarnings);
                    cmd.Parameters.AddWithValue("@StartTime", startDateTime);
                    cmd.Parameters.AddWithValue("@EndTime", endDateTime);
                    connection.Open();
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.IsDBNull(result) ? (decimal?) null: (decimal?) result;
                }
            }
        }



        private SqlConnection GetDataStoreConnectionString(short year)
        {
            switch (year)
            {
                case 1819:
                 return   new SqlConnection(configuration.GetConnectionString("ILR1819DataStoreConnectionString"));
                case 1920:
                    return new SqlConnection(configuration.GetConnectionString("ILR1920DataStoreConnectionString"));
                default:
                    throw new ArgumentOutOfRangeException($"The given year {year} is not supported");
            }
        }


        private SqlConnection GetPaymentsConnectionString()
        {
            return new SqlConnection(configuration.PaymentsConnectionString);
        }


        private static string CreateCsvFromDataReader(SqlDataReader reader)
        {
            StringBuilder sb = new StringBuilder();

            var columnNames = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToList();
            sb.Append(string.Join(",", columnNames));
            sb.AppendLine();

            while (reader.Read())
            {
                var rowData = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => (!reader.IsDBNull(i)) ? reader.GetValue(i).ToString() : (string) null)
                    .ToList();
                sb.Append(string.Join(",", rowData));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}