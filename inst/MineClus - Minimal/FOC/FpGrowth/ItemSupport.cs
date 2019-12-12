namespace FOC.FpGrowth
{
    internal class ItemSupport
    {
        public string Item { get; set; }
        public int Support { get; set; }

        public override string ToString()
        {
            return $"{Item}:{Support}";
        }
    }
}