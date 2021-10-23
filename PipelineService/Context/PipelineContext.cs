using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PipelineService.Models.Pipeline;

namespace PipelineService.Context
{
	public class PipelineContext : DbContext
	{
		public DbSet<Pipeline> Pipelines { get; set; }
		public DbSet<NodeDoubleInput> DoubleInputNodes { get; set; }
		public DbSet<NodeFileInput> FileInputNodes { get; set; }
		public DbSet<NodeSingleInput> SingleInputNodes { get; set; }

		private string DbPath { get; set; }

		public PipelineContext(DbContextOptions<PipelineContext> options) : base(options)
		{
			const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			DbPath = $"pipeline.db";
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options) =>
			options.UseSqlite($"Data Source={DbPath}");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Node>()
				.Property(n => n.OperationConfiguration)
				.HasConversion(
					v => JsonConvert.SerializeObject(v),
					v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
		}
	}
}
