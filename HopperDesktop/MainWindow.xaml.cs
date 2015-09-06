using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HopperEngine;
using System.Diagnostics;
using System.ComponentModel;
using System.Media;
using System.IO;

namespace HopperDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            m_manager = new GameManager(new GameManager.GameStateChangedDelgate(GameStateChanged), new GameManager.ShowHopDelegate(ShowHop));
            DataContext = this;

            InitializeTasks();
        }

        

        public GameManager.GameState GameStatus 
        { 
            get 
            {
                return m_gameState;
            }
            set
            {
                m_gameState = value;
                this.NotifyPropertyChanged("GameStatus");
            }
        }

        private GameManager.GameState m_gameState = GameManager.GameState.NotStarted;

        private void GameStateChanged(GameManager.GameState newState)
        {
            bool stateChanged = GameStatus != newState;
            GameStatus = newState;
            UpdatePadViews();
            if (stateChanged)
            {
                if (newState == GameManager.GameState.Lost)
                {
                    m_lostSound.Play();
                }
                else if (newState == GameManager.GameState.Won)
                {
                    m_wonSound.Play();
                }
                else
                {
                    m_swampSound.PlayLooping();
                }
            }
        }

        private void ShowHop(Hop hop)
        {
            PadView pvStart = m_padViews[hop.StartPad];
            PadView pvEnd = m_padViews[hop.EndPad];
            // Need to od a local animation here? https://msdn.microsoft.com/en-us/library/aa970492(v=vs.110).aspx
        }

        private void InitializeTasks()
        {
            m_tasks = new List<TaskInfo>();
            int total = m_manager.GetTotalTasks();
            for (int t = 0; t < total; t++)
            {
                TaskInfo ti = new TaskInfo();
                ti.Number = t;
                ti.Name = String.Format("{0} ({1} of {2})", m_manager.GetTaskName(t), t + 1, total);
                switch (m_manager.GetTaskLevel(t))
                {
                    case HopperEngine.Task.Level.Beginner: ti.Color = Brushes.Green; break;
                    case HopperEngine.Task.Level.Intermediate: ti.Color = Brushes.Magenta; break;
                    case HopperEngine.Task.Level.Advanced: ti.Color = Brushes.Cyan; break;
                    case HopperEngine.Task.Level.Expert: ti.Color = Brushes.Red; break;
                }
                m_tasks.Add(ti);
            }

            m_taskCombo.ItemsSource = m_tasks;
            m_taskCombo.SelectedValue = m_tasks[0];
        }

        private void m_taskCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TaskInfo item = m_taskCombo.SelectedItem as TaskInfo;
            m_manager.SetTask(item.Number);
        }

        class PadView
        {
            public PadView(char padId, GameManager manager, Canvas pondCanvas, Random rng)
            {
                m_manager = manager;
                m_padId = padId;

                hallo = CreateHalloEllipse();
                pondCanvas.Children.Add(hallo);

                pad = CreatePadCanvas(rng);
                pondCanvas.Children.Add(pad);

                frog = CreateFrogEllipse();
                pondCanvas.Children.Add(frog);

                hitTest = CreateInvisibleHitTestEllipse();
                pondCanvas.Children.Add(hitTest);

                hitTest.MouseDown += new MouseButtonEventHandler(MouseDownHandler);
            }

            internal void SetCoords(double xcoord, double ycoord)
            {
                hallo.SetValue(Canvas.LeftProperty, xcoord - hallo.Width / 2);
                hallo.SetValue(Canvas.TopProperty, ycoord - hallo.Height / 2);

                pad.SetValue(Canvas.LeftProperty, xcoord - pad.Width / 2);
                pad.SetValue(Canvas.TopProperty, ycoord - pad.Height / 2);

                frog.SetValue(Canvas.LeftProperty, xcoord - frog.Width / 2);
                frog.SetValue(Canvas.TopProperty, ycoord - frog.Height / 2);

                hitTest.SetValue(Canvas.LeftProperty, xcoord - hitTest.Width / 2);
                hitTest.SetValue(Canvas.TopProperty, ycoord - hitTest.Height / 2);
            }


            public bool IsSender(object sender) { return hitTest == sender;  }

            public char PadId { get { return m_padId; } }

            public PadSelection Selection { get { return m_manager.GetPad(m_padId).Selection;  } }

            public PadState State { get { return m_manager.GetPad(m_padId).State; } }

            private Canvas CreatePadCanvas(Random rng)
            {
                Canvas c = new Canvas();
                c.Width = 58;
                c.Height = 58;
                Ellipse lily = new Ellipse();
                lily.Height = lily.Width = 50;
                lily.Stroke = padBrush;
                lily.Fill = padBrush;
                c.Children.Add(lily);
                lily.SetValue(Canvas.LeftProperty, 4.0);
                lily.SetValue(Canvas.TopProperty, 4.0);
                Ellipse notch = new Ellipse();
                notch.Width = notch.Height = 20;
                notch.Stroke = Brushes.LightBlue;
                notch.Fill = Brushes.LightBlue;
                c.Children.Add(notch);
                notch.SetValue(Canvas.LeftProperty, rng.Next(0, 2) > 0 ? 2.0 : 34.0);
                notch.SetValue(Canvas.TopProperty, rng.Next(0, 2) > 0 ? 2.0 : 34.0);
                return c;
            }

            private Ellipse CreateHalloEllipse()
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = ellipse.Height = 65;
                ellipse.Stroke = selectHalloBrush;
                ellipse.Fill = waterBrush;
                ellipse.Opacity = 0;
                return ellipse;
            }

            private Ellipse CreateFrogEllipse()
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 30;
                ellipse.Width = 30;
                ellipse.Stroke = Brushes.Black;
                ellipse.Fill = Brushes.Red;
                ellipse.Opacity = 0;
                return ellipse;
            }

            private Ellipse CreateInvisibleHitTestEllipse()
            {

                Ellipse ellipse = new Ellipse();
                ellipse.Height = 70;
                ellipse.Width = 70;
                ellipse.Opacity = 0;
                ellipse.Stroke = Brushes.Black;
                ellipse.Fill = Brushes.Black;
                return ellipse;
            }

            public void UpdateView()
            {
                switch (State)
                {
                    case PadState.Green:
                        if (frog.Fill != Brushes.LightGreen)
                            frog.Fill = Brushes.LightGreen;
                        if (frog.Opacity != 100)
                            frog.Opacity = 100;
                        break;
                    case PadState.Red:
                        if (frog.Fill != Brushes.Red)
                            frog.Fill = Brushes.Red;
                        if (frog.Opacity != 100)
                            frog.Opacity = 100;
                        break;
                    case PadState.Empty:
                        if (frog.Opacity != 0)
                            frog.Opacity = 0;
                        break;
                }

                switch (Selection)
                {
                    case PadSelection.None:
                        if (hallo.Opacity != 0)
                            hallo.Opacity = 0;
                        break;
                    case PadSelection.Option:
                        if (hallo.Opacity != 100)
                            hallo.Opacity = 100;
                        if (hallo.Stroke != Brushes.Yellow)
                            hallo.Stroke = Brushes.Yellow;
                        break;
                    case PadSelection.Selected:
                        if (hallo.Opacity != 100)
                            hallo.Opacity = 100;
                        if (hallo.Stroke != Brushes.Red)
                            hallo.Stroke = Brushes.Red;
                        break;
                }
            }

            private void MouseDownHandler(object sender, MouseButtonEventArgs e)
            {
                if (sender == hitTest)
                {
                    Console.WriteLine("MouseDownHandler: sender = {0}", m_padId);
                    switch (Selection)
                    {
                        case PadSelection.None:
                            Console.WriteLine("Selecting pad: {0}", m_padId);
                            m_manager.SelectPad(m_padId);
                            break;
                        case PadSelection.Option:
                            Console.WriteLine("Jumping to pad: {0}", m_padId);
                            m_manager.JumpTo(m_padId);
                            break;
                    }
                }
            }

            private GameManager m_manager;
            private char m_padId;
            private Ellipse hallo;
            private Canvas pad;
            private Ellipse frog;
            private Ellipse hitTest;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeAudio(); 
            InitializeGraphics();
            UpdatePadViews();
            m_manager.ShowSolution();
        }

        private void InitializeAudio()
        {
            m_wonSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Audio\Applause.wav"));
            m_lostSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Audio\WahWah.wav"));
            m_swampSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Audio\Swamp.wav"));
            m_swampSound.PlayLooping();
        }
        private SoundPlayer m_swampSound;
        private SoundPlayer m_lostSound;
        private SoundPlayer m_wonSound;

        private void UpdatePadViews()
        {
            foreach(PadView pv in m_padViews.Values)
            {
                pv.UpdateView();
            }
            m_undo.IsEnabled = m_manager.HopStack.HasUndo();
            m_redo.IsEnabled = m_manager.HopStack.HasRedo();

            m_jumpedPanel.Children.Clear();
            foreach (Hop hop in m_manager.HopStack.Undos())
            {
                // Console.Write("({0} <- {1}) ", hop.StartPad, hop.EndPad);
                Ellipse e = new Ellipse();
                e.Width = 20;
                e.Height = 20;
                e.Stroke = Brushes.Black;
                e.Fill = Brushes.LightGreen;
                e.Margin = new Thickness(5);
                m_jumpedPanel.Children.Add(e);
            }
            foreach (Hop hop in m_manager.HopStack.Redos())
            {
                //Console.Write("({0} -> {1}) ", hop.StartPad, hop.EndPad);
                Ellipse e = new Ellipse();
                e.Width = 20;
                e.Height = 20;
                e.Stroke = Brushes.Gray;
                e.Fill = Brushes.White;
                e.Margin = new Thickness(5);
                m_jumpedPanel.Children.Add(e);
            }
        }

        private double QuarterPond { get { return Math.Min(PondCanvas.ActualWidth, PondCanvas.ActualHeight) / 4; } }

        private double EighthPond { get { return Math.Min(PondCanvas.ActualWidth, PondCanvas.ActualHeight) / 8; } }

        private void InitializeGraphics()
        {
            Random rng = new Random();
            char padId = 'A';
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    CreatePadView(padId++, rng);
                }
                if (y < 2)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        CreatePadView(padId++, rng);
                    }
                }
            }

            LayoutPond();
        }

        private void LayoutPond()
        {
            if (m_padViews.Count <= 0) return;

            double offsetHorizontal = Math.Max(PondCanvas.ActualWidth - PondCanvas.ActualHeight, 0) / 2;
            double offsetVertical = Math.Max(PondCanvas.ActualHeight - PondCanvas.ActualWidth, 0) / 2;

            char padId = 'A';
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    double xcoord = offsetHorizontal + QuarterPond + x * QuarterPond;
                    double ycoord = offsetVertical + QuarterPond + y * QuarterPond;

                    m_padViews[padId++].SetCoords(xcoord, ycoord);
                }
                if (y < 2)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        double xcoord = offsetHorizontal + QuarterPond + (x * QuarterPond) + EighthPond;
                        double ycoord = offsetVertical + QuarterPond + (y * QuarterPond) + EighthPond;

                        m_padViews[padId++].SetCoords(xcoord, ycoord);
                    }
                }
            }
        }

        private void PondCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutPond();
        }

        private void CreatePadView(char padId, Random rng)
        {
            PadView pv = new PadView(padId,
                                    m_manager,
                                    PondCanvas,
                                    rng);
            m_padViews[padId] = pv;

        }

        Dictionary<char, PadView> m_padViews = new Dictionary<char,PadView>();

        static Brush waterBrush = Brushes.LightBlue;
        static Brush padBrush = Brushes.ForestGreen;
        static Brush selectHalloBrush = Brushes.OrangeRed;
        static Brush optionHalloBrush = Brushes.Yellow;

        

        class TaskInfo
        {
            public int Number { get; set; }
            public string Name { get; set; }
            public Brush Color { get; set;  }
        }
        List<TaskInfo> m_tasks;

        private GameManager m_manager;


        private void m_next_Click(object sender, RoutedEventArgs e)
        {
            m_manager.NextTask();
            m_taskCombo.SelectedIndex = m_manager.GetTaskNumber();
        }

        private void m_undo_Click(object sender, RoutedEventArgs e)
        {
            m_manager.UndoHop();
        }

        private void m_redo_Click(object sender, RoutedEventArgs e)
        {
            m_manager.RedoHop();
        }

 
    
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
                {
                        if(this.PropertyChanged != null)
                                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
                }
    }
}
