﻿using FmpAnalyzer.ResultSet;
using FmpDataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FmpAnalyzer.Queries
{
    /// <summary>
    /// SortBy
    /// </summary>
    public class SortBy
    {
        public string Text { get; set; }

        public bool Descending { get; set; }

        public Func<ResultSet.ResultSet, object> Function;
    }
}