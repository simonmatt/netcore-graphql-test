using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace netcore_graphql_test
{
    [Table("Locations")]
    public class Location
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, MaxLength(5)]
        public string Code { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}