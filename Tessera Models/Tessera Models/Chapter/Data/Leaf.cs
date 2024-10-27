﻿namespace Tessera.Models.Chapter
{
    // Database Entity
    public abstract class LeafEntity
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class LeafModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    
    public abstract class LeafDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
