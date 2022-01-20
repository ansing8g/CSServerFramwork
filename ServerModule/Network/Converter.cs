using System;
using System.Text;
using System.Text.Json;

namespace ServerModule.Network
{
    public interface IConverter
    {
        public bool Serialize(object _packet_object, Type _packet_type, out byte[]? _packet_data);
        public bool Deserialize(byte[] _packet_data, Type _packet_type, out object? _packet);
        public bool Deserialize<PacketBase>(byte[] _packet_data, out PacketBase? _packet);
    }

    public class JsonConverter : IConverter
    {
        public bool Serialize(object _packet_object, Type _packet_type, out byte[]? _packet_data)
        {
            try
            {
                string json_data = JsonSerializer.Serialize(_packet_object, _packet_type);
                _packet_data = Encoding.UTF8.GetBytes(json_data);
            }
            catch(Exception)
            {
                _packet_data = null;
                return false;
            }

            return true;
        }

        public bool Deserialize(byte[] _packet_data, Type _packet_type, out object? _packet)
        {
            _packet = null;

            try
            {
                string json_data = Encoding.UTF8.GetString(_packet_data);
                _packet = JsonSerializer.Deserialize(json_data, _packet_type);
            }
            catch(Exception)
            {
                _packet = null;
                return false;
            }

            return true;
        }

        public bool Deserialize<PacketBase>(byte[] _packet_data, out PacketBase? _packet)
        {
            try
            {
                string json_data = Encoding.UTF8.GetString(_packet_data);
                _packet = JsonSerializer.Deserialize<PacketBase>(json_data);
            }
            catch(Exception)
            {
                _packet = default;
                return false;
            }

            return true;
        }
    }
}
