//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SipoliDev5.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RumahSakit
    {
        public RumahSakit()
        {
            this.Rujukan = new HashSet<Rujukan>();
        }
    
        public int ID { get; set; }
        public string Nama { get; set; }
        public int KecamatanID { get; set; }
        public string NoTelepon { get; set; }
    
        public virtual Kecamatan Kecamatan { get; set; }
        public virtual ICollection<Rujukan> Rujukan { get; set; }
    }
}