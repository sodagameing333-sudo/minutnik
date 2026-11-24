using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;
    private int _workDurationMinutes = 25;
    private int _breakDurationMinutes = 5;
    private int _completedCycles = 0;
    private bool _isWorkSession = true;
    private bool _isRunning = false;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        ResetTimer();
    }

    private void StartPauseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        ResetTimer();
    }

    private void WorkTimeInput_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (WorkTimeInput?.Value is decimal value)
        {
            _workDurationMinutes = (int)value;

            // Jeśli stoper jest zatrzymany w trybie pracy, od razu odśwież widoczny czas
            if (_isWorkSession && !_isRunning)
            {
                _remainingSeconds = _workDurationMinutes * 60;
                UpdateDisplay();
            }
        }
    }

    private void BreakTimeInput_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (BreakTimeInput?.Value is decimal value)
        {
            _breakDurationMinutes = (int)value;

            // Jeśli stoper jest zatrzymany w trybie przerwy, od razu odśwież widoczny czas
            if (!_isWorkSession && !_isRunning)
            {
                _remainingSeconds = _breakDurationMinutes * 60;
                UpdateDisplay();
            }
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            UpdateDisplay();
        }

        if (_remainingSeconds == 0)
        {
            if (_isWorkSession)
            {
                // Zakończenie pracy: zwiększenie licznika cykli i automatyczne uruchomienie przerwy
                _completedCycles++;
                _isWorkSession = false;
                _remainingSeconds = _breakDurationMinutes * 60;
                UpdateDisplay();
            }
            else
            {
                // Zakończenie przerwy: powrót do trybu pracy i oczekiwanie na Start
                _isWorkSession = true;
                _remainingSeconds = _workDurationMinutes * 60;
                PauseTimer();
                UpdateDisplay();
            }
        }
    }

    private void StartTimer()
    {
        _isRunning = true;
        _timer.Start();
        if (StartPauseButton != null)
        {
            StartPauseButton.Content = "Pauza";
        }
    }

    private void PauseTimer()
    {
        _isRunning = false;
        _timer.Stop();
        if (StartPauseButton != null)
        {
            StartPauseButton.Content = "Start";
        }
    }

    private void ResetTimer()
    {
        PauseTimer();
        _isWorkSession = true;
        _remainingSeconds = _workDurationMinutes * 60;
        _completedCycles = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (TimerText == null || StatusText == null || CyclesText == null)
            return;

        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        TimerText.Text = $"{minutes:D2}:{seconds:D2}";

        CyclesText.Text = $"Ukończone cykle: {_completedCycles}";

        if (_isWorkSession)
        {
            StatusText.Text = "Tryb: Praca";
            StatusText.Foreground = new SolidColorBrush(Color.Parse("#1E88E5"));
        }
        else
        {
            StatusText.Text = "Tryb: Przerwa";
            StatusText.Foreground = new SolidColorBrush(Color.Parse("#43A047"));
        }
    }
}
