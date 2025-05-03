using System.Collections.Generic;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class WebtoonDetailViewModel
    {
        public WebtoonDetailDto WebtoonDetail { get; set; }
        public List<Comment> Comments { get; set; }
        public double AverageRating { get; set; }
        public int UserRating { get; set; }
    }
} 