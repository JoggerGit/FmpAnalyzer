﻿using FmpAnalyzer.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FmpAnalyzer
{
    /// <summary>
    /// MainWindowViewModel
    /// </summary>
    public class MainWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty ConnectionStringProperty;
        public static readonly DependencyProperty ResultsProperty;
        public static readonly DependencyProperty RoeFilterProperty;
        public static readonly DependencyProperty CurrentActionProperty;
        public static readonly DependencyProperty StableRoeGrowthProperty;
        public static readonly DependencyProperty HistoryDepthRoeProperty;
        public static readonly DependencyProperty GrowthGradRoeProperty;
        public static readonly DependencyProperty ProgressMaxProperty;
        public static readonly DependencyProperty ProgressCurrentProperty;
        public static readonly DependencyProperty StableReinvestmentGrowthProperty;
        public static readonly DependencyProperty HistoryDepthReinvestmentProperty;
        public static readonly DependencyProperty GrowthGradReinvestmentProperty;
        public static readonly DependencyProperty BackgroundResultsProperty;
        public RelayCommand CommandGo { get; set; }

        static MainWindowViewModel()
        {
            ConnectionStringProperty = DependencyProperty.Register("ConnectionString", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(String.Empty));
            ResultsProperty = DependencyProperty.Register("Results", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(String.Empty));
            RoeFilterProperty = DependencyProperty.Register("RoeFilter", typeof(double), typeof(MainWindowViewModel), new PropertyMetadata(default(Double)));
            CurrentActionProperty = DependencyProperty.Register("CurrentAction", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(String.Empty));
            StableRoeGrowthProperty = DependencyProperty.Register("StableRoeGrowth", typeof(bool), typeof(MainWindowViewModel), new PropertyMetadata(default(Boolean)));
            HistoryDepthRoeProperty = DependencyProperty.Register("HistoryDepthRoe", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            GrowthGradRoeProperty = DependencyProperty.Register("GrowthGradRoe", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            ProgressMaxProperty = DependencyProperty.Register("ProgressMax", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            ProgressCurrentProperty = DependencyProperty.Register("ProgressCurrent", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            StableReinvestmentGrowthProperty = DependencyProperty.Register("StableReinvestmentGrowth", typeof(bool), typeof(MainWindowViewModel), new PropertyMetadata(default(Boolean)));
            HistoryDepthReinvestmentProperty = DependencyProperty.Register("HistoryDepthReinvestment", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            GrowthGradReinvestmentProperty = DependencyProperty.Register("GrowthGradReinvestment", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));
            BackgroundResultsProperty = DependencyProperty.Register("BackgroundResults", typeof(Brush), typeof(MainWindowViewModel), new PropertyMetadata(default(Brush)));
        }

        public MainWindowViewModel()
        {
            ConnectionString = Configuration.Instance["ConnectionString"];
            RoeFilter = 15;
            CurrentAction = "Willkommen!";
            StableRoeGrowth = true;
            HistoryDepthRoe = 5;
            GrowthGradRoe = 3;
            StableReinvestmentGrowth = true;
            HistoryDepthReinvestment = 5;
            GrowthGradReinvestment = 3;

            CommandGo = new RelayCommand(p => { OnCommandGo(p); });
        }

        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return (string)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        /// <summary>
        /// Results
        /// </summary>
        public string Results
        {
            get { return (string)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }

        /// <summary>
        /// RoeFilter
        /// </summary>
        public double RoeFilter
        {
            get { return (double)GetValue(RoeFilterProperty); }
            set { SetValue(RoeFilterProperty, value); }
        }

        /// <summary>
        /// CurrentAction
        /// </summary>
        public string CurrentAction
        {
            get { return (string)GetValue(CurrentActionProperty); }
            set { SetValue(CurrentActionProperty, value); }
        }

        /// <summary>
        /// StableRoeGrowth
        /// </summary>
        public bool StableRoeGrowth
        {
            get { return (bool)GetValue(StableRoeGrowthProperty); }
            set { SetValue(StableRoeGrowthProperty, value); }
        }

        /// <summary>
        /// HistoryDepthRoe
        /// </summary>
        public int HistoryDepthRoe
        {
            get { return (int)GetValue(HistoryDepthRoeProperty); }
            set { SetValue(HistoryDepthRoeProperty, value); }
        }

        /// <summary>
        /// GrowthGradRoe
        /// </summary>
        public int GrowthGradRoe
        {
            get { return (int)GetValue(GrowthGradRoeProperty); }
            set { SetValue(GrowthGradRoeProperty, value); }
        }

        /// <summary>
        /// ProgressMax
        /// </summary>
        public int ProgressMax
        {
            get { return (int)GetValue(ProgressMaxProperty); }
            set { SetValue(ProgressMaxProperty, value); }
        }

        /// <summary>
        /// ProgressCurrent
        /// </summary>
        public int ProgressCurrent
        {
            get { return (int)GetValue(ProgressCurrentProperty); }
            set { SetValue(ProgressCurrentProperty, value); }
        }

        /// <summary>
        /// StableReinvestmentGrowth
        /// </summary>
        public bool StableReinvestmentGrowth
        {
            get { return (bool)GetValue(StableReinvestmentGrowthProperty); }
            set { SetValue(StableReinvestmentGrowthProperty, value); }
        }

        /// <summary>
        /// HistoryDepthReinvestment
        /// </summary>
        public int HistoryDepthReinvestment
        {
            get { return (int)GetValue(HistoryDepthReinvestmentProperty); }
            set { SetValue(HistoryDepthReinvestmentProperty, value); }
        }

        /// <summary>
        /// GrowthGradReinvestment
        /// </summary>
        public int GrowthGradReinvestment
        {
            get { return (int)GetValue(GrowthGradReinvestmentProperty); }
            set { SetValue(GrowthGradReinvestmentProperty, value); }
        }

        /// <summary>
        /// BackgroundResults
        /// </summary>
        public Brush BackgroundResults
        {
            get { return (Brush)GetValue(BackgroundResultsProperty); }
            set { SetValue(BackgroundResultsProperty, value); }
        }

        /// <summary>
        /// CompounderQueryParams
        /// </summary>
        public CompounderQueryParams CompounderQueryParams { get; set; }

        /// <summary>
        /// Symbols
        /// </summary>
        public List<string> Symbols { get; set; }

        /// <summary>
        /// OnCommandGo
        /// </summary>
        /// <param name="p"></param>
        private void OnCommandGo(object p)
        {
            Results = string.Empty;
            BackgroundResults = Brushes.DarkGray;

            CompounderQueryParams = new CompounderQueryParams
            {
                Date = "2019-12-31",
                Roe = RoeFilter,
                HistoryDepthRoe = StableRoeGrowth ? HistoryDepthRoe : 0,
                GrowthGradRoe = StableRoeGrowth ? GrowthGradRoe : 0,
                HistoryDepthReinvestment = StableReinvestmentGrowth ? HistoryDepthReinvestment : 0,
                GrowthGradReinvestment = StableReinvestmentGrowth ? GrowthGradReinvestment : 0
            };

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (s, e) =>
            {
                var symbols = QueryFactory.CompounderQuery.Run(CompounderQueryParams);
                (s as BackgroundWorker).ReportProgress(100, symbols);
            };
            worker.ProgressChanged += (s, e) =>
            {
                Symbols = (List<string>)e.UserState;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundResults = Brushes.White;
                Results = $"Found {Symbols.Count()} companies:";
                Symbols.ForEach(s => Results += $"\r\n{s}");
            };
            worker.RunWorkerAsync();
        }
    }
}
