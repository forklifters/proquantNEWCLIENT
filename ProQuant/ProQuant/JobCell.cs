using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ProQuant
{
    public class JobCell
    {
        public string JobNumber { get; set; }
        public string SubJobNumber { get; set; }
        public string Add1 { get; set; }
        public string Add2 { get; set; }
        public string Add3 { get; set; }
        public string Add4 { get; set; }
        public string AddPC { get; set; }
        public string Awarded { get; set; }
        public string Builder { get; set; }
        public string Notes { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string PO { get; set; }
        public Color JobColor { get; set; }
        public Color CellColor { get; set; }
        public Color StatusColor { get; set; }
        public Job job { get; set; }
        public Job subjob { get; set; }
        public int SentCount { get; set; }
        public double GrossValue { get; set; }
        public double NetValue { get; set; }
        public double VatValue { get; set; }
        public bool noSubs { get; set; } = true;

        public string NumberAndPart { get; set; }
    }
}
