using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models
{
    public class EditObjectViewModel
    {
        public Exception SaveException;
        public Int64 ID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        public String Notice { get; set; }
        public bool Disabled { get; set; }
    }
}
