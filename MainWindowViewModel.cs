﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace FmpAnalyzer
{
    public class MainWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty ConnectionStringProperty;
        public static readonly DependencyProperty ResultsProperty;
        public RelayCommand CommandGo { get; set; }

        static MainWindowViewModel()
        {
            ConnectionStringProperty = DependencyProperty.Register("ConnectionString", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(String.Empty));
            ResultsProperty = DependencyProperty.Register("Results", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(String.Empty));
    }

        public MainWindowViewModel()
        {
            ConnectionString = Configuration.Instance["ConnectionString"];

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
        /// OnCommandGo
        /// </summary>
        /// <param name="p"></param>
        private void OnCommandGo(object p)
        {
            var topRoe = Companies.Instance.WithBestRoe(10, "2019-12-31");
            foreach (var obj in topRoe)
            {
                Results += $"\r\n{obj}";
            }

        }

    }
}
