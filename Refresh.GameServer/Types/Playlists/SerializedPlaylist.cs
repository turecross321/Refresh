using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types.Playlists;

[XmlType("playlist")]
[XmlRoot("playlist")]
public class SerializedPlaylist : IDataConvertableFrom<SerializedPlaylist, GamePlaylist>
{
    [XmlElement("id")]
    public int Id { get; set; }
    
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("description")]
    public string Description { get; set; }
    [XmlElement("icon")]
    public string Icon { get; set; }
    
    [XmlElement("location")]
    public GameLocation Location { get; set; }

    public static SerializedPlaylist? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null) 
            return null;
        
        return new SerializedPlaylist
        {
            Id = old.PlaylistId,
            Name = old.Name,
            Description = old.Description,
            Icon = old.Icon,
            Location = new GameLocation(old.LocationX, old.LocationY),
        };
    }

    public static IEnumerable<SerializedPlaylist> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext)
        => oldList.Select(p => FromOld(p, dataContext)!);
}