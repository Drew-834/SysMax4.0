using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SysMax2._1.Models
{
    /// <summary>
    /// Model class that represents a system issue or warning
    /// </summary>
    public class IssueInfo : INotifyPropertyChanged
    {
        public enum Severity
        {
            Low,
            Medium,
            High,
            Critical
        }

        private string _icon = "⚠️";
        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _fixButtonText = "Fix";
        public string FixButtonText
        {
            get => _fixButtonText;
            set
            {
                if (_fixButtonText != value)
                {
                    _fixButtonText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _fixActionTag = "";
        public string FixActionTag
        {
            get => _fixActionTag;
            set
            {
                if (_fixActionTag != value)
                {
                    _fixActionTag = value;
                    OnPropertyChanged();
                }
            }
        }

        private Severity _issueSeverity = Severity.Medium;
        public Severity IssueSeverity
        {
            get => _issueSeverity;
            set
            {
                if (_issueSeverity != value)
                {
                    _issueSeverity = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _timestamp = DateTime.Now;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    OnPropertyChanged();
                }
            }
        }

        // Additional properties for detailed issue information
        private string _issueTitle = "";
        public string IssueTitle
        {
            get => _issueTitle;
            set
            {
                if (_issueTitle != value)
                {
                    _issueTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _detailedDescription = "";
        public string DetailedDescription
        {
            get => _detailedDescription;
            set
            {
                if (_detailedDescription != value)
                {
                    _detailedDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isFixed = false;
        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                if (_isFixed != value)
                {
                    _isFixed = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}