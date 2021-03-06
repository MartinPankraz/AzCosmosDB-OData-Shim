using System.ComponentModel.DataAnnotations;
using GenericODataWebAPI.Core;

namespace GenericODataWebAPI
{
  public class Sflight : IDocumentWithId
  {
    [Key]
    public string id { get; set; }
    public string carrid { get; set; }
    public string connid { get; set; }
    public string fldate { get; set; }
    public string planetype { get; set; }
    public System.Nullable<int> seatsmax { get; set; }
    public System.Nullable<int> seatsocc { get; set; }
  }

 
}