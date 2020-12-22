using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TINF.LV.Z05 {
    /*
        Glavni i jedini prozor aplikacije
    */
    partial class UI: Window {
        // Zadnji dial je poseban jer prikazuje posljednju vjerojatnost,
        // koju korisnik ne može mijenjati nego se sama prilagođava
        // dok zbroj svih vjerojatnosti ne dosegne 100%.
        Dial Last;

        // Inicijalizacija prozora
        public UI() {
            InitializeComponent();

            Last = new Dial();          // Zadnji dial je poseban jer prikazuje posljednju vjerojatnost,
            Last.RawValue = 100;        // koju korisnik ne može mijenjati nego se sama prilagođava
            Last.Enabled = false;       // dok zbroj svih vjerojatnosti ne dosegne 100%.
            Params.Children.Add(Last);

            Added(); Added();           // Dodaj još 2 simbola, tako da započinjemo s 3 (a, b, c)
            
            CanExecute();      // Inicijaliziraj stanje
        }

        // Dodavanje novog simbola
        void Added() {
            if (Last.RawValue < 2 || Params.Children.Count >= 26) return;  // Do 26 slova, moguće proširiti uz bolje imenovanje

            Dial param = new Dial();

            param.RawValue = Last.RawValue / 2;
            Last.RawValue -= param.RawValue;

            param.Changed += Changed;
            param.ContextMenu = (ContextMenu)FindResource("Remove");

            int i = Params.Children.Count - 1;
            param.Title = ArithmeticCode.PName(i).ToString();     // Nazivi simbola
            Last.Title = ArithmeticCode.PName(i + 1).ToString();

            Params.Children.Insert(i, param);

            CanExecute();
        }
        
        // Promjenjena vjerojatnost nekog simbola
        void Changed(Dial sender) {
            if (!Params.Children.Contains(sender)) return;

            // Kolika bi trebala biti vrijednost posljednje vjerojatnosti?
            int next = 100 - Params.Children.OfType<Dial>().Aggregate(0, (a, i) => a + (int)i.RawValue) + (int)Last.RawValue;

            if (next < 1) // Ako je 0 ili manje, prelazimo preko 100%
                Dispatcher.InvokeAsync(() => sender.RawValue += next - 1);  // Vrati vrijednost simbola na maksimalnu dozvoljenu

            else Last.RawValue = next;  // Prilagodi posljednju vjerojatnost
        }

        // Brisanje nekog simbola
        void Removed(object sender, RoutedEventArgs e) {
            if (Params.Children.Count <= 2) return;  // Aritmetički kod nema smisla za samo jedan simbol

            Dial removing = (Dial)((ContextMenu)FindResource("Remove")).PlacementTarget;

            removing.Changed -= Changed;
            Last.RawValue += removing.RawValue;

            // Namjesti nazive simbola
            for (int i = Params.Children.IndexOf(removing) + 1; i < Params.Children.Count; i++)
                ((Dial)Params.Children[i]).Title = ArithmeticCode.PName(i - 1).ToString();

            Params.Children.Remove(removing);

            CanExecute();
        }

        // Promjena teksta poruke koju kodiramo
        void MessageChanged(object sender, TextChangedEventArgs e) => CanExecute();

        // Provjeri da poruka nije prazna i sadrži samo zadane simbole
        bool CheckMessage(string t) {
            if (t.Length < 1) return false;
            
            IEnumerable<char> chars = Enumerable.Range(0, Params.Children.Count).Select(ArithmeticCode.PName);

            foreach (char i in t)
                if (!chars.Contains(i))
                    return false;

            return true;
        }

        // Postavi stanje prozora ovisno o valjanosti parametara - može li se izvršiti kodiranje?
        void CanExecute()
            => Message.Foreground = (btnExecute.IsEnabled = CheckMessage(Message.Text))? SystemColors.WindowTextBrush : new SolidColorBrush(Colors.Red);

        // Aritmetički kodiraj poruku
        void Execute(object sender, RoutedEventArgs e) {
            // Kodiraj poruku jednoznačno
            double result = ArithmeticCode.Encode(
                Message.Text,
                Params.Children.OfType<Dial>().Select(i => (int)i.RawValue).ToList(),
                out double left, out double right
            );

            // Bitova potrebno za jednoznačno dekodiranje poruke
            int unambig = ArithmeticCode.BitsForUnambiguity(left, right);

            // Prikaz rezultata
            Result.Text = $"Enkodirana poruka: {result}";
            Interval.Text = $"Interval: [{left}, {right}⟩";
            Bits.Text = $"Broj bitova za jednoznačnost: {unambig}";
        }
    }
}
