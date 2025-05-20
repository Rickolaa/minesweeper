// DROUET DE LA THIBAUDERIE Pierrick

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Reflection.Metadata.BlobBuilder;

namespace MinesweeperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private int gridSize;      // grid size
        private int nbMines;       // number of mines
        private int nbCellsChecked = 0; // number of cells that have been checked (opened)
        private int[,] matrix;          // matrix preserving grid values (see below)
        private int nbMinesRemaining; // number of mines remaining (for the right click)

        public MainWindow() {
            InitializeComponent();

        }
        
        private UIElement GetUIElementFromPosition(Grid g, int col, int row) {
            return g.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
        }

        private void WNDMainWindow_Loaded(object sender, RoutedEventArgs e) {
            newGame();
        }

        private void reinitializeMap() {
            matrix = new int[gridSize, gridSize];
            nbCellsChecked = 0;
            GRDGame.Children.Clear();
            GRDGame.ColumnDefinitions.Clear();
            GRDGame.RowDefinitions.Clear();
            for (int i = 0; i < gridSize; i++) {
                GRDGame.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                GRDGame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
        }

        private void createMap() {
            for (int i = 0; i < gridSize; i++) {
                for (int j = 0; j < gridSize; j++) {
                    Border b = new Border();
                    b.BorderThickness = new Thickness(1);
                    b.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                    b.SetValue(Grid.RowProperty, j);
                    b.SetValue(Grid.ColumnProperty, i);
                    Grid grid = new Grid();

                    Label label = new Label {
                        FontSize = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    grid.Children.Add(label);

                    Button button = new Button();
                    button.Click += ButtonClick;
                    button.MouseRightButtonDown += MouseRightButtonDown;
                    grid.Children.Add(button);
                    
                    b.Child = grid;
                    GRDGame.Children.Add(b);
                    label.Content = matrix[i, j];
                }
            }
        }

        private void initializeMatrix() {

            int minesRemaining = nbMines;
            Random rnd = new Random();

            while (minesRemaining > 0) {
                int x = rnd.Next(0, gridSize - 1);
                int y = rnd.Next(0, gridSize - 1);
                if (matrix[x,y] != -1) {
                    matrix[x, y] = -1;

                    for (int i = Math.Max(0, x - 1); i <= Math.Min(gridSize - 1, x + 1); i++) {
                        for (int j = Math.Max(0, y - 1); j <= Math.Min(gridSize - 1, y + 1); j++) {
                            if (matrix[i, j] != -1) {
                                matrix[i, j]++;
                            }
                        }
                    }
                    minesRemaining--;
                }
            }

        }

        private void ButtonClick(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            //Here I assume that in each grid cell, I have a Border containing a grid containing my button. 
            Border b = (Border)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button));
            int col = Grid.GetColumn(b);
            int row = Grid.GetRow(b);

            checkCell(col, row);
        }

        private void MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Button button = (Button)sender;
            Border b = (Border)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button));
            int col = Grid.GetColumn(b);
            int row = Grid.GetRow(b);

            if (button.Visibility != Visibility.Visible)
                return;

            if (button.Content == null || button.Content.ToString() == "" || button.Content.ToString() == " ")
            {
                button.Content = "🚩";
                button.Foreground = new SolidColorBrush(Colors.Red);
                nbMinesRemaining--;
                LBLBombesRemainingValue.Content = nbMinesRemaining;
            }
            else if (button.Content.ToString() == "🚩")
            {
                button.Content = "";
                nbMinesRemaining++;
                LBLBombesRemainingValue.Content = nbMinesRemaining;
            }
        }

        //Boolean function checkCell(integer column, integer row)
        private bool checkCell(int column, int row) {
            
            Border b = (Border)GetUIElementFromPosition(GRDGame, column, row);
            Grid cellGrid = (Grid)b.Child;
            Button button = cellGrid.Children.OfType<Button>().FirstOrDefault();
            Label label = cellGrid.Children.OfType<Label>().FirstOrDefault();

            // IF the cell has not already been checked (the button is still visible/active)
            if (button != null && button.Visibility == Visibility.Visible) {

                // Hide / deactivate the button and display the value of this cell
                button.Visibility = Visibility.Collapsed;

                // Bonus, display the number except for 0 et -1
                if (matrix[column, row] != 0) {
                    label.Content = matrix[column, row];
                } if(matrix[column, row] == 0) {
                    label.Content = " ";
                } if (matrix[column, row] == -1) {
                    label.Content = "💣";
                }


                nbCellsChecked++;

                // IF the cell is a bomb THEN{ game lost, reset the game; return TRUE }
                if (matrix[column, row] == -1)  {
                    if (MessageBox.Show("You lost... Restart ?","Game Over",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        newGame();
                        return true;
                    }
                    
                }
                else {

                    // IF it was the last cell to be checked
                    if (nbCellsChecked == gridSize * gridSize - nbMines) {

                        // THEN{ game won, reset the game; return TRUE}
                        if (MessageBox.Show("You WIN !! Restart ?", "Game Over", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                            newGame();
                            return true;
                        }
                    }
                    else {
                        // IF matrix[column, row] is 0(no bombs around)
                        if (matrix[column, row] == 0) {

                            // FOR i from Max(0, column - 1) to Min(gridsize -1, column + 1) 
                            for (int i = Math.Max(0, column - 1); i <= Math.Min(gridSize - 1, column + 1); i++) {

                                // FOR j from Max(0, row-1) to Min(gridsize -1, row+1)
                                for (int j = Math.Max(0, row - 1); j <= Math.Min(gridSize - 1, row + 1); j++) {
                                    
                                    if (!(i == column && j == row)) {

                                        // Boolean resultat = checkCell(i, j)
                                        bool resultat = checkCell(i, j);

                                        //IF resultat equals TRUE
                                        if (resultat == true) {

                                            //THEN{ return TRUE }
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        private void newGame() {
            menu();
            LBLBombesRemainingValue.Content = nbMines;
            nbMinesRemaining = nbMines;
            reinitializeMap();
            initializeMatrix();
            createMap();
        }

        private void menu() {

            Window menuWindow = new Window {
                Title = "Game menu",
                Width = 220,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            panel.Children.Add(new Label { Content = "Grid size:" });
            TextBox TBXGridSize = new TextBox { Text = gridSize > 0 ? gridSize.ToString() : "10", Margin = new Thickness(0, 0, 0, 8) };
            panel.Children.Add(TBXGridSize);

            panel.Children.Add(new Label { Content = "Number of mines:" });
            TextBox TBXnbMines = new TextBox { Text = nbMines > 0 ? nbMines.ToString() : "10", Margin = new Thickness(0, 0, 0, 8) };
            panel.Children.Add(TBXnbMines);

            Button btnOK = new Button { Content = "OK", Width = 60, HorizontalAlignment = HorizontalAlignment.Center, IsDefault = true };
            btnOK.Click += (s, e) =>
            {
                int size, mines;
                if (!int.TryParse(TBXGridSize.Text, out size) || size < 2 || size > 30) {
                    MessageBox.Show("Grid size must be between 2 and 30.");
                    return;
                }
                if (!int.TryParse(TBXnbMines.Text, out mines) || mines < 1 || mines >= size * size) {
                    MessageBox.Show("Number of mines must be at least 1 and less than the number of cells.");
                    return;
                }
                gridSize = size;
                nbMines = mines;
                menuWindow.Close();
            };
            panel.Children.Add(btnOK);

            menuWindow.Content = panel;
            menuWindow.ShowDialog();
        }
    }
}