using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wordle
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] words;
        private string currentWord;
        private readonly Label[][] labels;
        private readonly Button[] letters;
        private int currentRow;
        private int currentLabel;
        private bool inPlay = false;
        private readonly int[] stats = new int[8];
        private readonly SolidColorBrush
            green = new SolidColorBrush(Colors.Green),
            orange = new SolidColorBrush(Colors.Orange),
            gray = new SolidColorBrush(Colors.LightGray),
            transparent = new SolidColorBrush(Colors.Transparent);

        public MainWindow()
        {
            words = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Wordle.dico.txt"))
                .ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            InitializeComponent();
            for (int i = 0; i < 6; ++i)
            {
                fieldGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
                if (i < 5) fieldGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(19) });
            }
            for (int i = 0; i < 5; ++i)
            {
                fieldGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                if (i < 4) fieldGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19) });
            }
            labels = new Label[6][];
            for (int i = 0; i < 6; ++i)
            {
                labels[i] = new Label[5];
                for (int j = 0; j < 5; ++j)
                {
                    var lbl = new Label { Template = (ControlTemplate)FindResource("fieldLetter") };
                    Grid.SetRow(lbl, i * 2);
                    Grid.SetColumn(lbl, j * 2);
                    fieldGrid.Children.Add(lbl);
                    labels[i][j] = lbl;
                }
            }

            for (int i = 0; i < 2; ++i)
            {
                letterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
                if (i < 1) letterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            }
            for (int i = 0; i < 13; ++i)
            {
                letterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
                if (i < 12) letterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            }
            letters = new Button[26];
            for (int i = 0, ch = 'A'; i < 2; ++i)
            {
                for (int j = 0; j < 13; ++j, ++ch)
                {
                    var btn = new Button { Template = (ControlTemplate)FindResource("button"), Content = (char)ch, Focusable = false };
                    btn.Click += (sender, args) => ProcessKey((char)btn.Content);
                    Grid.SetRow(btn, i * 2);
                    Grid.SetColumn(btn, j * 2);
                    letters[ch - 'A'] = btn;
                    letterGrid.Children.Add(btn);
                }
            }

            GenerateNewWord();
        }

        private void KeyPressClick(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 1) ProcessKey(char.ToUpper(e.Text[0]));
        }

        private void EnterClick(object sender, RoutedEventArgs e) => ProcessKey('\r');

        private void StatsClick(object sender, RoutedEventArgs e) => MessageBox.Show(
            $"1 essai: {stats[0]}\n2 essais: {stats[1]}\n3 essais: {stats[2]}\n4 essais: {stats[3]}\n5 essais: {stats[4]}\n6 essais: {stats[5]}\nDéfaites: {stats[6]}\nAbandons: {stats[7]}");

        private void NewWordClick(object sender, RoutedEventArgs e)
        {
            if (!inPlay)
            {
                GenerateNewWord();
            }
            else if (MessageBox.Show("Abandonner la partie en cours ?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Abandonné. Le mot était : " + currentWord);
                ++stats[7];
                GenerateNewWord();
            }
        }

        private void GenerateNewWord()
        {
            var r = new Random();
            do
            {
                currentWord = words[r.Next(words.Length)];
            } while (currentWord.Distinct().Count() < 5);
            foreach (Label lbl in fieldGrid.Children)
            {
                lbl.Content = null;
                lbl.Background = transparent;
            }
            foreach (Button btn in letterGrid.Children)
            {
                btn.Background = transparent;
            }
            currentRow = 0;
            currentLabel = 0;
            inPlay = true;
        }

        private void ProcessKey(char key)
        {
            if (!inPlay) return;
            if (key == '\b')
            {
                if (currentLabel > 0)
                {
                    labels[currentRow][--currentLabel].Content = null;
                }
            }
            else if (key == '\r' || key == '\n')
            {
                if (currentLabel == 5)
                {
                    string guess = labels[currentRow].Select(l => l.Content.ToString()).Aggregate((a, b) => a + b);
                    if (!words.Contains(guess))
                    {
                        MessageBox.Show("Mot non reconnu");
                        return;
                    }
                    for (int i = 0; i < 5; ++i)
                    {
                        char l = (char)labels[currentRow][i].Content;
                        if (l == currentWord[i])
                        {
                            labels[currentRow][i].Background = green;
                            letters[l - 'A'].Background = green;
                        }
                        else if (currentWord.Contains(l))
                        {
                            labels[currentRow][i].Background = orange;
                            if (letters[l - 'A'].Background != green)
                                letters[l - 'A'].Background = orange;
                        }
                        else
                        {
                            labels[currentRow][i].Background = gray;
                            letters[l - 'A'].Background = gray;
                        }
                    }
                    if (guess == currentWord)
                    {
                        inPlay = false;
                        ++stats[currentRow];
                        MessageBox.Show("Gagné !");
                    }
                    else
                    {
                        ++currentRow;
                        currentLabel = 0;
                        if (currentRow == 6)
                        {
                            inPlay = false;
                            ++stats[6];
                            MessageBox.Show("Perdu. Le mot était : " + currentWord);
                        }
                    }
                }
            }
            else if (key >= 'A' && key <= 'Z')
            {
                labels[currentRow][currentLabel++].Content = key;
            }
        }
    }
}
