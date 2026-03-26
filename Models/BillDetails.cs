namespace FCTournament.Models
{
    public class BillDetails
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public Bill? Bill { get; set; }
        public DateTime DatePay {  get; set; } = DateTime.Now;
        public float FeePaid { get; set; }
    }
}
