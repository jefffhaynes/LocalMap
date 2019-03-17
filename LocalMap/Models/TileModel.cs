using System.ComponentModel.DataAnnotations.Schema;

namespace MapTest.Models
{
    [Table("map")]
    public class TileModel
    {
        [Column("grid_id")]
        public string GridId { get; set; }

        [Column("tile_id")]
        public string TileId { get; set; }

        [Column("zoom_level")]
        public int ZoomLevel { get; set; }

        [Column("tile_column")]
        public int Column { get; set; }

        [Column("tile_row")]
        public int Row { get; set; }

        public DataModel Data { get; set; }
    }
}
