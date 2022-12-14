namespace KKVideoPlayer.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AutoCompleteTextBox.Editors;
    using KKVideoPlayer.Models;

    /// <summary>
    /// Search match type.
    /// </summary>
    public enum MatchKind
    {
        StartsWith,
        Contains,
        EndsWith,
        Exact,
    }

    /// <summary>
    /// Services used to get searched contents.
    /// </summary>
    /// <typeparam name="T">Video property types. </typeparam>
    public class AutoCompleteTextBoxService<T> : DependencyObject, ISuggestionProvider
        where T : VideoProperty
    {
        private string m_LastFilter;
        private StringComparison comparison;
        private Func<string, string, bool> matchPredicate;
        
        public static readonly DependencyProperty VideoItemsDependencyProperty = DependencyProperty.Register(
            "VideoPropertyItems",
            typeof(IEnumerable<T>),
            typeof(AutoCompleteTextBoxService<T>));

        public static readonly DependencyProperty AllowEmptyFilterDependencyProperty = DependencyProperty.Register(
            "AllowEmptyFilter",
            typeof(bool),
            typeof(AutoCompleteTextBoxService<T>));

        public static readonly DependencyProperty IgnoreCaseDependencyProperty = DependencyProperty.Register(
            "IgnoreCase",
            typeof(bool),
            typeof(AutoCompleteTextBoxService<T>));
        
        public static readonly DependencyProperty MaxSuggestionCountDependencyProperty = DependencyProperty.Register(
            "MaxSuggestionCount",
            typeof(int),
            typeof(AutoCompleteTextBoxService<T>));

        public static readonly DependencyProperty MatchKindDependencyProperty = DependencyProperty.Register(
            "MatchKind",
            typeof(MatchKind),
            typeof(AutoCompleteTextBoxService<T>));

        public IEnumerable<T> VideoPropertyItems
        {
            get => Dispatcher.Invoke(() => (IEnumerable<T>)GetValue(VideoItemsDependencyProperty));
            set => Dispatcher.Invoke(() => SetValue(VideoItemsDependencyProperty, value));
        }

        public bool AllowEmptyFilter
        {
            get => (bool)GetValue(AllowEmptyFilterDependencyProperty);
            set => SetValue(AllowEmptyFilterDependencyProperty, value);
        }

        public bool IgnoreCase
        {
            get => (bool)GetValue(IgnoreCaseDependencyProperty);
            set
            {
                SetValue(IgnoreCaseDependencyProperty, value);
                comparison = value ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            }
        }

        public string LastFilter
        {
            get => m_LastFilter;
            set => m_LastFilter = value;
        }

        public int MaxSuggestionCount
        {
            get => Dispatcher.Invoke(() => (int)GetValue(MaxSuggestionCountDependencyProperty));
            set => Dispatcher.Invoke(() => SetValue(MaxSuggestionCountDependencyProperty, value));
        }

        public MatchKind MatchKind
        {
            get => (MatchKind)GetValue(MatchKindDependencyProperty);
            set
            {
                switch (value)
                {
                    case MatchKind.StartsWith:
                        matchPredicate = StartsWith;
                        break;
                    case MatchKind.EndsWith:
                        matchPredicate = EndsWith;
                        break;
                    case MatchKind.Exact:
                        matchPredicate = Exact;
                        break;
                    case MatchKind.Contains:
                    default:
                        matchPredicate = Contains;
                        break;
                }

                SetValue(MatchKindDependencyProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteTextBoxService{T}"/> class.
        /// </summary>
        public AutoCompleteTextBoxService()
        {
            IgnoreCase = true;
            MatchKind = MatchKind.Contains;
            MaxSuggestionCount = 10;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            LastFilter = filter;
            if (string.IsNullOrWhiteSpace(filter))
            {
                if (!AllowEmptyFilter)
                    return null;

                return VideoPropertyItems
                    .Take(MaxSuggestionCount)
                    .ToList();
            }

            return
                VideoPropertyItems
                    .Where(x => matchPredicate(x.PropName, filter))
                    .Take(MaxSuggestionCount)
                    .ToList();
        }

        private bool Contains(string source, string value)
        {
            if (source == null || value == null) return false;
            return source.IndexOf(value, comparison) > -1;
        }

        private bool EndsWith(string source, string value)
        {
            if (source == null || value == null) return false;
            return source.EndsWith(value, comparison);
        }

        private bool Exact(string source, string value)
        {
            return string.Equals(source, value, comparison);
        }

        private bool StartsWith(string source, string value)
        {
            if (source == null || value == null) return false;
            return source.StartsWith(value, comparison);
        }
    }
}