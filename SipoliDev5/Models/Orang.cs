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
    
    public partial class Orang
    {
        public Orang()
        {
            this.KelainanBawaan = new HashSet<KelainanBawaan>();
            this.RekamMedik = new HashSet<RekamMedik>();
            this.RiwayatPenyakit = new HashSet<RiwayatPenyakit>();
            this.RiwayatPenyakitKeluarga = new HashSet<RiwayatPenyakitKeluarga>();
            this.Kunjungan = new HashSet<Kunjungan>();
        }
    
        public int ID { get; set; }
        public string Nama { get; set; }
        public string TempatLahir { get; set; }
        public Nullable<int> TempatLahirID { get; set; }
        public System.DateTime TanggalLahir { get; set; }
        public byte JenisKelaminID { get; set; }
    
        public virtual ICollection<KelainanBawaan> KelainanBawaan { get; set; }
        public virtual ICollection<RekamMedik> RekamMedik { get; set; }
        public virtual ICollection<RiwayatPenyakit> RiwayatPenyakit { get; set; }
        public virtual ICollection<RiwayatPenyakitKeluarga> RiwayatPenyakitKeluarga { get; set; }
        public virtual Mahasiswa Mahasiswa { get; set; }
        public virtual KotaKabupaten KotaKabupaten { get; set; }
        public virtual Pegawai Pegawai { get; set; }
        public virtual ICollection<Kunjungan> Kunjungan { get; set; }
    }
}