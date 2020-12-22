using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TINF.LV.Z05 {
    /*
        UI komponenta tipke za dodavanje novih simbola i njihovih pripadnih vjerojatnosti.
        Originalno napisana za jedan vlastiti projekt, modificirana za upotrebu u laboratorijskoj vježbi:
        https://github.com/mat1jaczyyy/apollo-studio/blob/master/Apollo/Components/AddButton.cs
        https://github.com/mat1jaczyyy/apollo-studio/blob/master/Apollo/Components/VerticalAdd.cs
        https://github.com/mat1jaczyyy/apollo-studio/blob/master/Apollo/Components/VerticalAdd.xaml
    */
    partial class AddButton: UserControl {
        static readonly SolidColorBrush _up = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));   // Boja tipke u nepritisnutom stanju
        static readonly SolidColorBrush _down = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78)); // Boja tipke u pritisnutom stanju
        static readonly SolidColorBrush _over = new SolidColorBrush(Color.FromArgb(0xFF, 0x64, 0x64, 0x64)); // Boja tipke u stanju prelaska pokazivačem preko

        // Inicijalizacija tipke
        public AddButton() {
            InitializeComponent();
            MLeave(this, null);
        }

        // Event pritiska tipke
        public delegate void PressedEventHandler();
        public event PressedEventHandler Pressed;

        #region Input Handling
        // Pamti stanje pritisnutosti
        bool mHeld = false;

        // Prelazak pokazivačem preko
        protected void MEnter(object sender, MouseEventArgs e) {
            Path.Stroke = mHeld? _down : _over;
        }

        // Izlazak pokazivača
        protected void MLeave(object sender, MouseEventArgs e) {
            Path.Stroke = _up;
            mHeld = false;
        }

        // Pritisnuto
        protected void MDown(object sender, MouseEventArgs e) {
            mHeld = true;
            Path.Stroke = _down;
        }

        // Nepritisnuto
        protected void MUp(object sender, MouseEventArgs e) {
            if (!mHeld) return;

            mHeld = false;
            MEnter(sender, null);

            Pressed?.Invoke(); // Tipka se pritisnula
        }
        #endregion
    }
}
