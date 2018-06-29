using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompanyDatabase.Models
{
    public partial class CompanyDbContext : DbContext
    {
        public virtual DbSet<Companies> Companies { get; set; }
        public virtual DbSet<CompanySkills> CompanySkills { get; set; }
        public virtual DbSet<CompanyDetails> CompanyDetails { get; set; }
        public virtual DbSet<SkillDetail> SkillDetail { get; set; }
        public virtual DbSet<SkillSet> SkillSet { get; set; }
        public virtual DbSet<HomePage> HomePage { get; set; }
        public virtual DbSet<Focus> Focus { get; set; }
        public virtual DbSet<CompanyFocus> CompanyFocus { get; set; }
        public virtual DbSet<TemporaryCompanyDetails> TemporaryCompanyDetails { get; set; }
        public virtual DbSet<TemporaryCompanyFocus> TemporaryCompanyFocus { get; set; }
        public virtual DbSet<TemporaryCompanySkills> TemporaryCompanySkills { get; set; }
        public virtual DbSet<TemporaryCompanyTemplate> TemporaryCompanyTemplate { get; set; }
        public virtual DbSet<TreeNodes> TreeNodes { get; set; }
        public virtual DbSet<FocusNodes> FocusNodes { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }


        public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
