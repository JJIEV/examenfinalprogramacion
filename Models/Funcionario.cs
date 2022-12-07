using System.ComponentModel.DataAnnotations;

namespace api.examenfinal.Models
{
    public class Funcionario
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string Nombres { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string Apellidos { get; set; }

        [Required]
        [StringLength(10)]
        public string Documento { get; set; }

        [Required]
        public string Telefono { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        public string Ciudad { get; set; }

        [Required]
        public string Nacionalidad { get; set; }

        [Required]
        public string Cargo { get; set; }

        [Required]
        public int Antiguedad { get; set; }


    }
}
