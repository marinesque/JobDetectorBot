using HeadHunterGrabber.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadHunterGrabber.DataAccess.Context
{
	public class VacancyDbContext : DbContext
	{
		public DbSet<Vacancy> Vacancies { get; set; }

		public VacancyDbContext(DbContextOptions options) : base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Vacancy>();
		}
	}
}
