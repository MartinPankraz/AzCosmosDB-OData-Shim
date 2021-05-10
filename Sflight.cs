using System.ComponentModel.DataAnnotations;

namespace AzCosmosDB_OData_Shim
{
  public class Sflight
  {
    [Key]
    public string id { get; set; }
    public string carrid { get; set; }
    public string connid { get; set; }
    public string fldate { get; set; }
    public string planetype { get; set; }
    public int seatsmax { get; set; }
    public int seatsocc { get; set; }
  }
}