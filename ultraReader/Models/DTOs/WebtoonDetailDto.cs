using System.Collections.Generic;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class WebtoonDetailDto
    {
        public WebtoonInfo WebtoonInfo { get; set; }
        public List<string> Chapters { get; set; }

        public static implicit operator WebtoonDetailDto(WebtoonDetails v)
        {
            throw new NotImplementedException();
        }
    }
} 