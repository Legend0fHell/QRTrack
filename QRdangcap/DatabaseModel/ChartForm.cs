namespace QRdangcap.DatabaseModel
{
    public class ChartForm
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Percentage { get; set; }

        public ChartForm(string name, double value, double totalValue)
        {
            Name = name;
            Value = value;
            Percentage = string.Format("{0:F2}", value * 100.0 / totalValue) + "%";
        }
    }
}