using System;
using System.Collections.Generic;
using System.Linq;

namespace TINF.LV.Z05 {
    /*  
        Statična klasa koja sadrži logiku kodiranja aritmetičkog koda, usko vezana uz zadatak.
    */
    static class ArithmeticCode {
        // Pretvaranje naziva simbola u njegov indeks, i obratno
        public static char PName(int i) => (char)(i + 97);
        public static int PIndex(char c) => c - 97;

        // Kodira aritmetičkim kodom poruku te određuje interval koji jednoznačno definira poruku
        // text -> poruka
        // p -> lista vjerojatnosti simbola
        // out left, right -> izlazni parametri intervala
        public static double Encode(string text, List<int> p, out double left, out double right) {
            List<int> pos = new List<int>() { 0 };
            p.ForEach(i => pos.Add(pos.Last() + i));   // Stvaranje kumulativnih podskupova, tj. granica intervala u kojima se nalaze simboli

            return EncodeAux(text, pos, out left, out right);   // Kodirati pomoćnom rekurzijom
        }

        // Pomoćna rekurzija za kodiranje aritmetičkim kodom
        static double EncodeAux(string text, List<int> p, out double left, out double right) {
            int i = PIndex(text[0]);    // Kodirati prvi simbol u poruci sam po sebi
            left = p[i] / 100.0;
            right = p[i + 1] / 100.0;

            if (text.Length != 1) {     // Ako ima još simbola
                EncodeAux(text.Substring(1), p, out double next_left, out double next_right);   // Kodirati ostale simbole, pa onda
                double delta = right - left;                                                    // dodatno ograničiti interval trenutnog 
                left += next_left * delta;                                                      // simbola tako da jednoznačno određuje
                right -= (1 - next_right) * delta;                                              // ostale simbole u poruci
            }

            return (left + right) / 2;   // Sredina intervala daje točan kodirani broj
        }

        // Potreban broj bitova za jednoznačno kodiranje zadane poruke, ako je poznat interval poruke
        public static int BitsForUnambiguity(double left, double right)
            => (int)Math.Ceiling(Math.Log(1 / (right - left), 2)) + 1;
    }
}
