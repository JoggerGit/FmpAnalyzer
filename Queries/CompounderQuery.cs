﻿using FmpAnalyzer.Data;
using FmpDataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Common;

namespace FmpAnalyzer.Queries
{
    /// <summary>
    /// CompounderQuery
    /// </summary>
    public class CompounderQuery : QueryBase
    {
        public CompounderQuery(DataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ResultSetList Run<T>(CompounderQueryParams<T> parameters)
        {
            ResultSetList resultSetList = null;

            var p = parameters;
            ReportProgress(100, 10, $"Retrieving companies with ROE > {parameters.RoeFrom}");

            var command = CommandCompounder(DataContext.Database.GetDbConnection(), SqlCompounder(parameters), parameters);
            var queryAsEnumerable = QueryAsEnumerable(command, ResultsetFunctionCompounder).OrderByDescending(parameters.OrderFunction).ToList();

            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.RoeHistoryQuery, a => a.RoeHistory);
            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.RevenueHistoryQuery, a => a.RevenueHistory);
            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.EpsHistoryQuery, a => a.EpsHistory);

            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.RoeGrowthKoef, r => r.RoeHistory);
            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.RevenueGrowthKoef, r => r.RevenueHistory);
            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.EpsGrowthKoef, r => r.EpsHistory);

            List<ResultSet> listOfResultSets = p.Descending
                ? queryAsEnumerable.OrderByDescending(p.OrderFunction).Skip(p.CurrentPage * p.PageSize).Take(p.PageSize).ToList()
                : queryAsEnumerable.OrderBy(p.OrderFunction).Skip(p.CurrentPage * p.PageSize).Take(p.PageSize).ToList();
            resultSetList = new ResultSetList(listOfResultSets);
            resultSetList.CountTotal = queryAsEnumerable.Count();

            ReportProgress(100, 20, $"OK! {resultSetList.CountTotal} companies found.");

            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.ReinvestmentHistoryQuery, a => a.ReinvestmentHistory);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.IncrementalRoeQuery, a => a.IncrementalRoe);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.OperatingIncomeHistoryQuery, a => a.OperatingIncome);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.CashConversionQuery, a => a.CashConversionHistory);
            resultSetList = AddCompanyName(resultSetList);
            resultSetList = AddDebtEquityIncome(resultSetList);

            ReportProgress(100, 100, $"OK! Finished query.");
            return resultSetList;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Count(CompounderCountQueryParams parameters)
        {
            var command = CommandCompounder(DataContext.Database.GetDbConnection(), SqlCompounder(parameters), parameters);
            return QueryAsEnumerable(command, ResultsetFunctionCompounder).Count();
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public ResultSetList Run<T>(CompounderQueryParams<T> parameters, string symbol)
        {
            ResultSetList resultSetList = null;

            var command = CommandFindBySymbol(DataContext.Database.GetDbConnection(), SqlFindBySymbol(symbol), symbol);
            var queryAsEnumerable = QueryAsEnumerable(command, ResultsetFunctionFindBySymbol).ToList();

            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.RoeHistoryQuery, a => a.RoeHistory);
            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.RevenueHistoryQuery, a => a.RevenueHistory);
            queryAsEnumerable = AddHistoryData(queryAsEnumerable, parameters, QueryFactory.EpsHistoryQuery, a => a.EpsHistory);

            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.RoeGrowthKoef, r => r.RoeHistory);
            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.RevenueGrowthKoef, r => r.RevenueHistory);
            queryAsEnumerable = AdjustToGrowthKoef(queryAsEnumerable, parameters.EpsGrowthKoef, r => r.EpsHistory);

            List<ResultSet> listOfResultSets = queryAsEnumerable.ToList();
            resultSetList = new ResultSetList(listOfResultSets);
            resultSetList.CountTotal = queryAsEnumerable.Count();

            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.ReinvestmentHistoryQuery, a => a.ReinvestmentHistory);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.IncrementalRoeQuery, a => a.IncrementalRoe);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.OperatingIncomeHistoryQuery, a => a.OperatingIncome);
            resultSetList = AddHistoryData(resultSetList, parameters, QueryFactory.CashConversionQuery, a => a.CashConversionHistory);
            resultSetList = AddCompanyName(resultSetList);
            resultSetList = AddDebtEquityIncome(resultSetList);

            return resultSetList;

        }


        /// <summary>
        /// FindByCompany
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public string FindByCompany<T>(CompounderQueryParams<T> parameters, string company)
        {
            string result = string.Empty;

            var command = CommandFindByCompany(DataContext.Database.GetDbConnection(), SqlFindByCompany(company), company);
            var queryAsEnumerable = QueryAsEnumerable(command, ResultsetFunctionFindByCompany).ToList();
            if(!queryAsEnumerable.Any())
            {
                return string.Empty;
            }
            
            result = queryAsEnumerable.Select(q => q.Symbol).Distinct().Aggregate((r, n) => r+ "\r\n" + n);

            return result;
        }

        /// <summary>
        /// QueryAsEnumerable
        /// </summary>
        /// <param name="command"></param>
        /// <param name="resultSetFunction"></param>
        /// <returns></returns>
        private IEnumerable<ResultSet> QueryAsEnumerable(DbCommand command, Func<DataTable, IEnumerable<ResultSet>> resultSetFunction)
        {
            command.Connection.Open();
            DataTable dataTable = null;
            using (var reader = command.ExecuteReader())
            {
                dataTable = new DataTable();
                dataTable.Load(reader);

            }
            command.Connection.Close();
            return resultSetFunction(dataTable);
        }

        /// <summary>
        /// CommandCompounder
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private DbCommand CommandCompounder(DbConnection connection, string sql, CompounderCountQueryParams parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            AddDoubleParameter(command, "@RoeFrom", DbType.Double, parameters.RoeFrom);
            AddDoubleParameter(command, "@RoeTo", DbType.Double, parameters.RoeTo);
            AddDoubleParameter(command, "@ReinvestmentRateFrom", DbType.Double, parameters.ReinvestmentRateFrom);
            AddDoubleParameter(command, "@ReinvestmentRateTo", DbType.Double, parameters.ReinvestmentRateTo);

            var dates = FmpHelper.BuildDatesList(parameters.YearFrom, parameters.YearTo, parameters.Dates);
            AddStringListParameter(command, "@Dates", DbType.String, dates);

            AddDoubleParameter(command, "@DebtEquityRatioFrom", DbType.Double, parameters.DebtEquityRatioFrom);
            AddDoubleParameter(command, "@DebtEquityRatioTo", DbType.Double, parameters.DebtEquityRatioTo);

            return command;
        }

        /// <summary>
        /// CommandFindBySymbol
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private DbCommand CommandFindBySymbol(DbConnection connection, string sql, string symbol)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            AddStringParameter(command, "@Symbol", DbType.String, symbol);
            return command;
        }

        /// <summary>
        /// CommandFindByCompany
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="v"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        private DbCommand CommandFindByCompany(DbConnection connection, string sql, string company)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            AddStringParameter(command, "@Symbol", DbType.String, company);
            return command;
        }

        /// <summary>
        /// SqlCompounder
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string SqlCompounder(CompounderCountQueryParams parameters)
        {
            string sqlBase = $@"select Symbol, Date, Equity, Debt, NetIncome, Roe, ReinvestmentRate, DebtEquityRatio
                from ViewCompounder 
                where 1 = 1
                and Date in (@Dates)
                and Roe >= @RoeFrom
                and ReinvestmentRate >= @ReinvestmentRateFrom";

            var dates = FmpHelper.BuildDatesList(parameters.YearFrom, parameters.YearTo, parameters.Dates);
            string datesAsParam = CreateCommaSeparatedParams("@Dates", dates.Count);
            string sql = sqlBase.Replace("@Dates", datesAsParam);

            if (parameters.RoeTo != 0)
            {
                sql += " and Roe <= @RoeTo ";
            }
            if (parameters.ReinvestmentRateTo != 0)
            {
                sql += " and ReinvestmentRate <= @ReinvestmentRateTo ";
            }
            if (parameters.DebtEquityRatioFrom != 0)
            {
                sql += " and DebtEquityRatio >= @DebtEquityRatioFrom ";
            }
            if (parameters.DebtEquityRatioTo != 0)
            {
                sql += " and DebtEquityRatio <= @DebtEquityRatioTo ";
            }

            return sql;
        }

        /// <summary>
        /// SqlFindBySymbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private string SqlFindBySymbol(string symbol)
        {
            string sqlBase = $@"select Symbol, Date, Equity, Debt, NetIncome, Roe, ReinvestmentRate, DebtEquityRatio
                from ViewCompounder 
                where 1 = 1
                and Symbol = '@Symbol'";

            string sql = sqlBase.Replace("@Symbol", symbol);
            return sql;
        }

        /// <summary>
        /// SqlFindByCompany
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        private string SqlFindByCompany(string company)
        {
            string sqlBase = $@"select v.Symbol
                from ViewCompounder v
                inner join Stocks s on s.Symbol = v.Symbol
                where 1 = 1
                and s.Name like '%@Company%';";

            string sql = sqlBase.Replace("@Company", company);
            return sql;
        }

        /// <summary>
        /// CreateCommaSeparatedParams
        /// </summary>
        /// <param name="paramBase"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private string CreateCommaSeparatedParams(string paramBase, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(paramBase + i.ToString());
                if (i < count - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// AddStringListParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="dates"></param>
        private void AddStringListParameter(DbCommand command, string name, DbType dbType, List<string> dates)
        {
            for (int i = 0; i < dates.Count; i++)
            {
                string date = dates[i];
                var param = command.CreateParameter();
                param.ParameterName = name + i.ToString();
                param.DbType = dbType;
                param.Value = date;
                command.Parameters.Add(param);
            }

        }

        /// <summary>
        /// AddStringParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        private void AddStringParameter(DbCommand command, string name, DbType dbType, string value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.DbType = dbType;
            param.Value = value;
            command.Parameters.Add(param);
        }

        /// <summary>
        /// AddDoubleParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        private void AddDoubleParameter(DbCommand command, string name, DbType dbType, double value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.DbType = dbType;
            param.Value = value;
            command.Parameters.Add(param);
        }

        /// <summary>
        /// ResultsetFunctionCompounder
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private IEnumerable<ResultSet> ResultsetFunctionCompounder(DataTable dataTable)
        {
            List<ResultSet> listOfResultSets = new List<ResultSet>();

            foreach (DataRow row in dataTable.Rows)
            {
                ResultSet resultSet = new ResultSet();

                resultSet.Symbol = (string)row["Symbol"];
                resultSet.Date = (string)row["Date"];
                resultSet.Equity = row["Equity"] == DBNull.Value ? null : (double)row["Equity"];
                resultSet.Debt = row["Debt"] == DBNull.Value ? null : (double)row["Debt"];
                resultSet.NetIncome = row["NetIncome"] == DBNull.Value ? null : (double)row["NetIncome"];
                resultSet.Roe = row["Roe"] == DBNull.Value ? null : Math.Round((double)row["Roe"], 0);
                resultSet.ReinvestmentRate = row["ReinvestmentRate"] == DBNull.Value ? null : Math.Round((double)row["ReinvestmentRate"], 0);
                resultSet.DebtEquityRatio = row["DebtEquityRatio"] == DBNull.Value ? null : Math.Round((double)row["DebtEquityRatio"], 2);

                listOfResultSets.Add(resultSet);
            }

            return listOfResultSets;
        }

        /// <summary>
        /// ResultsetFunctionFindBySymbol
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private IEnumerable<ResultSet> ResultsetFunctionFindBySymbol(DataTable dataTable)
        {
            return ResultsetFunctionCompounder(dataTable);
        }

        /// <summary>
        /// ResultsetFunctionFindByCompany
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private IEnumerable<ResultSet> ResultsetFunctionFindByCompany(DataTable dataTable)
        {
            List<ResultSet> listOfResultSets = new List<ResultSet>();
            foreach (DataRow row in dataTable.Rows)
            {
                ResultSet resultSet = new ResultSet();

                resultSet.Symbol = (string)row["Symbol"];
                
                listOfResultSets.Add(resultSet);
            }

            return listOfResultSets;
        }

        /// <summary>
        /// AddHistoryData
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <param name="parameters"></param>
        /// <param name="query"></param>
        /// <param name="funcAttributeToSet"></param>
        /// <returns></returns>
        private ResultSetList AddHistoryData(ResultSetList inputResultSetList, HistoryQueryParams parameters,
            HistoryQuery query, Func<ResultSet, List<double>> funcAttributeToSet)
        {
            for (int i = 0; i < inputResultSetList.ResultSets.Count(); i++)
            {
                foreach (string dateParam in parameters.Dates)
                {
                    string date = parameters.YearFrom.ToString() + dateParam[4..];

                    var queryResults = query.Run(inputResultSetList.ResultSets[i].Symbol, date, parameters.HistoryDepth);
                    if (!queryResults.Any())
                    {
                        continue;
                    }

                    queryResults.Reverse();
                    for (int ii = 0; ii < queryResults.Count(); ii++)
                    {
                        inputResultSetList.ResultSets.Select(funcAttributeToSet).ToList()[i].Add(queryResults[ii]);
                    }
                    break;
                }
            }

            return inputResultSetList;
        }

        /// <summary>
        /// AddHistoryData
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <param name="parameters"></param>
        /// <param name="query"></param>
        /// <param name="funcAttributeToSet"></param>
        /// <returns></returns>
        private List<ResultSet> AddHistoryData(List<ResultSet> inputResultSetList, HistoryQueryParams parameters,
            HistoryQuery query, Func<ResultSet, List<double>> funcAttributeToSet)
        {
            for (int i = 0; i < inputResultSetList.Count(); i++)
            {
                foreach (string dateParam in parameters.Dates)
                {
                    string date = parameters.YearFrom.ToString() + dateParam[4..];

                    var queryResults = query.Run(inputResultSetList[i].Symbol, date, parameters.HistoryDepth);
                    if (!queryResults.Any())
                    {
                        continue;
                    }

                    queryResults.Reverse();
                    for (int ii = 0; ii < queryResults.Count(); ii++)
                    {
                        inputResultSetList.Select(funcAttributeToSet).ToList()[i].Add(queryResults[ii]);
                    }
                    break;
                }
            }

            return inputResultSetList;
        }


        /// <summary>
        /// AdjustToRoeGrowthKoef
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <param name="growthKoef"></param>
        /// <param name="funcResultSetParam"></param>
        /// <returns></returns>
        private List<ResultSet> AdjustToGrowthKoef(List<ResultSet> inputResultSetList, int growthKoef, Func<ResultSet, List<double>> funcResultSetParam)
        {
            if (growthKoef == 0)
            {
                return inputResultSetList;
            }

            List<ResultSet> resultSetList = new List<ResultSet>();

            foreach (ResultSet resultSet in inputResultSetList)
            {
                if (funcResultSetParam(resultSet).Grows() >= growthKoef)
                {
                    resultSetList.Add(resultSet);
                }
            }

            return resultSetList;
        }

        /// <summary>
        /// AddCompanyName
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <returns></returns>
        private ResultSetList AddCompanyName(ResultSetList inputResultSetList)
        {
            ReportProgress(100, 70, $"Searching for the companies names ...");
            ResultSetList resultSetList = QueryFactory.CompanyNameQuery.Run(inputResultSetList);
            ReportProgress(100, 80, $"Search for the companies names ended ...");
            return resultSetList;
        }

        /// <summary>
        /// AddCompanyName
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <returns></returns>
        private List<ResultSet> AddCompanyName(List<ResultSet> inputResultSetList)
        {
            ReportProgress(100, 70, $"Searching for the companies names ...");
            List<ResultSet> resultSetList = QueryFactory.CompanyNameQuery.Run(inputResultSetList);
            ReportProgress(100, 80, $"Search for the companies names ended ...");
            return resultSetList;
        }

        /// <summary>
        /// AddDebtEquityIncome
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <returns></returns>
        private ResultSetList AddDebtEquityIncome(ResultSetList inputResultSetList)
        {
            for (int i = 0; i < inputResultSetList.ResultSets.Count(); i++)
            {
                ResultSet resultSet = inputResultSetList.ResultSets[i];
                resultSet.DebtEquityIncome.Add(resultSet.Debt.Value);
                resultSet.DebtEquityIncome.Add(resultSet.Equity.Value);
                resultSet.DebtEquityIncome.Add(resultSet.NetIncome.Value);
            }
            return inputResultSetList;
        }

        /// <summary>
        /// AddDebtEquityIncome
        /// </summary>
        /// <param name="inputResultSetList"></param>
        /// <returns></returns>
        private List<ResultSet> AddDebtEquityIncome(List<ResultSet> inputResultSetList)
        {
            for (int i = 0; i < inputResultSetList.Count(); i++)
            {
                ResultSet resultSet = inputResultSetList[i];
                resultSet.DebtEquityIncome.Add(resultSet.Debt.Value);
                resultSet.DebtEquityIncome.Add(resultSet.Equity.Value);
                resultSet.DebtEquityIncome.Add(resultSet.NetIncome.Value);
            }
            return inputResultSetList;
        }
    }
}
