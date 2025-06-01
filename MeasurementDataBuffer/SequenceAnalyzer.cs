using System.Collections;

namespace MeasurementDataBuffer
{
    // Verbesserungspotenziale:
    // Performance durch Caching

    // Häufiger Zugriff auf Min, Max, Average ist teuer -> O(n).
    // Lösung: Diese Werte kumulativ berechnen und zwischenspeichern (lazy update oder bei jedem Add-Aufruf updaten).

    // Clear-Zugriffsschutz
    // Wenn Clear() aus einem UI-Thread ausgeführt wird, könnte es zu Race Conditions mit dem Timer führen.
    // Lösung: Zugriff auf InnerList synchronisieren.

    // Verwendung von double für Add(float)
    // Die Methode nimmt float entgegen, aber intern wird double verwendet. Das führt zu unnötigen Konvertierungen.
    // Lösung: Entweder Add(double) oder Konvertierung dokumentieren.

    // Unnötige Initialisierung in Konstruktor
    // InnerList wird beim Feld und nochmal im Konstruktor initialisiert.
    // Lösung: Nur im Konstruktor initialisieren.

    // Kein Schutz gegen ungültige Werte (NaN, +/- unendlich)
    // Lösung: In Add prüfen, ob value gültig ist (!double.IsNaN(value) usw.).

    public class SequenceAnalyzer : IDisposable
    {
        // Falsche Berechnung von Min
        // Funktioniert nicht, wenn ausschließlich positive Zahlen > 0 vorliegen.
        public double Min
        {
            get
            {
                double temp = 0;

                foreach (double v in InnerList)
                    if (v < temp)
                        temp = v;

                return temp;
            }
        }

        public event Action NewData;

        // Falsche Berechnung von Max
        // Funktioniet nicht, wenn ausschließlich negative Zahlen vorliegen.
        public double Max
        {
            get
            {
                double temp = 0;

                foreach (double v in InnerList)
                    if (v > temp)
                        temp = v;

                return temp;
            }
        }

        // Verwendung von ArrayList ist nicht typsicher (object), besser wäre List<double>
        // ArrayList ist nicht generisch und erlaubt Werte beliebigen Typs
        public ArrayList InnerList = new ArrayList();

        public double Last { get; set; }

        public double Average
        {
            get
            {
                double temp = 0;

                foreach (double v in InnerList)
                    temp += v;

                // Division durch 0 möglich in Average, wenn InnerList.Count == 0, dann temp / 0
                // Vor der Division prüfen, ob Count > 0 !
                return temp / InnerList.Count;
            }
        }

        // Es gibt keinerlei Synchronisation bei Zugriffen auf InnerList, obwohl Add durch einen Timer-Thread
        // und andere Mitglieder durch andere Threads aufgerufen werden können. Arbeiten mit lock() { }
        // oder ConcurrentQueue<double>
        //
        // Verwendung von double für Add(float)
        public void Add(float value)
        {
            Last = value;
            InnerList.Add(value);

            // Event NewData kann NullReferenceException werfen
            NewData();
        }

        public SequenceAnalyzer()
        {
            this.InnerList = new ArrayList();
        }

        // Unnötiger Finalizer(~SequenceAnalyzer)
        ~SequenceAnalyzer()
        {
            ((IDisposable)this).Dispose();
        }

        // Der Finalizer ruft Dispose auf, was Clear aufruft, aber kein unmanaged Resource Cleanup erfolgt.
        void IDisposable.Dispose()
        {
            Clear();
        }

        // Speicher freigeben mit InnerList.Clear();
        private void Clear()
        {
            InnerList = new ArrayList();
        }
    }
}
