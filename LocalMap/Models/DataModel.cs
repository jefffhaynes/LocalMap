﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MapTest.Models
{
    [Table("images")]
    public class DataModel
    {
        [Column("tile_id")]
        public string TileId { get; set; }

        [Column("tile_data")]
        public byte[] Data { get; set; }
    }
}
