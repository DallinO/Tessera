﻿using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Chapter
{
    public class ChapterEntity
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid BookId { get; set; }
    }

    
    public class ChapterDto
    {
        public string Title { get; set; }
        public string Description{ get; set; }
        public List<LeafDto> Contents { get; set; } = new List<LeafDto>();
    }


    public struct AddChapterRequest
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }


}