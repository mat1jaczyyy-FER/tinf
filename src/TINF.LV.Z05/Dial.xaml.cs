using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TINF.LV.Z05 {
    /*
        UI komponenta diala za postavljanje vrijednosti vjerojatnosti.
        Originalno napisana za jedan vlastiti projekt, modificirana za upotrebu u laboratorijskoj vježbi:
        https://github.com/mat1jaczyyy/apollo-studio/blob/master/Apollo/Components/Dial.cs
        https://github.com/mat1jaczyyy/apollo-studio/blob/master/Apollo/Components/Dial.xaml
    */
    partial class Dial: UserControl {
        // Inicijalizacija diala
        public Dial() {
            InitializeComponent();

            ArcCanvas.Width = width;    // Veličina elementa
            ArcCanvas.Height = height;

            DrawArcBase();   // Nacrtaj luk na početku
            DrawArcMain();
        }

        // Event promjene vrijednosti
        public delegate void DialChangedEventHandler(Dial sender);
        public event DialChangedEventHandler Changed;

        #region Value Handling
        // Konstante
        const double Minimum = 1;
        const double Maximum = 100;

        // Pretvorbe
        double ToValue(double rawValue) => (rawValue - Minimum) / (Maximum - Minimum);
        double ToRawValue(double value) => Minimum + (Maximum - Minimum) * value;

        // Value pamti u kojoj poziciji je dial, 0 skroz lijevo, 1 skroz desno
        bool _valuechanging = false;
        double _value = 0.5;
        public double Value {
            get => _value;
            set {
                value = Math.Max(0, Math.Min(1, value));
                if (!_valuechanging && value != _value) {
                    _valuechanging = true;

                    _value = value;
                    RawValue = ToRawValue(_value);
                    DrawArcMain();

                    _valuechanging = false;
                }
            }
        }

        // RawValue pamti koju stvarnu vrijednost reprezentira trenutna pozicija diala
        bool _rawchanging = false;
        double _raw = 50;
        public double RawValue {
            get => _raw;
            set {
                value = Math.Round(Math.Max(Minimum, Math.Min(Maximum, value)));
                if (!_rawchanging && _raw != value) {
                    _rawchanging = true;

                    _raw = value;
                    Value = ToValue(_raw);
                    Display.Text = ValueString;

                    Changed?.Invoke(this);

                    _rawchanging = false;
                }
            }
        }

        // Naziv diala
        string _title;
        public string Title {
            get => _title;
            set => TitleText.Text = _title = $"p({value})";
        }

        // Mijenjanje diala omogućeno
        bool _enabled = true;
        public bool Enabled {
            get => _enabled;
            set {
                _enabled = value;

                Focus();
                DrawArcMain();
            }
        }
        #endregion

        #region Drawing
        // Prikaz vrijednosti uz dial
        string ValueString => $"{RawValue}%";

        // Konstante za crtanje diala
        const double width = 21.5, height = 19.5;
        const double radius = 9, stroke = 3.5;
        const double strokeHalf = stroke / 2;

        const double angle_start = 4 * Math.PI / 3;
        const double angle_end = -1 * Math.PI / 3;

        // Računanje pozicije točke na kružnici
        void CalcPos(double ang, out double x, out double y) {
            x = radius * (Math.Cos(ang) + 1) + strokeHalf;
            y = radius * (-Math.Sin(ang) + 1) + strokeHalf;
        }

        // Crtanje luka diala
        void DrawArc(Path Arc, double value, bool overrideBase) {
            CalcPos(angle_start, out double x_start, out double y_start);  // Lijeva točka (0)

            double angle_point = angle_start - Math.Abs(angle_end - angle_start) * (1 - (1 - value) * 0.9);  // Kut do kud ide dial, uz padding
            CalcPos(angle_point, out double x_end, out double y_end);      // Točka do kud ide dial

            double angle = (angle_start - angle_point) / Math.PI * 180;    // Parametri za Arc Geometry Data
            int large = Convert.ToInt32(angle > 180);
            int direction = Convert.ToInt32(angle > 0);

            Arc.StrokeThickness = stroke;   // Debljina luka
            if (!overrideBase) {
                Arc.Stroke = new SolidColorBrush(Enabled? Color.FromArgb(0xFF, 0x02, 0x88, 0xD1) : Color.FromArgb(0xFF, 0x70, 0x70, 0x70));  // Boja luka
                Display.Text = ValueString;  // Prikaz stvarne vrijednosti
            }

            // Nacrtaj luk
            Arc.Data = Geometry.Parse(String.Format("M {0},{1} A {2},{2} {3} {4} {5} {6},{7}",
                x_start.ToString(CultureInfo.InvariantCulture),
                y_start.ToString(CultureInfo.InvariantCulture),
                radius.ToString(CultureInfo.InvariantCulture),
                angle.ToString(CultureInfo.InvariantCulture),
                large,
                direction,
                x_end.ToString(CultureInfo.InvariantCulture),
                y_end.ToString(CultureInfo.InvariantCulture)
            ));
        }

        // Baza luka je crna pozadina, Main dio luka prikazuje vrijednost grafički
        void DrawArcBase() => DrawArc(ArcBase, 1, true);
        void DrawArcMain() => DrawArc(ArcMain, _value, false);
        #endregion

        #region Mouse Input Handling
        // Pamti stanje pritisnutosti, prethodnu poziciju 
        bool mouseHeld = false;
        double lastY;

        // Pritisak miša
        void Down(object sender, MouseButtonEventArgs e) {
            if (!Enabled || e.ChangedButton != MouseButton.Left) return;

            if (e.ClickCount == 2) {  // Double-click započinje unos tipkovnicom
                DisplayPressed(sender, e);
                return;
            }

            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);   // Makni fokus sa svih ostalih elemenata
            Keyboard.ClearFocus();

            mouseHeld = true;
            e.MouseDevice.Capture(ArcCanvas);  // Capture za povlačenje

            lastY = e.GetPosition(ArcCanvas).Y;

            ArcCanvas.Cursor = Cursors.SizeNS;
        }

        void Up(object sender, MouseButtonEventArgs e) {
            if (!Enabled || e.ChangedButton != MouseButton.Left) return;
            
            mouseHeld = false;
            e.MouseDevice.Capture(null);  // Otpusti pokazivač

            Changed?.Invoke(this);

            ArcCanvas.Cursor = Cursors.Hand;
        }

        void Move(object sender, MouseEventArgs e) {
            if (!Enabled || !mouseHeld) return;

            double Y = e.GetPosition(ArcCanvas).Y;

            Value += (lastY - Y) / 300;  // Promjena vrijednosti je promjena pozicije povlačenjem pokazivača
            lastY = Y;
        }
        #endregion

        #region Keyboard Input Handling
        Action InputUpdate;

        // Kod za provjeravanje unosa broja tipkovnicom. Ovo sam napisao prije više od godine i pol, nemam pojma kako radi
        void InputChanged(object sender, TextChangedEventArgs e) {
            string text = Input.Text;

            if (text == null) return;
            if (text == "") return;

            InputUpdate = () => Input.Text = RawValue.ToString();

            if (int.TryParse(text, out int value)) {
                if (Minimum <= value && value <= Maximum) {
                    RawValue = value;
                    InputUpdate = () => Input.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0xDC, 0xDC));
                } else {
                    InputUpdate = () => Input.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xBE, 0x17, 0x07));
                }

                InputUpdate += () => {
                    if (value < 0) text = $"-{text.Substring(1).TrimStart('0')}";
                    else if (value > 0) text = text.TrimStart('0');
                    else text = "0";

                    if (Minimum >= 0) {
                        if (value < 0) text = "0";

                    } else {
                        int lower = -(int)Math.Pow(10, ((int)Minimum).ToString().Length - 1) + 1;
                        if (value < lower) text = lower.ToString();
                    }

                    int upper = (int)Math.Pow(10, ((int)Maximum).ToString().Length) - 1;
                    if (value > upper) text = upper.ToString();

                    Input.Text = text;
                };
            }

            if (Minimum < 0 && text == "-") InputUpdate = null;

            Dispatcher.InvokeAsync(() => {
                InputUpdate?.Invoke();
                InputUpdate = null;
            });
        }

        // Započni unos tipkovnicom
        void DisplayPressed(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && Enabled) {
                Input.Text = RawValue.ToString();

                Input.CaretIndex = Input.Text.Length;
                Input.SelectionStart = 0;
                Input.SelectionLength = Input.Text.Length;

                Input.Opacity = 1;
                Input.IsEnabled = Input.IsHitTestVisible = true;
                Input.Focus();

                e.Handled = true;
            }
        }
        
        // Prekini unos tipkovnicom
        void InputLostFocus(object sender, RoutedEventArgs e) {
            Input.Opacity = 0;
            Input.IsEnabled = Input.IsHitTestVisible = false;

            Changed?.Invoke(this);
        }

        // Prekini unos tipkovnicom ako je Enter tipka pritisnuta
        void InputKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                InputLostFocus(null, null);

            e.Handled = true;
        }
        #endregion
    }
}
